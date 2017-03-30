using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using NServiceBus.Timeout.Core;

namespace NServiceBus.Persistence.MongoDB.Timeout
{
    /// <summary>
    /// MongoDB wrapper class for <see cref="TimeoutData"/>
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