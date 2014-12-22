using NServiceBus.Features;
using NServiceBus.Persistence.MongoDB.Database;

namespace NServiceBus.Persistence.MongoDB.DataBus
{
    public class MongoDbDataBusPersistence : Feature
    {
        public MongoDbDataBusPersistence()
        {
            DependsOn<MongoDbStorage>();
            DependsOn<Features.DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<GridFsDataBus>(DependencyLifecycle.SingleInstance);
        }
    }
}