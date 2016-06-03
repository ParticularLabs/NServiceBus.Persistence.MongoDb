using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Persistence.MongoDB.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    public class MongoFixture
    {
        private SubscriptionPersister _storage;
        private IMongoDatabase _database;
        private MongoClient _client;
        private readonly string _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

        [SetUp]
        public void SetupContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(_databaseName);
            _storage = new SubscriptionPersister(_database);
        }

        protected SubscriptionPersister Storage => _storage;

        protected IMongoCollection<Subscription> Subscriptions => _database.GetCollection<Subscription>(MongoPersistenceConstants.SubscriptionCollectionName);

        [TearDown]
        public void TeardownContext() => _client.DropDatabase(_databaseName);
    }

    public static class Extentions
    {
        public static long Count<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return collection.Count(new BsonDocument());
        }
    }
}