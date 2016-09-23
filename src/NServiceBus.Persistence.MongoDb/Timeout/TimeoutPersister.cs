using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Persistence.MongoDB.Timeout
{
    public class TimeoutPersister : IPersistTimeouts, IPersistTimeoutsV2, IWantToRunWhenBusStartsAndStops
    {
        private readonly IMongoCollection<TimeoutEntity> _collection;
        
        public string EndpointName { get; set; }

        public TimeoutPersister(IMongoDatabase database)
        {
            _collection = database.GetCollection<TimeoutEntity>("timeouts");
        }


        void IWantToRunWhenBusStartsAndStops.Start()
        {
            _collection.Indexes.CreateOne(Builders<TimeoutEntity>.IndexKeys.Ascending(t => t.SagaId));
        }

        void IWantToRunWhenBusStartsAndStops.Stop()
        {

        }

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            var now = DateTime.UtcNow;

            var rBuilder = Builders<TimeoutEntity>.Filter;
            var rQuery = rBuilder.Eq(t => t.Endpoint, EndpointName) &
                         rBuilder.Gte(t => t.Time, startSlice) &
                         rBuilder.Lte(t => t.Time, now);


            var results = _collection
                .Find(rQuery)
                .Sort(Builders<TimeoutEntity>.Sort.Ascending(t => t.Time))
                .Project(t => new { t.Id, t.Time })
                .ToList()
                .Select(t => Tuple.Create(t.Id.ToString(), t.Time))
                .ToList();

            var ncBuilder = Builders<TimeoutEntity>.Filter;
            var ncQuery = ncBuilder.Eq(t => t.Endpoint, EndpointName) &
                          ncBuilder.Gte(t => t.Time, now);

            var startOfNextChunkQry = _collection
                .Find(ncQuery)
                .Sort(Builders<TimeoutEntity>.Sort.Ascending(t => t.Time))
                .Limit(1)
                .Project(t => new { t.Time })
                .ToList();

            var startOfNextChunk = startOfNextChunkQry.SingleOrDefault();

            if (startOfNextChunk != null)
            {
                nextTimeToRunQuery = startOfNextChunk.Time;
            }
            else
            {
                nextTimeToRunQuery = DateTime.UtcNow.AddMinutes(10);
            }

            return results;
        }

        public void Add(TimeoutData timeout)
        {
            var timeoutId = Guid.Empty;

            string messageId;
            if (timeout.Headers != null && timeout.Headers.TryGetValue(Headers.MessageId, out messageId))
            {
                Guid.TryParse(messageId, out timeoutId);
            }

            if (timeoutId == Guid.Empty)
            {
                timeoutId = CombGuidGenerator.Instance.NewCombGuid(Guid.NewGuid(), DateTime.UtcNow);
            }

            _collection.InsertOne(new TimeoutEntity
            {
                Id = timeoutId.ToString(),
                Destination = timeout.Destination,
                SagaId = timeout.SagaId,
                State = timeout.State,
                Time = timeout.Time,
                Headers = timeout.Headers,
                Endpoint = timeout.OwningTimeoutManager,
                OwningTimeoutManager = timeout.OwningTimeoutManager
            });
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            var query =  Builders<TimeoutEntity>.Filter.Eq(t => t.Id, timeoutId);
            var entity = _collection.Find(query).FirstOrDefault();

            if (entity == null)
            {
                timeoutData = null;
                return false;
            }

            timeoutData = new TimeoutData
            {
                Destination = entity.Destination,
                Id = entity.Id,
                SagaId = entity.SagaId,
                State = entity.State,
                Time = entity.Time,
                Headers = entity.Headers,
            };

            if (_collection.DeleteOne(query).DeletedCount == 0)
            {
                timeoutData = null;
                return false;
            }

            return true;
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            _collection.DeleteMany(t => t.SagaId == sagaId);
        }
        
        public bool TryRemove(string timeoutId)
        {
            TimeoutData timeoutData;
            return TryRemove(timeoutId, out timeoutData);
        }
        public TimeoutData Peek(string timeoutId)
        {
            var now = DateTime.UtcNow;
            var update = new UpdateDefinitionBuilder<TimeoutEntity>().Set(te => te.LockDateTime, now);
            var timeoutEntity = _collection.FindOneAndUpdate<TimeoutEntity, TimeoutEntity>(e => e.Id == timeoutId && (!e.LockDateTime.HasValue || e.LockDateTime.Value < now.AddSeconds(-10)), update);
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

        /// <summary>
        /// The address of the client who requested the timeout.
        /// </summary>
        public virtual Address Destination { get; set; }

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
