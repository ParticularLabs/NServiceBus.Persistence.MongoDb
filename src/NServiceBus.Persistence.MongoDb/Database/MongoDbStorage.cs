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
        public const string DatabaseName = "MongoDbDatabaseName";
    }

    public static class MongoPersistenceConnectionStringNames
    {
        public const string DefaultConnectionStringName = "NServiceBus/Persistence/MongoDB";
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
            string connectionString = GetConnectionString(context);

            if (context.Settings.HasSetting(MongoPersistenceSettings.DatabaseName))
            {
                context.Container.MongoPersistence(connectionString, context.Settings.Get<string>(MongoPersistenceSettings.DatabaseName));
            }
            else
            {
                context.Container.MongoPersistence(connectionString);
            }
        }

        private static string GetConnectionString(FeatureConfigurationContext context)
        {
            string connectionString;
            if (context.Settings.HasSetting(MongoPersistenceSettings.ConnectionStringName))
            {
                connectionString = GetConnectionStringByName(context.Settings.Get<string>(MongoPersistenceSettings.ConnectionStringName));
            }
            else if (context.Settings.HasSetting(MongoPersistenceSettings.ConnectionString))
            {
                connectionString = context.Settings.Get<string>(MongoPersistenceSettings.ConnectionString);
                if (String.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationErrorsException("Cannot configure Mongo Persister. No connection string was found");
                }
            }
            else
            {
                connectionString = GetConnectionStringByName(MongoPersistenceConnectionStringNames.DefaultConnectionStringName);
            }

            return connectionString;
        }

        private static string GetConnectionStringByName(string connectionStringName)
        {
            var connectionStringEntry = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionStringEntry == null)
            {
                throw new ConfigurationErrorsException(string.Format("Cannot configure Mongo Persister. No connection string named {0} was found", connectionStringName));
            }

            return connectionStringEntry.ConnectionString;
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

        public static ObjectBuilder.IConfigureComponents MongoPersistence(this ObjectBuilder.IConfigureComponents config, string connectionString, string databaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
            {
                throw new ConfigurationErrorsException("Cannot configure Mongo Persister. No database name was found");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            return MongoDbPersistence(config, database);
        }

        public static ObjectBuilder.IConfigureComponents MongoPersistence(this ObjectBuilder.IConfigureComponents config, string connectionString)
        {
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ConfigurationErrorsException("Cannot configure Mongo Persister. Database name not present in the connection string.");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            return MongoDbPersistence(config, database);
        }
    }
}
