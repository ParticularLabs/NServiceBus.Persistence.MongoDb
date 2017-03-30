using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using NServiceBus.Extensibility;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Persistence.MongoDB.Timeout
{
    public class TimeoutPersister : IPersistTimeouts, IQueryTimeouts
    {
        private readonly string _endpointName;
        private readonly IMongoCollection<TimeoutEntity> _collection;
        private readonly IEnumerable<BsonDocument> _ensureIndexes = new[]
        {
            new BsonDocument {{nameof(TimeoutEntity.SagaId), 1}},
            new BsonDocument {{nameof(TimeoutEntity.Endpoint), 1}, {nameof(TimeoutEntity.Time), 1}}
        };

        public TimeoutPersister(string endpointName, IMongoDatabase database)
        {
            _endpointName = endpointName;
            _collection = database.GetCollection<TimeoutEntity>(MongoPersistenceConstants.TimeoutCollectionName)
                .WithReadPreference(ReadPreference.Primary)
                .WithWriteConcern(WriteConcern.WMajority);
        }

        /// <summary>
        /// Initializes persister.
        /// </summary>
        /// <remarks>Ensure necessary indexes exist.</remarks>
        public void Init()
        {
            foreach (var ensureIndex in _ensureIndexes)
            {
                _collection.Indexes.EnsureIndex(ensureIndex, new CreateIndexOptions() {Background = true}).Wait();
            }
        }

        public async Task<TimeoutsChunk> GetNextChunk(DateTime startSlice)
        {
            var now = DateTime.UtcNow;

            var rBuilder = Builders<TimeoutEntity>.Filter;
            var rQuery = rBuilder.Eq(t => t.Endpoint, _endpointName) &
                         rBuilder.Gte(t => t.Time, startSlice) &
                         rBuilder.Lte(t => t.Time, now);


            var results = await _collection
                .Find(rQuery)
                .Sort(Builders<TimeoutEntity>.Sort.Ascending(t => t.Time))
                .Project(t => new { t.Id, t.Time })
                .ToListAsync()
                .ConfigureAwait(false);


            var ncBuilder = Builders<TimeoutEntity>.Filter;
            var ncQuery = ncBuilder.Eq(t => t.Endpoint, _endpointName) &
                          ncBuilder.Gte(t => t.Time, now);

            var startOfNextChunkQry = _collection
                .Find(ncQuery)
                .Sort(Builders<TimeoutEntity>.Sort.Ascending(t => t.Time))
                .Limit(1)
                .Project(t => new { t.Time })
                .ToList();

            var startOfNextChunk = startOfNextChunkQry.SingleOrDefault();

            var nextTimeToRunQuery = startOfNextChunk?.Time ?? DateTime.UtcNow.AddMinutes(10);

            return new TimeoutsChunk(
                results.Select(x => new TimeoutsChunk.Timeout(x.Id, x.Time)).ToArray(),
                nextTimeToRunQuery);
        }

        public async Task Add(TimeoutData timeout, ContextBag context)
        {
            await _collection.InsertOneAsync(new TimeoutEntity
            {
                Id = CombGuidGenerator.Instance.NewCombGuid(Guid.NewGuid(), DateTime.UtcNow).ToString(),
                Destination = timeout.Destination,
                SagaId = timeout.SagaId,
                State = timeout.State,
                Time = timeout.Time,
                Headers = timeout.Headers,
                Endpoint = timeout.OwningTimeoutManager,
                OwningTimeoutManager = timeout.OwningTimeoutManager
            }).ConfigureAwait(false);
        }

        public async Task<bool> TryRemove(string timeoutId, ContextBag context)
        {
            var query = Builders<TimeoutEntity>.Filter.Eq(t => t.Id, timeoutId);
            var entity = _collection.Find(query).FirstOrDefault();

            if (entity == null)
            {
                return false;
            }

            return (await _collection.DeleteOneAsync(query).ConfigureAwait(false)).DeletedCount != 0;
        }

        public Task RemoveTimeoutBy(Guid sagaId, ContextBag context)
        {
            return _collection.DeleteManyAsync(t => t.SagaId == sagaId);
        }

        public async Task<TimeoutData> Peek(string timeoutId, ContextBag context)
        {
            var now = DateTime.UtcNow;

            var timeoutEntity = await _collection.FindOneAndUpdateAsync<TimeoutEntity, TimeoutEntity>(
                e => e.Id == timeoutId && (!e.LockDateTime.HasValue || e.LockDateTime.Value < now.AddSeconds(-10)),
                new UpdateDefinitionBuilder<TimeoutEntity>().Set(te => te.LockDateTime, now))
                    .ConfigureAwait(false);

            return timeoutEntity?.ToTimeoutData();
        }
    }
}
