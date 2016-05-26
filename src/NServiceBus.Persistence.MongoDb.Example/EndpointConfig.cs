using NServiceBus;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Persistence.MongoDB.DataBus;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration configuration)
        {
            configuration.UsePersistence<MongoDbPersistence>();
            configuration.UseDataBus<MongoDbDataBus>();
        }
    }
}
