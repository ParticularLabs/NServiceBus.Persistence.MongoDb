using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Configuration;
using NServiceBus.Persistence.MongoDB.SagaPersistence;

namespace NServiceBus.Persistence.MongoDB
{
    public class MongoDbPersistence : PersistenceDefinition
    {
        public MongoDbPersistence()
        {
            Defaults(s =>
            {
                s.EnableFeatureByDefault<MongoDbStorage>();
            });

            /*Supports(Storage.GatewayDeduplication, s => s.EnableFeatureByDefault<RavenDbGatewayDeduplication>());
            Supports(Storage.Timeouts, s => s.EnableFeatureByDefault<RavenDbTimeoutStorage>());*/

            Supports(Storage.Sagas, s => s.EnableFeatureByDefault<MongoDbSagaStorage>());
            Supports(Storage.Subscriptions, s => s.EnableFeatureByDefault<MongoDbSubscriptionStorage>());
        }
    }
}
