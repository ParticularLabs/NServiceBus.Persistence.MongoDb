using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB
{
    public static class MongoDbSettingsExtensions
    {
        public static PersistenceExtentions<MongoDbPersistence> SetConnectionStringName(
            this PersistenceExtentions<MongoDbPersistence> cfg, string connectionStringName)
        {
            cfg.GetSettings().Set(MongoPersistenceSettings.ConnectionStringName, connectionStringName);
            return cfg;
        }

        public static PersistenceExtentions<MongoDbPersistence> SetConnectionString(
            this PersistenceExtentions<MongoDbPersistence> cfg, string connectionString)
        {
            cfg.GetSettings().Set(MongoPersistenceSettings.ConnectionString, connectionString);
            return cfg;
        }

        public static PersistenceExtentions<MongoDbPersistence> SetDatabaseName(
            this PersistenceExtentions<MongoDbPersistence> cfg, string databaseName)
        {
            cfg.GetSettings().Set(MongoPersistenceSettings.DatabaseName, databaseName);
            return cfg;
        }

    }
}
