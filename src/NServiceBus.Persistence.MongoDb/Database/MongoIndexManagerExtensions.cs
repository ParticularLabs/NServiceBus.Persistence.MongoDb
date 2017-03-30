using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NServiceBus.Persistence.MongoDB.Database
{
    internal static class MongoIndexManagerExtensions
    {
        public static Task EnsureIndex<T>(this IMongoIndexManager<T> indexManager, BsonDocument indexToEnsure,
            CreateIndexOptions options = null)
        {
            return MongoIndexEnsurenceManager<T>.EnsureIndex(indexManager, indexToEnsure, options);
        }

        public static async Task<List<BsonDocument>> IndexesToList<T>(this IMongoIndexManager<T> indexManager)
        {
            var indexesCursor = await indexManager.ListAsync().ConfigureAwait(false);
            var indexes = await indexesCursor.ToListAsync().ConfigureAwait(false);
            return indexes;
        }
    }
}