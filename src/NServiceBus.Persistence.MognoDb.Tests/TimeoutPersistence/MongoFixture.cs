using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Timeout;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.TimeoutPersistence
{
    public class MongoFixture
    {
        private TimeoutPersister _storage;
        private IMongoDatabase _database;
        private MongoClient _client;
        private readonly string _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

        [SetUp]
        public void SetupContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(_databaseName);

            _storage = new TimeoutPersister(_database)
            {
                EndpointName = "MyTestEndpoint",
            };

            ((IWantToRunWhenBusStartsAndStops)_storage).Start();
        }

        protected TimeoutPersister Storage
        {
            get { return _storage; }
        }

        [TearDown]
        public void TeardownContext()
        {
            ((IWantToRunWhenBusStartsAndStops)_storage).Stop();
            _client.DropDatabaseAsync(_databaseName).Wait();
        }
    }
}