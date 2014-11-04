using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Persistence.MongoDB.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    public class MongoFixture
    {
        private SubscriptionPersister _storage;
        private MongoDatabase _database;
        private MongoClient _client;

        [SetUp]
        public void SetupContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _database = _client.GetServer().GetDatabase("Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            _storage = new SubscriptionPersister(_database);
        }

        protected SubscriptionPersister Storage
        {
            get { return _storage; }
        }

        protected MongoCollection<Subscription> Subscriptions
        {
            get { return _database.GetCollection<Subscription>(MongoPersistenceConstants.SubscriptionCollectionName); }
        }

        protected MongoDatabase Database
        {
            get { return _database; }
        }

        [TearDown]
        public void TeardownContext()
        {
            _database.Drop();
        }
    }
}