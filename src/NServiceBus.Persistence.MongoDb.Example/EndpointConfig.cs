using NServiceBus;
using NServiceBus.Persistence.MongoDB;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UsePersistence<MongoDbPersistence>();
        }
    }
}
