using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Persistence.MongoDB.Sagas;
using NServiceBus.Saga;

namespace NServiceBus.Persistence.MongoDB.Database
{
    public class MongoDbSagaRepository : BaseNsbMongoDbRepository
    {
        public MongoDbSagaRepository(MongoDatabase database)
            : base(database)
        {
        }

        public T FindById<T>(Guid id)
        {
            var collection = Database.GetCollection(GetCollectionName(typeof(T)));
            return collection.FindOneAs<T>(Query.EQ("_id", id));
        }

        public T FindByFieldName<T>(string fieldName, object value)
        {
            var collection = Database.GetCollection(GetCollectionName(typeof(T)));
            return collection.FindOneAs<T>(Query.EQ(fieldName, BsonValue.Create(value)));
        }

        public void Update(IContainSagaData saga, string versionFieldName, int version)
        {
            var collection = Database.GetCollection(GetCollectionName(saga.GetType()));

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
            var collection = Database.GetCollection(GetCollectionName(saga.GetType()));
            collection.Remove(Query.EQ("_id", saga.Id));
        }

        public void Insert(object entity)
        {
            var collection = Database.GetCollection(GetCollectionName(entity.GetType()));
            collection.Insert(entity);
        }

    }

    public abstract class BaseNsbMongoDbRepository
    {
        protected MongoDatabase Database { get; private set; }

        protected BaseNsbMongoDbRepository(MongoDatabase database)
        {
            Database = database;
        }

        protected string GetCollectionName(Type entityType)
        {
            return entityType.Name.ToLower();
        }

        public void EnsureUniqueIndex(Type entityType, string fieldName)
        {
            var collection = Database.GetCollection(GetCollectionName(entityType));
            collection.CreateIndex(IndexKeys.Ascending(fieldName), IndexOptions.SetUnique(true));
        }
    }

    public static class NsbMongoDbExtensions
    {
        public static void EnsureIndex<T>(this MongoCollection<T> collection, params Expression<Func<T, object>>[] fields)
        {
            collection.CreateIndex(IndexKeys<T>.Ascending(fields));
        }
    }
}
