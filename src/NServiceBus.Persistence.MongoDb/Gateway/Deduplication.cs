using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Gateway.Deduplication;

namespace NServiceBus.Persistence.MongoDB.Gateway
{
    public class Deduplication : IDeduplicateMessages
    {
        private readonly MongoDatabase _database;
        private readonly MongoCollection<GatewayMessage> _collection;

        public Deduplication(MongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<GatewayMessage>("deduplication");
        }

        public bool DeduplicateMessage(string clientId, DateTime timeReceived)
        {
            try
            {
                _collection.Insert(new GatewayMessage()
                {
                    Id = clientId,
                    TimeReceived = timeReceived
                });
                
                return true;
            }
            catch (MongoDuplicateKeyException ex)
            {
                return false;
            }
        }
    }
}
