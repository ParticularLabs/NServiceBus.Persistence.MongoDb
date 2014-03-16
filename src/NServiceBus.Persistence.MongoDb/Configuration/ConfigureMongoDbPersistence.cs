using System;
using System.Configuration;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Repository;

namespace NServiceBus.Persistence.MongoDB.Configuration
{
    public static class MongoPersistenceConstants
    {
        public const string SubscriptionCollectionName = "subscriptions";
        public const string SagaCollectionName = "sagas";
        public const string SagaUniqueIdentityCollectionName = "saga_unique_ids";
    }

    public static class ConfigureMongoDbPersistence
    {
        public static Configure MongoDbPersistence(this Configure config, MongoServer server, MongoDatabase database)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (server == null) throw new ArgumentNullException("server");
            if (database == null) throw new ArgumentNullException("database");

            config.Configurer.RegisterSingleton<MongoDatabase>(database);
            config.Configurer.RegisterSingleton<MongoServer>(server);
            config.Configurer.RegisterSingleton<MongoDbRepository>(new MongoDbRepository(database));
            return config;
        }

        public static Configure MongoDbPersistence(this Configure config, string connectionStringName)
        {
            var connectionStringEntry = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionStringEntry == null)
            {
                throw new ConfigurationErrorsException(string.Format("Cannot configure Mongo Persister. No connection string named {0} was found", connectionStringName));
            }

            var connectionString = connectionStringEntry.ConnectionString;
            return MongoPersistenceWithConectionString(config, connectionString);
        }

        public static Configure MongoDbPersistence(this Configure config)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings["NServiceBus/Persistence/MongoDB"];

            //user existing config if we can find one
            if (connectionStringSetting != null)
            {
                return MongoPersistenceWithConectionString(config, connectionStringSetting.ConnectionString);
            }

            return MongoPersistenceWithConectionString(config, String.Format("mongodb://localhost/{0}", Configure.EndpointName));
        }

        public static Configure MongoPersistenceWithConectionString(Configure config, string connectionString)
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
        
        public static Configure MongoDbPersistence(this Configure config, Func<string> getConnectionString)
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
