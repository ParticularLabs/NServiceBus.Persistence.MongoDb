using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using NServiceBus.Extensibility;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Sagas;

namespace NServiceBus.Persistence.MongoDB.Sagas
{
    public class SagaPersister : ISagaPersister
    {
        private readonly MongoDbSagaRepository _repo;

        public SagaPersister(MongoDbSagaRepository repo)
        {
            _repo = repo;
        }

        public async Task Save(IContainSagaData sagaData, SagaCorrelationProperty correlationProperty, SynchronizedStorageSession session, ContextBag context)
        {
            DocumentVersionAttribute.SetPropertyValue(sagaData, 0);
            await EnsureUniqueIndex(sagaData.GetType(), correlationProperty.Name).ConfigureAwait(false);

            await _repo.Insert(sagaData).ConfigureAwait(false);
        }

        private async Task EnsureUniqueIndex(Type sagaDataType, string propertyName)
        {
            if (propertyName == null)
            {
                return;
            }
            
            var classmap = BsonClassMap.LookupClassMap(sagaDataType);
            var uniqueFieldName = GetFieldName(classmap, propertyName);

            await _repo.EnsureUniqueIndex(sagaDataType, uniqueFieldName).ConfigureAwait(false);
        }

        public Task Update(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            var versionProperty = DocumentVersionAttribute.GetProperty(sagaData);

            var classmap = BsonClassMap.LookupClassMap(sagaData.GetType());
            var versionFieldName = GetFieldName(classmap, versionProperty.Key);

            return _repo.Update(sagaData, versionFieldName, versionProperty.Value);
        }

        public Task<TSagaData> Get<TSagaData>(Guid sagaId, SynchronizedStorageSession session, ContextBag context) where TSagaData : IContainSagaData
        {
            return _repo.FindById<TSagaData>(sagaId);
        }

        public async Task<TSagaData> Get<TSagaData>(string propertyName, object propertyValue, SynchronizedStorageSession session, ContextBag context) where TSagaData : IContainSagaData
        {
            var classmap = BsonClassMap.LookupClassMap(typeof(TSagaData));
            var propertyFieldName = GetFieldName(classmap, propertyName);

            var result = await _repo.FindByFieldName<TSagaData>(propertyFieldName, propertyValue).ConfigureAwait(false);
            return result;
        }

        public Task Complete(IContainSagaData sagaData, SynchronizedStorageSession session, ContextBag context)
        {
            return _repo.Remove(sagaData);
        }

        private string GetFieldName(BsonClassMap classMap, string property)
        {
            var element = classMap.AllMemberMaps.First(m => m.MemberName == property);
            return element.ElementName;
        }
    }
}
