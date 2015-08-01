using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Saga;

namespace NServiceBus.Persistence.MongoDB.Sagas
{
    public class MongoDbSagaRepository : BaseNsbMongoDbRepository
    {
        public MongoDbSagaRepository(IMongoDatabase database)
            : base(database)
        {
        }

        public T FindById<T>(Guid id)
        {
            var doc = GetCollection<T>().Find(new QueryDocument("_id", id)).FirstOrDefaultAsync().Result;
            return Deserialize<T>(doc);
        }

        


        public T FindByFieldName<T>(string fieldName, object value)
        {
            var doc = GetCollection<T>().Find(new QueryDocument(fieldName, BsonValue.Create(value))).FirstOrDefaultAsync().Result;
            return Deserialize<T>(doc);
        }

        public void Update(IContainSagaData saga, string versionFieldName, int version)
        {
            var collection = GetCollection(saga.GetType());

            var fbuilder = Builders<BsonDocument>.Filter;
            var filter = fbuilder.Eq("_id", saga.Id) & fbuilder.Eq(versionFieldName, version);

            


            var bsonDoc = saga.ToBsonDocument();
            var ubuilder = Builders<BsonDocument>.Update;
            var update = ubuilder.Inc(versionFieldName, 1);

            foreach (var field in bsonDoc.Where(field => field.Name != versionFieldName && field.Name != "_id"))
            {
                update = update.Set(field.Name, field.Value);
            }


            var modifyResult = collection.FindOneAndUpdateAsync(
                filter, 
                update,
                new FindOneAndUpdateOptions<BsonDocument> {IsUpsert = false, ReturnDocument = ReturnDocument.After}).Result;

            if (modifyResult == null)
            {
                throw new SagaMongoDbConcurrentUpdateException(version);
            }
        }

        public void Remove(IContainSagaData saga)
        {
            var collection = GetCollection(saga.GetType());
            collection.DeleteOneAsync(new QueryDocument("_id", saga.Id)).Wait();
        }

        public void Insert(object entity)
        {
            var collection = GetCollection(entity.GetType());
            collection.InsertOneAsync(entity.ToBsonDocument()).Wait();
        }

    }
}