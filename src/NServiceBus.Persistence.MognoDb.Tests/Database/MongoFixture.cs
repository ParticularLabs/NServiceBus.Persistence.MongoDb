using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Database;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Database
{
    public class MongoFixture
    {
        private const string MongoTestCollectionName = "MongoTestDocumentCollection";
        private IMongoCollection<MongoTestDocument> _storage;
        private IMongoDatabase _database;
        private MongoClient _client;
        private readonly string _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

        [SetUp]
        public void SetupContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(_databaseName);
            _storage = _database.GetCollection<MongoTestDocument>(MongoTestCollectionName);

            FillCollection();
        }

        protected IMongoCollection<MongoTestDocument> Storage => _storage;

        private void FillCollection()
        {
            _storage.InsertOne(new MongoTestDocument()
            {
                Property1 = Guid.NewGuid().ToString(),
                Property2 = Guid.NewGuid().ToString()
            });
        }

        [TearDown]
        public void TeardownContext() => _client.DropDatabase(_databaseName);
    }
}