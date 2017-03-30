using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Database;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Database
{
    [TestFixture]
    public class When_ensuring_indexes :MongoFixture
    {
        private const string IndexName = "name";
        private const string BsonDocumentKey = "key";
        private const string UniquenessKey = "unique";
        private const string MongoIndexByDefault = "_id_";
        private readonly BsonDocument _testIndex = new BsonDocument { { nameof(MongoTestDocument.Property1), 1 } };
        private readonly BsonDocument _testCompositeIndex = new BsonDocument
        {
            {nameof(MongoTestDocument.Property1), 1},
            {nameof(MongoTestDocument.Property2), 1}
        };

        private List<BsonDocument> StorageIndexes => Storage.Indexes.IndexesToList().GetAwaiter().GetResult();

        [Test]
        public Task Should_return_custom_index()
        {
            var indexToCreate = new BsonDocument { { nameof(MongoTestDocument.Property2), 1 } };
            Storage.Indexes.CreateOne(new BsonDocumentIndexKeysDefinition<MongoTestDocument>(indexToCreate));
            var indexes = Storage.Indexes.IndexesToList().GetAwaiter().GetResult();

            //mongo collection always has _id_ index by default
            Assert.AreEqual(2, indexes.Count);
            Assert.AreEqual(indexToCreate, indexes.First(i=>i[IndexName] != MongoIndexByDefault)[BsonDocumentKey]);

            return Task.FromResult(false);
        }



        [Test]
        public Task Should_create_nonunique_index()
        {
            Storage.Indexes.EnsureIndex(_testIndex).Wait();

            var indexes = Storage.Indexes.IndexesToList().GetAwaiter().GetResult();
            Assert.AreEqual(2, indexes.Count);

            var createdIndex = indexes.First(i => i[IndexName] != MongoIndexByDefault);
            Assert.AreEqual(_testIndex, createdIndex[BsonDocumentKey]);
            Assert.IsFalse(createdIndex.Contains(UniquenessKey));

            return Task.FromResult(false);
        }

        [Test]
        public Task Should_not_create_index()
        {
            Storage.Indexes.CreateOne(new BsonDocumentIndexKeysDefinition<MongoTestDocument>(_testIndex),
                new CreateIndexOptions {Unique = true});
            Assert.AreEqual(2, StorageIndexes.Count);

            Storage.Indexes.EnsureIndex(_testIndex, new CreateIndexOptions { Unique = true }).Wait();
            Assert.AreEqual(2, StorageIndexes.Count);
            
            return Task.FromResult(false);
        }

        [Test]
        public Task Should_recreate_unique_index_on_same_field()
        {
            Storage.Indexes.CreateOne(new BsonDocumentIndexKeysDefinition<MongoTestDocument>(_testIndex));

            Storage.Indexes.EnsureIndex(_testIndex, new CreateIndexOptions {Unique = true}).Wait();
            
            Assert.AreEqual(2, StorageIndexes.Count);
            var createdIndex = StorageIndexes.First(i => i[IndexName] != MongoIndexByDefault);
            Assert.AreEqual(_testIndex, createdIndex[BsonDocumentKey]);
            Assert.IsTrue(createdIndex[UniquenessKey].ToBoolean());

            return Task.FromResult(false);
        }

        [Test]
        public Task Should_recreate_unique_index_on_same_field_with_default_name()
        {
            Storage.Indexes.CreateOne(new BsonDocumentIndexKeysDefinition<MongoTestDocument>(_testIndex),
                new CreateIndexOptions() {Name = "custom_index_name"});
            var indexNames = StorageIndexes.Select(index=>index[IndexName]);

            Storage.Indexes.EnsureIndex(_testIndex, new CreateIndexOptions { Unique = true }).Wait();
            
            Assert.AreEqual(2, StorageIndexes.Count);
            var createdIndex = StorageIndexes.First(i => !indexNames.Contains(i[IndexName]));
            Assert.AreEqual(_testIndex, createdIndex[BsonDocumentKey]);
            Assert.IsTrue(createdIndex[UniquenessKey].ToBoolean());

            return Task.FromResult(false);
        }

        [Test]
        public Task Should_create_composite_index()
        {
            Storage.Indexes.CreateOne(new BsonDocumentIndexKeysDefinition<MongoTestDocument>(_testIndex));
            var indexNames = StorageIndexes.Select(index => index[IndexName]);
            Storage.Indexes.EnsureIndex(_testCompositeIndex, new CreateIndexOptions { Unique = true }).Wait();

            Assert.AreEqual(3, StorageIndexes.Count);
            var createdIndex = StorageIndexes.First(i => !indexNames.Contains(i[IndexName]));
            Assert.AreEqual(_testCompositeIndex, createdIndex[BsonDocumentKey]);
            Assert.IsTrue(createdIndex[UniquenessKey].ToBoolean());

            return Task.FromResult(false);
        }
    }
}
