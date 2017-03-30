using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NServiceBus.Persistence.MongoDB.Database
{
    internal static class MongoIndexEnsurenceManager<T>
    {
        private const string UniquenessKey = "unique";
        private const string IndexName = "name";
        private const string BsonDocumentKey = "key";

        private enum IndexExistence
        {
            Create,
            Recreate,
            None
        }

        private delegate Task IndexExistenceDelegate(
            IMongoIndexManager<T> indexManager, BsonDocument key, CreateIndexOptions options,
            IEnumerable<BsonDocument> indexes);

        private static readonly Dictionary<IndexExistence, IndexExistenceDelegate> IndexExistenceHandlers = new Dictionary
            <IndexExistence, IndexExistenceDelegate>()
        {
            {IndexExistence.Create, OnNoneIndex},
            {IndexExistence.Recreate, OnDifferentOprionsIndex},
            {IndexExistence.None, (manager, index, options, indexes) => Task.FromResult(false)}
        };



        public static async Task EnsureIndex(IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options = null)
        {
            if (options == null)
                options = new CreateIndexOptions();
            var collectionIndexes = await indexManager.IndexesToList().ConfigureAwait(false);
            var existence = IndexExists(collectionIndexes, indexToEnsure, options);
            await IndexExistenceHandlers[existence].Invoke(indexManager, indexToEnsure, options, collectionIndexes).ConfigureAwait(false);
        }

        private static IndexExistence IndexExists(IEnumerable<BsonDocument> collectionIndexes, BsonDocument indexToEnsure,
            CreateIndexOptions options)
        {
            var existingIndex = collectionIndexes.FirstOrDefault(index =>
                index[BsonDocumentKey].Equals(indexToEnsure));

            //no such index key -> create index
            if (existingIndex == null)
                return IndexExistence.Create;

            //if nonunique index required, no matter unique/nonunique index was created before -> do nothing
            //if unique index required and current index options are the same -> do nothing
            if (!options.Unique.HasValue || !options.Unique.Value
                || collectionIndexes.Any(
                    index =>
                        index[BsonDocumentKey].Equals(indexToEnsure) &&
                        index.Contains(UniquenessKey)
                        && index[UniquenessKey].AsBoolean == true))
                return IndexExistence.None;

            //collection has index on same fields with different options with name by default -> drop and create index
            return IndexExistence.Recreate;
        }


        #region Existence handlers

        /// <summary>
        /// Handles Create index. Creates index.
        /// </summary>
        /// <param name="indexManager">Mongo index manager.</param>
        /// <param name="indexToEnsure">Index to recreate.</param>
        /// <param name="options">Options for creating an index.</param>
        /// <param name="collectionIndexes">Set of collection indexes.</param>
        /// <returns>Task to await index recreation.</returns>
        private static Task OnNoneIndex(IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options, IEnumerable<BsonDocument> collectionIndexes)
        {
            return indexManager.CreateOneAsync(new BsonDocumentIndexKeysDefinition<T>(indexToEnsure), options);
        }

        /// <summary>
        /// Handles DifferentOprionsIndex. Drops and recreates index because it was created with different options.
        /// </summary>
        /// <param name="indexManager">Mongo index manager.</param>
        /// <param name="indexToEnsure">Index to recreate.</param>
        /// <param name="options">Options for creating an index.</param>
        /// <param name="collectionIndexes">Set of collection indexes.</param>
        /// <returns>Task to await index recreation.</returns>
        private static async Task OnDifferentOprionsIndex(IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options, IEnumerable<BsonDocument> collectionIndexes)
        {
            // index can have custom name
            var indexName = collectionIndexes.First(index => index[BsonDocumentKey].Equals(indexToEnsure))[IndexName].ToString();

            await indexManager.DropOneAsync(indexName).ConfigureAwait(false);

            await
                indexManager.CreateOneAsync(new BsonDocumentIndexKeysDefinition<T>(indexToEnsure), options)
                    .ConfigureAwait(false);
        }

        #endregion

    }
}