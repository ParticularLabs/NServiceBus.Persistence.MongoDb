using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Persistence.MongoDB.Sagas;
using NServiceBus.Saga;

namespace NServiceBus.Persistence.MongoDB.Database
{
    public class MongoDbRepository
    {
        private readonly MongoDatabase _db;

        public MongoDbRepository(MongoDatabase db)
        {
            _db = db;
        }

        public T FindById<T>(Guid id)
        {
            var collection = _db.GetCollection(GetCollectionName(typeof(T)));
            return collection.FindOneAs<T>(Query.EQ("_id", id));
        }

        public T FindByFieldName<T>(string fieldName, object value)
        {
            
            var collection = _db.GetCollection(GetCollectionName(typeof(T)));
            return collection.FindOneAs<T>(Query.EQ(fieldName, BsonValue.Create(value)));
        }

        public void Insert(object entity)
        {
            var collection = _db.GetCollection(GetCollectionName(entity.GetType()));
            collection.Insert(entity);
        }

        public void Update(IContainSagaData saga, string versionFieldName, int version)
        {
            var collection = _db.GetCollection(GetCollectionName(saga.GetType()));

            var query = Query.And(Query.EQ("_id", saga.Id), Query.EQ(versionFieldName, version));

            var bsonDoc = saga.ToBsonDocument();
            var update = new UpdateBuilder().Inc(versionFieldName, 1);

            foreach (var field in bsonDoc.Where(field => field.Name != versionFieldName && field.Name != "_id"))
            {
                update.Set(field.Name, field.Value);
            }

            var modifyResult = collection.FindAndModify(new FindAndModifyArgs
                                                        {
                                                            Query = query,
                                                            Update = update,
                                                            SortBy = SortBy.Null,
                                                            VersionReturned = FindAndModifyDocumentVersion.Modified,
                                                            Upsert = false
                                                        });

            if (modifyResult.ModifiedDocument == null)
            {
                throw new SagaMongoDbConcurrentUpdateException(version);
            }
        }

        public void Remove(IContainSagaData saga)
        {
            var collection = _db.GetCollection(GetCollectionName(saga.GetType()));
            collection.Remove(Query.EQ("_id", saga.Id));
        }

        public void EnsureUniqueIndex(Type entityType, string fieldName)
        {
            var collection = _db.GetCollection(GetCollectionName(entityType));
            collection.EnsureIndex(new IndexKeysBuilder().Ascending(fieldName), IndexOptions.SetUnique(true));
        }
        
        public string GetCollectionName(Type entityType)
        {
            return entityType.Name.ToLower();
        }
    }
}
