using System;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Persistence.MongoDB.DataBus;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public class Program
    {
        static ILog Logger = LogManager.GetLogger<Program>();

        public static async Task Main()
        {
            var configuration = new EndpointConfiguration("Mongo");
            configuration.UsePersistence<MongoDbPersistence>().SetConnectionString("mongodb://localhost/persistence-example");
            configuration.UseDataBus<MongoDbDataBus>();
            configuration.SendFailedMessagesTo("error");
            configuration.AuditProcessedMessagesTo("audit");

            var session = await Endpoint.Start(configuration);

            while (true)
            {
                Logger.Info("Enter an int to send in a test message: ");

                var id = 0;
                if (Int32.TryParse(Console.ReadLine(), out id))
                {
                    await session.SendLocal<TestMessage>(message =>
                    {
                        message.UserId = id; //console input
                        message.Message = Guid.NewGuid().ToString(); //random text
                        message.DataBusData = new DataBusProperty<byte[]>(new byte[1024 * 1024 * 5]); //5MB
                    }).ConfigureAwait(false);

                    Logger.InfoFormat("Message sent with Id = {0}", id);
                }
                else
                {
                    Logger.Error("Error: Console input did not parse to an int.");
                }
            }
        }
    }
}