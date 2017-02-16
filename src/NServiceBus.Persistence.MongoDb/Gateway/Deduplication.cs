﻿using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus.Extensibility;
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

        public async Task<bool> DeduplicateMessage(string clientId, DateTime timeReceived, ContextBag context)
        {
            try
            {
                await _collection.WithWriteConcern(WriteConcern.W1).WithReadPreference(ReadPreference.Primary).InsertOneAsync(new GatewayMessage()
                {
                    Id = clientId,
                    TimeReceived = timeReceived
                }).ConfigureAwait(false);
                
                return true;
            }
            catch (MongoWriteException aggEx)
            {
                // Check for "E11000 duplicate key error"
                // https://docs.mongodb.org/manual/reference/command/insert/
                if (aggEx.WriteError?.Code == 11000)
                {
                    return false;
                }

                throw;
            }
        }
    }
}
