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
        private const string IndexNameKey = "name";
        private const string BsonDocumentKey = "key";

        private enum IndexEnsurenceActionType
        {
            Create,
            Recreate,
            None
        }

        private delegate Task IndexEnsurenceActionDelegate(
            IMongoIndexManager<T> indexManager, BsonDocument key, CreateIndexOptions options,
            BsonDocument existingIndex);

        private static readonly Dictionary<IndexEnsurenceActionType, IndexEnsurenceActionDelegate>
            IndexEnsurenceActionHandlers = new Dictionary
                <IndexEnsurenceActionType, IndexEnsurenceActionDelegate>()
            {
                {
                    IndexEnsurenceActionType.Create,
                    (manager, index, options, existingIndex) => CreateIndex(manager, index, options)
                },
                {IndexEnsurenceActionType.Recreate, DropAndCreateIndex},
                {IndexEnsurenceActionType.None, (manager, index, options, existingIndex) => Task.FromResult(false)}
            };
        
        public static async Task EnsureIndex(IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options = null)
        {
            if (options == null)
                options = new CreateIndexOptions();
            var collectionIndexes = await indexManager.IndexesToList().ConfigureAwait(false);
            var existingIndex = collectionIndexes.FirstOrDefault(index =>
                index[BsonDocumentKey].Equals(indexToEnsure));
            var action = GetIndexEnsurenceAction(existingIndex, options);
            await IndexEnsurenceActionHandlers[action].Invoke(indexManager, indexToEnsure, options, existingIndex).ConfigureAwait(false);
        }

        private static IndexEnsurenceActionType GetIndexEnsurenceAction(BsonDocument existingIndex,
            CreateIndexOptions options)
        {
            //no such index key -> create index
            if (existingIndex == null)
                return IndexEnsurenceActionType.Create;

            //if nonunique index required, no matter unique/nonunique index was created before -> do nothing
            //if unique index required and current index options are the same -> do nothing
            if (!options.Unique.HasValue || !options.Unique.Value
                || (existingIndex.Contains(UniquenessKey)
                    && existingIndex[UniquenessKey].AsBoolean == true))
                return IndexEnsurenceActionType.None;

            //collection has index on same fields with different options -> drop and create index
            return IndexEnsurenceActionType.Recreate;
        }


        #region Existence handlers

        /// <summary>
        /// Creates index.
        /// </summary>
        /// <param name="indexManager">Mongo index manager.</param>
        /// <param name="indexToEnsure">Index to recreate.</param>
        /// <param name="options">Options for creating an index.</param>
        /// <returns>Task to await index recreation.</returns>
        private static Task CreateIndex(IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options)
        {
            return indexManager.CreateOneAsync(new BsonDocumentIndexKeysDefinition<T>(indexToEnsure), options);
        }

        /// <summary>
        /// Drops and recreates index because it was created with different options.
        /// </summary>
        /// <param name="indexManager">Mongo index manager.</param>
        /// <param name="indexToEnsure">Index to recreate.</param>
        /// <param name="options">Options for creating an index.</param>
        /// <param name="existingIndex">Existing index.</param>
        /// <returns>Task to await index recreation.</returns>
        private static async Task DropAndCreateIndex(IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options, BsonDocument existingIndex)
        {
            var indexName = existingIndex[IndexNameKey].ToString();
            await indexManager.DropOneAsync(indexName).ConfigureAwait(false);

            await
                indexManager.CreateOneAsync(new BsonDocumentIndexKeysDefinition<T>(indexToEnsure), options)
                    .ConfigureAwait(false);
        }

        #endregion

    }
}