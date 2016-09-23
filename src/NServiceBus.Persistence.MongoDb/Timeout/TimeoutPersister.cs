using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using NServiceBus.Extensibility;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Persistence.MongoDB.Timeout
{
    public class TimeoutPersister : IPersistTimeouts, IQueryTimeouts
    {
        private readonly string _endpointName;
        private readonly IMongoCollection<TimeoutEntity> _collection;
        private static bool _indexCreated = false;

        public TimeoutPersister(string endpointName, IMongoDatabase database)
        {
            _endpointName = endpointName;
            _collection = database.GetCollection<TimeoutEntity>("timeouts")
                .WithReadPreference(ReadPreference.Primary)
                .WithWriteConcern(WriteConcern.WMajority);
        }

        private async Task EnsureIndex()
        {
            if (!_indexCreated)
            {
                //no locking - if it runs more than once it's okay - performance is higher priority
                _indexCreated = true;
                await _collection.Indexes.CreateOneAsync(
                    Builders<TimeoutEntity>.IndexKeys.Ascending(t => t.SagaId),
                    new CreateIndexOptions { Background = true }).ConfigureAwait(false);

                await _collection.Indexes.CreateOneAsync(
                    Builders<TimeoutEntity>.IndexKeys.Ascending(t => t.Endpoint).Ascending(t => t.Time), 
                    new CreateIndexOptions { Background = true }).ConfigureAwait(false);
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
            await EnsureIndex().ConfigureAwait(false);
            
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
            var query =  Builders<TimeoutEntity>.Filter.Eq(t => t.Id, timeoutId);
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
    
    /// <summary>
    /// NHibernate wrapper class for <see cref="TimeoutData"/>
    /// </summary>
    public class TimeoutEntity
    {
        /// <summary>
        /// Id of this timeout.
        /// </summary>
        public virtual string Id { get; set; }

        //TODO: Breaking change NSB v5 to v6 - was type NServiceBus.Address
        /// <summary>
        /// The address of the client who requested the timeout.
        /// </summary>
        public virtual string Destination { get; set; }

        /// <summary>
        /// The saga ID.
        /// </summary>
        public virtual Guid SagaId { get; set; }

        /// <summary>
        /// Additional state.
        /// </summary>
        public virtual byte[] State { get; set; }

        /// <summary>
        /// The time at which the saga ID expired.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// Store the headers to preserve them across timeouts.
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public virtual Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Timeout endpoint name.
        /// </summary>
        public virtual string Endpoint { get; set; }

        /// <summary>
        ///     The timeout manager that owns this particular timeout
        /// </summary>
        public string OwningTimeoutManager { get; set; }

        /// <summary>
        /// The time when the timeout record was locked. If null then the record has not been locked.
        /// </summary>
        /// <remarks>
        /// Timeout locks are only considered valid for 10 seconds, therefore if the LockDateTime is older than 10 seconds it is no longer valid.
        /// </remarks>
        public DateTime? LockDateTime { get; set; }

        public TimeoutData ToTimeoutData()
        {
            return new TimeoutData
            {
                Destination = Destination,
                Headers = Headers,
                OwningTimeoutManager = OwningTimeoutManager,
                SagaId = SagaId,
                State = State,
                Time = Time
            };
        }
    }
}
