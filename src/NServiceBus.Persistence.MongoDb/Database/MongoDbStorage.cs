using System;
using System.Configuration;
using MongoDB.Driver;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;

namespace NServiceBus.Persistence.MongoDB.Database
{
    public static class MongoPersistenceConstants
    {
        public const string SubscriptionCollectionName = "subscriptions";
        public const string DeduplicationCollectionName = "deduplication";
        public const string SagaUniqueIdentityCollectionName = "saga_unique_ids";
    }

    public static class MongoPersistenceSettings
    {
        public const string ConnectionStringName = "MongoDbConnectionStringName";
        public const string ConnectionString = "MongoDbConnectionString";

    }

    public class MongoDbStorage : Feature
    {
        internal MongoDbStorage()
        {
        }

        /// <summary>
        /// Called when the feature should perform its initialization. This call will only happen if the feature is enabled.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            if (context.Settings.HasSetting(MongoPersistenceSettings.ConnectionString))
            {
                context.Container.MongoDbPersistence(() => context.Settings.Get<string>(MongoPersistenceSettings.ConnectionString));
            }
            else
            {
                throw new Exception("Cannot configure Mongo Persister. No connection string was found. Connection string is a mandatory setting.");
            }
        }
    }

    internal static class ConfigureMongoDbPersistence
    {
        public static IConfigureComponents MongoDbPersistence(this IConfigureComponents config, IMongoDatabase database)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (database == null) throw new ArgumentNullException(nameof(database));

            config.RegisterSingleton(database);


            return config;
        }

        public static IConfigureComponents MongoPersistenceWithConectionString(IConfigureComponents config, string connectionString)
        {
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            if (String.IsNullOrWhiteSpace(databaseName))
            {
                throw new Exception("Cannot configure Mongo Persister. Database name not present in the connection string.");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            return MongoDbPersistence(config, database);
        }

        public static IConfigureComponents MongoDbPersistence(this IConfigureComponents config, Func<string> getConnectionString)
        {
            var connectionString = getConnectionString();

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Cannot configure Mongo Persister. No connection string was found");
            }

            return MongoPersistenceWithConectionString(config, connectionString);
        }
    }
}