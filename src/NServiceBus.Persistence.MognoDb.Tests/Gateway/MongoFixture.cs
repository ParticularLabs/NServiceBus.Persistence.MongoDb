using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Driver;
using NServiceBus.Gateway.Deduplication;
using NServiceBus.Persistence.MongoDB.Gateway;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Gateway
{
    public class MongorFixture
    {
        private IDeduplicateMessages _deduplication;
        private IMongoDatabase _database;
        private MongoClient _client;
        private readonly string _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

        [SetUp]
        public void SetupContext()
        {
            var connectionString = "mongodb://localhost/NServiceBus-Persistence-MognoDb-Tests"; // hardcoded for now

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(_databaseName);
            _deduplication = new Deduplication(_database);
        }

        protected IDeduplicateMessages Deduplication => _deduplication;

        [TearDown]
        public void TeardownContext() => _client.DropDatabase(_databaseName);
    }
}