using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Persistence.MongoDB.Configuration;
using NServiceBus.Persistence.MongoDB.Repository;
using NServiceBus.Persistence.MongoDB.SagaPersistence;
using NServiceBus.Saga;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class MongoFixture
    {
        private MongoDatabase _database;
        private MongoDbRepository _repo;
        private ISagaPersister _sagaPersister;
        private MongoClient _client;

        [SetUp]
        public virtual void SetupContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _database = _client.GetServer().GetDatabase("Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            _repo = new MongoDbRepository(_database);
            _sagaPersister = new MongoDbSagaPersistence(_repo);
        }

        protected MongoCollection<BsonDocument> Sagas
        {
            get { return _database.GetCollection<BsonDocument>(MongoPersistenceConstants.SagaCollectionName); }
        }

        protected MongoDatabase Database
        {
            get { return _database; }
        }

        protected ISagaPersister SagaPersister
        {
            get { return _sagaPersister; }
        }

        [TearDown]
        public void TeardownContext()
        {
            _database.Drop();
        }

        protected void SaveSaga<T>(T saga) where T : IContainSagaData
        {
            _sagaPersister.Save(saga);
        }

        protected void CompleteSaga<T>(Guid sagaId) where T : IContainSagaData
        {
            var saga = _sagaPersister.Get<T>(sagaId);
            Assert.NotNull(saga);
            _sagaPersister.Complete(saga);
        }

        protected void UpdateSaga<T>(Guid sagaId, Action<T> update) where T : IContainSagaData
        {
            var saga = _sagaPersister.Get<T>(sagaId);
            Assert.NotNull(saga, "Could not update saga. Saga not found");
            update(saga);
            _sagaPersister.Update(saga);
        }

        protected void ChangeSagaVersionManually<T>(Guid sagaId, int version)  where T: IContainSagaData
        {
            var collection = _database.GetCollection(_repo.GetCollectionName(typeof(T)));
            collection.Update(Query.EQ("_id", sagaId), new UpdateBuilder().Set("Version", version));
        }
    }
}