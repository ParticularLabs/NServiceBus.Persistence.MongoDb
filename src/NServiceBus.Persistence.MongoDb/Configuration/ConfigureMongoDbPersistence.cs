using System;
using System.Configuration;
using MongoDB.Driver;
using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Repository;

namespace NServiceBus.Persistence.MongoDB.Configuration
{
    public static class MongoPersistenceConstants
    {
        public const string SubscriptionCollectionName = "subscriptions";
        public const string SagaCollectionName = "sagas";
        public const string SagaUniqueIdentityCollectionName = "saga_unique_ids";
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
            context.Container.MongoDbPersistence();
        }
    }

    internal static class ConfigureMongoDbPersistence
    {
        public static ObjectBuilder.IConfigureComponents MongoDbPersistence(this ObjectBuilder.IConfigureComponents config, MongoServer server, MongoDatabase database)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (server == null) throw new ArgumentNullException("server");
            if (database == null) throw new ArgumentNullException("database");

            config.RegisterSingleton(database);
            config.RegisterSingleton(server);
            config.RegisterSingleton(new MongoDbRepository(database));
            return config;
        }

        public static ObjectBuilder.IConfigureComponents MongoDbPersistence(this ObjectBuilder.IConfigureComponents config, string connectionStringName)
        {
            var connectionStringEntry = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionStringEntry == null)
            {
                throw new ConfigurationErrorsException(string.Format("Cannot configure Mongo Persister. No connection string named {0} was found", connectionStringName));
            }

            var connectionString = connectionStringEntry.ConnectionString;
            return MongoPersistenceWithConectionString(config, connectionString);
        }

        public static ObjectBuilder.IConfigureComponents MongoDbPersistence(this ObjectBuilder.IConfigureComponents config)
        {
            return MongoDbPersistence(config, "NServiceBus/Persistence/MongoDB");
        }

        public static ObjectBuilder.IConfigureComponents MongoPersistenceWithConectionString(ObjectBuilder.IConfigureComponents config, string connectionString)
        {
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            if (String.IsNullOrWhiteSpace(databaseName))
            {
                throw new ConfigurationErrorsException("Cannot configure Mongo Persister. Database name not present in the connection string.");
            }

            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);


            return MongoDbPersistence(config, server, database);
        }

        public static ObjectBuilder.IConfigureComponents MongoDbPersistence(this ObjectBuilder.IConfigureComponents config, Func<string> getConnectionString)
        {
            var connectionString = getConnectionString();

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("Cannot configure Mongo Persister. No connection string was found");
            }
            
            return MongoPersistenceWithConectionString(config, connectionString);
        }
    }
}
