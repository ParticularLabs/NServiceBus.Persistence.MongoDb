using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB.Sagas
{
    public class MongoDbSagaStorage : Feature
    {
        internal MongoDbSagaStorage()
        {
            DependsOn<Features.Sagas>();
            DependsOn<MongoDbStorage>();
        }

        /// <summary>
        /// Called when the feature should perform its initialization. This call will only happen if the feature is enabled.
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<SagaPersister>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<MongoDbSagaRepository>(DependencyLifecycle.SingleInstance);
        }
    }
}
