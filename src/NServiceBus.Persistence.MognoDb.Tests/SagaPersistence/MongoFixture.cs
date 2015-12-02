using System;
using System.Configuration;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Sagas;
using NServiceBus.Saga;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class MongoFixture
    {
        private IMongoDatabase _database;
        private MongoDbSagaRepository _repo;
        private ISagaPersister _sagaPersister;
        private MongoClient _client;
        private bool _camelCaseConventionSet;
        private string _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

        [SetUp]
        public virtual void SetupContext()
        {

            var camelCasePack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCasePack, type => true);
            _camelCaseConventionSet = true;

            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(_databaseName);
            _repo = new MongoDbSagaRepository(_database);

            
            _sagaPersister = new SagaPersister(_repo);
        }

        protected ISagaPersister SagaPersister
        {
            get { return _sagaPersister; }
        }

        [TearDown]
        public void TeardownContext()
        {
            _client.DropDatabaseAsync(_databaseName).Wait();
        }

        protected void SaveSaga<T>(T saga) where T : IContainSagaData
        {
            _sagaPersister.Save(saga);
        }

        protected T LoadSaga<T>(Guid id) where T : IContainSagaData
        {
            return _sagaPersister.Get<T>(id);
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
            var versionName = _camelCaseConventionSet ? "version" : "Version";
            var collection = _database.GetCollection<BsonDocument>(typeof(T).Name.ToLower());

            collection.UpdateOneAsync(new BsonDocument("_id", sagaId), new BsonDocumentUpdateDefinition<BsonDocument>(
                new BsonDocument("$set", new BsonDocument(versionName, version))))
                .Wait();
        }
    }
}