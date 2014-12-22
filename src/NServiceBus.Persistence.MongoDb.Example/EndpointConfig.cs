using NServiceBus;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Persistence.MongoDB.DataBus;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UsePersistence<MongoDbPersistence>();
            configuration.UseDataBus<MongoDbDataBus>();
        }
    }
}
