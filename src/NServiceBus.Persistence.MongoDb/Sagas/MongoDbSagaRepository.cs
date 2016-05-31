using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB.Sagas
{
    public class MongoDbSagaRepository : BaseNsbMongoDbRepository
    {
        public MongoDbSagaRepository(IMongoDatabase database)
            : base(database)
        {
        }

        public async Task<T> FindById<T>(Guid id)
        {
            var doc = await GetCollection<T>().Find(new BsonDocument("_id", id)).FirstOrDefaultAsync().ConfigureAwait(false);
            return Deserialize<T>(doc);
        }

        public async Task<T> FindByFieldName<T>(string fieldName, object value)
        {
            var doc = await GetCollection<T>().Find(new BsonDocument(fieldName, BsonValue.Create(value))).Limit(1).FirstOrDefaultAsync().ConfigureAwait(false);
            return Deserialize<T>(doc);
        }

        public async Task Update(IContainSagaData saga, string versionFieldName, int version)
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

            var modifyResult = await collection.FindOneAndUpdateAsync(
                filter, 
                update,
                new FindOneAndUpdateOptions<BsonDocument> {IsUpsert = false, ReturnDocument = ReturnDocument.After}).ConfigureAwait(false);

            if (modifyResult == null)
            {
                throw new SagaMongoDbConcurrentUpdateException(version);
            }
        }

        public Task Remove(IContainSagaData saga)
        {
            var collection = GetCollection(saga.GetType());
            return collection.DeleteOneAsync(new BsonDocument("_id", saga.Id));
        }

        public async Task Insert(object entity)
        {
            var collection = GetCollection(entity.GetType());
            await collection.InsertOneAsync(entity.ToBsonDocument()).ConfigureAwait(false);
        }
    }
}