using System;
using System.Linq;
using MongoDB.Bson.Serialization;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Saga;

namespace NServiceBus.Persistence.MongoDB.Sagas
{
    public class SagaPersister : ISagaPersister
    {
        private readonly MongoDbSagaRepository _repo;

        public SagaPersister(MongoDbSagaRepository repo)
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
            var uniqueFieldName = GetFieldName(classmap, uniqueProperty.Name);

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
            var versionFieldName = GetFieldName(classmap, versionProperty.Name);

            _repo.Update(saga, versionFieldName, version);
        }

        public T Get<T>(Guid sagaId) where T : IContainSagaData
        {
            return _repo.FindById<T>(sagaId);
        }

        public T Get<T>(string property, object value) where T : IContainSagaData
        {
            var classmap = BsonClassMap.LookupClassMap(typeof(T));
            var propertyFieldName = GetFieldName(classmap, property);

            var result = _repo.FindByFieldName<T>(propertyFieldName, value);
            return result;
        }

        public void Complete(IContainSagaData saga)
        {
            _repo.Remove(saga);
        }

        private string GetFieldName(BsonClassMap classMap, string property)
        {
            var element = classMap.AllMemberMaps.First(m => m.MemberName == property);
            return element.ElementName;
        }
    }
}
