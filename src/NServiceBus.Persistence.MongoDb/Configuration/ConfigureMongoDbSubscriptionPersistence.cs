using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Persistence.MongoDB.SubscriptionPersistence;

namespace NServiceBus.Persistence.MongoDB.Configuration
{
    public static class ConfigureMongoDbSubscriptionPersistence
    {
        public static Configure MongoDbSubscriptionStorage(this Configure config)
        {
            config.Configurer.ConfigureComponent<MongoDbSubscriptionPersistence>(DependencyLifecycle.InstancePerCall);

            return config;
        }
    }
}
