using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NServiceBus.Persistence.MongoDB.Repository;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Persistence.MongoDB.TimeoutPersistence
{
    public class MongoDbTimeoutPersistence : IPersistTimeouts
    {
        private readonly MongoDatabase _database;
        private readonly MongoCollection<TimeoutEntity> _collection;

        public MongoDbTimeoutPersistence(MongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<TimeoutEntity>("timeouts");
        }


        public string EndpointName { get; set; }
        

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            var now = DateTime.UtcNow;

            var results = _collection
                .Find(Query.And(
                    Query<TimeoutEntity>.EQ(t => t.Endpoint, EndpointName),
                    Query<TimeoutEntity>.GTE(t => t.Time, startSlice),
                    Query<TimeoutEntity>.LTE(t => t.Time, now)))
                .SetFields(Fields<TimeoutEntity>.Include(t => t.Id).Include(t => t.Time))
                .SetSortOrder(SortBy<TimeoutEntity>.Ascending(t => t.Time))
                .Select(t => Tuple.Create(t.Id.ToString(), t.Time))
                .ToList();

            var startOfNextChunk = _collection
                .Find(Query.And(
                    Query<TimeoutEntity>.EQ(t => t.Endpoint, EndpointName),
                    Query<TimeoutEntity>.GTE(t => t.Time, now)))
                .SetFields(Fields<TimeoutEntity>.Exclude(t => t.Id).Include(t => t.Time))
                .SetSortOrder(SortBy<TimeoutEntity>.Ascending(t => t.Time))
                .SetLimit(1)
                .SingleOrDefault();

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
            if (timeout.Headers.TryGetValue(Headers.MessageId, out messageId))
            {
                Guid.TryParse(messageId, out timeoutId);
            }

            if (timeoutId == Guid.Empty)
            {
                timeoutId = GenerateCombGuid();
            }

            _collection.Insert(new TimeoutEntity
            {
                Id = timeoutId.ToString(),
                Destination = timeout.Destination,
                SagaId = timeout.SagaId,
                State = timeout.State,
                Time = timeout.Time,
                Headers = timeout.Headers,
                Endpoint = timeout.OwningTimeoutManager,
            });
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            var query = Query<TimeoutEntity>.EQ(t => t.Id, timeoutId);
            var entity = _collection.FindOne(query);

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

            if (_collection.Remove(query).DocumentsAffected == 0)
            {
                timeoutData = null;
                return false;
            }

            return true;
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            _collection.Remove(Query<TimeoutEntity>.EQ(t => t.SagaId, sagaId));
        }
        
        static Guid GenerateCombGuid()
        {
            var guidArray = Guid.NewGuid().ToByteArray();

            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.Now;

            // Get the days and milliseconds which will be used to build the byte string 
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var timeOfDay = now.TimeOfDay;

            // Convert to a byte array 
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
            var daysArray = BitConverter.GetBytes(days.Days);
            var millisecondArray = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysArray);
            Array.Reverse(millisecondArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(millisecondArray, millisecondArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
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
        public virtual Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Timeout endpoint name.
        /// </summary>
        public virtual string Endpoint { get; set; }
    }
}
