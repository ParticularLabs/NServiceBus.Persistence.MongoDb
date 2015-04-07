using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Gateway.Deduplication;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB.Gateway
{
    
    public class Deduplication : IDeduplicateMessages
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<GatewayMessage> _collection;

        public Deduplication(IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<GatewayMessage>(MongoPersistenceConstants.DeduplicationCollectionName);
        }

        public bool DeduplicateMessage(string clientId, DateTime timeReceived)
        {
            try
            {
                _collection.WithWriteConcern(WriteConcern.W1).WithReadPreference(ReadPreference.Primary).InsertOneAsync(new GatewayMessage()
                {
                    Id = clientId,
                    TimeReceived = timeReceived
                }).Wait();
                
                return true;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.GetBaseException().GetType() == typeof (MongoWriteException))
                {
                    return false;
                }

                throw;
            }
        }
    }
}
