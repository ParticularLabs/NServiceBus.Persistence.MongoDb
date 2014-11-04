using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Persistence.MongoDB.Gateway;
using NServiceBus.Persistence.MongoDB.Sagas;
using NServiceBus.Persistence.MongoDB.Subscriptions;
using NServiceBus.Persistence.MongoDB.Timeout;

namespace NServiceBus.Persistence.MongoDB
{
    public class MongoDbPersistence : PersistenceDefinition
    {
        public MongoDbPersistence()
        {
            Supports(Storage.GatewayDeduplication, s => s.EnableFeatureByDefault<MongoDbGatewayDeduplication>());
            Supports(Storage.Timeouts, s => s.EnableFeatureByDefault<MongoDbTimeoutStorage>());
            Supports(Storage.Sagas, s => s.EnableFeatureByDefault<MongoDbSagaStorage>());
            Supports(Storage.Subscriptions, s => s.EnableFeatureByDefault<MongoDbSubscriptionStorage>());
        }
    }
}
