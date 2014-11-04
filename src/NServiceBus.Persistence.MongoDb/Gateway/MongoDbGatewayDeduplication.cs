using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB.Gateway
{
    public class MongoDbGatewayDeduplication : Feature
    {
        public MongoDbGatewayDeduplication()
        {
            DependsOn("Gateway");
            DependsOn<MongoDbStorage>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<Deduplication>(DependencyLifecycle.InstancePerCall);
        }
    }
}
