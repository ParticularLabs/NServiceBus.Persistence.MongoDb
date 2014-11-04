using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB.Subscriptions
{
    public class MongoDbSubscriptionStorage : Feature
    {
        internal MongoDbSubscriptionStorage()
        {
            DependsOn<StorageDrivenPublishing>();
            DependsOn<MongoDbStorage>();
        }

        /// <summary>
        /// Called when the feature should perform its initialization. This call will only happen if the feature is enabled.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<SubscriptionPersister>(DependencyLifecycle.InstancePerCall);
        }
    }
}
