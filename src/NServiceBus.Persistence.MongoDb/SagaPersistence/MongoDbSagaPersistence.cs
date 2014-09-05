using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using NServiceBus.Persistence.MongoDB.Repository;
using NServiceBus.Saga;

namespace NServiceBus.Persistence.MongoDB.SagaPersistence
{
    public class MongoDbSagaPersistence : ISagaPersister
    {
        private readonly MongoDbRepository _repo;

        public MongoDbSagaPersistence(MongoDbRepository repo)
        {
            _repo = repo;
        }

        public void Save(IContainSagaData saga)
        {
            SetInitialVersion(saga);
            EnsureUniqueIndex(saga);

            _repo.Insert(saga);
        }

        private void EnsureUniqueIndex(IContainSagaData saga)
        {
            var sagaDataType = saga.GetType();
            var uniqueProperty = UniqueAttribute.GetUniqueProperty(sagaDataType);

            if (uniqueProperty == null)
            {
                return;
            }

            var classmap = BsonClassMap.LookupClassMap(sagaDataType);
            var membermap = classmap.GetMemberMap(uniqueProperty.Name);
            var uniqueFieldName = membermap.ElementName;

            _repo.EnsureUniqueIndex(sagaDataType, uniqueFieldName);
        }

        private static void SetInitialVersion(IContainSagaData saga)
        {
            var versionProperty = DocumentVersionAttribute.GetDocumentVersionProperty(saga.GetType());
            versionProperty.SetValue(saga, 0);
        }

        public void Update(IContainSagaData saga)
        {
            var sagaDataType = saga.GetType();
            var versionProperty = DocumentVersionAttribute.GetDocumentVersionProperty(sagaDataType);
            var version = (int)versionProperty.GetValue(saga);

            var classmap = BsonClassMap.LookupClassMap(sagaDataType);
            var membermap = classmap.GetMemberMap(versionProperty.Name);
            var versionFieldName = membermap.ElementName;


            _repo.Update(saga, versionFieldName, version);
        }

        public T Get<T>(Guid sagaId) where T : IContainSagaData
        {
            return _repo.FindById<T>(sagaId);
        }

        public T Get<T>(string property, object value) where T : IContainSagaData
        {
            var classmap = BsonClassMap.LookupClassMap(typeof(T));
            var membermap = classmap.GetMemberMap(property);
            var propertyFieldName = membermap.ElementName;

            var result = _repo.FindByFieldName<T>(propertyFieldName, value);
            return result;
        }

        public void Complete(IContainSagaData saga)
        {
            _repo.Remove(saga);
        }
    }
}
