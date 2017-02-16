﻿using System;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Sagas;
using NServiceBus.Sagas;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    [TestFixture]
    public class MongoFixture
    {
        private IMongoDatabase _database;
        private MongoDbSagaRepository _repo;
        private ISagaPersister _sagaPersister;
        private MongoClient _client;
        private bool _camelCaseConventionSet;
        private readonly string _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

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

        [TearDown]
        public void TeardownContext() => _client.DropDatabase(_databaseName);

        protected Task SaveSaga<T>(T saga) where T : IContainSagaData
        {
            SagaCorrelationProperty correlationProperty = null;

            if(saga.GetType() == typeof(SagaWithUniqueProperty))
            {
                correlationProperty = new SagaCorrelationProperty("UniqueString", String.Empty);
            }

            return _sagaPersister.Save(saga, correlationProperty, null, null );
        }

        protected Task<T> LoadSaga<T>(Guid id) where T : IContainSagaData
        {
            return _sagaPersister.Get<T>(id, null, null);
        }

        protected async Task CompleteSaga<T>(Guid sagaId) where T : IContainSagaData
        {
            var saga = await LoadSaga<T>(sagaId).ConfigureAwait(false);
            Assert.NotNull(saga);
            await _sagaPersister.Complete(saga, null, null).ConfigureAwait(false);
        }

        protected async Task UpdateSaga<T>(Guid sagaId, Action<T> update) where T : IContainSagaData
        {
            var saga = await LoadSaga<T>(sagaId).ConfigureAwait(false);
            Assert.NotNull(saga, "Could not update saga. Saga not found");
            update(saga);
            await _sagaPersister.Update(saga, null, null).ConfigureAwait(false);
        }

        protected void ChangeSagaVersionManually<T>(Guid sagaId, int version)  where T: IContainSagaData
        {
            var versionName = _camelCaseConventionSet ? "version" : "Version";
            var collection = _database.GetCollection<BsonDocument>(typeof(T).Name.ToLower());

            collection.UpdateOne(new BsonDocument("_id", sagaId), new BsonDocumentUpdateDefinition<BsonDocument>(
                new BsonDocument("$set", new BsonDocument(versionName, version))));
        }
    }
}