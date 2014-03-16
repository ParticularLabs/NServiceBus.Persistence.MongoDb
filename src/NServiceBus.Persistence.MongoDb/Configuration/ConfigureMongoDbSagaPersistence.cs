using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Repository;
using NServiceBus.Persistence.MongoDB.SagaPersistence;

namespace NServiceBus.Persistence.MongoDB.Configuration
{
    public static class ConfigureMongoDbSagaPersistence
    {
        public static Configure MongoDbSagaPersister(this Configure config)
        {
            if (!config.Configurer.HasComponent<MongoDbRepository>())
                config.MongoDbPersistence();

            config.Configurer.ConfigureComponent<MongoDbSagaPersistence>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
