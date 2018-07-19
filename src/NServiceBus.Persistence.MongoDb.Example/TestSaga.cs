using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Persistence.MongoDB.DataBus;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public static class TestStartup
    {
        private static string getTransportDirectory()
        {
            var assembly = Assembly.GetEntryAssembly();

            var assemblyLocation = assembly.CodeBase;
            if (assemblyLocation.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                assemblyLocation = assemblyLocation.Substring(8);
            }

            var returnValue = Path.Combine(Path.GetDirectoryName(assemblyLocation) ?? "", "transport");

            if (Directory.Exists(returnValue))
            {
                Directory.Delete(returnValue);
            }

            Directory.CreateDirectory(returnValue);

            return returnValue;
        }

        public static void Main(string[] args) => main().Wait();
        private static async Task main()
        {
            var endpointConfiguration = new EndpointConfiguration("MongoTestEndpoint");

            endpointConfiguration.UsePersistence<MongoDbPersistence>().SetConnectionString("mongodb://localhost/persistence-example");
            endpointConfiguration.UseDataBus<MongoDbDataBus>();

            var learningTransport = endpointConfiguration.UseTransport<LearningTransport>();
            learningTransport.NoPayloadSizeRestriction();
            learningTransport.StorageDirectory(getTransportDirectory());

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var endpoint = await Endpoint.Create(endpointConfiguration).ConfigureAwait(false);

            var endpointInstance = await endpoint.Start().ConfigureAwait(false);

            while (true)
            {
                Logger.Info("Enter an int to send in a test message: ");

                if (int.TryParse(Console.ReadLine(), out var id))
                {
                    await endpointInstance.SendLocal<TestMessage>(message =>
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
                    break;
                }
            }

            await endpointInstance.Stop().ConfigureAwait(false);
        }

        public static readonly ILog Logger = LogManager.GetLogger("log");
    }
    public class TestMessage : IMessage
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public DataBusProperty<byte[]> DataBusData { get; set; }
    }

    public class TestSaga : Saga<TestSagaData>, IAmStartedByMessages<TestMessage>, IHandleTimeouts<TestSagaTimeout>
    {
        static ILog Logger = LogManager.GetLogger<TestSaga>();

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
        {
            mapper.ConfigureMapping<TestMessage>(m => m.UserId).ToSaga(m => m.UserId);
        }

        public async Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            Logger.InfoFormat($"[{message.UserId}] TestSaga TestMessage recieved.  Current version: {Data.Version}");
            
            Data.Message = $"{Data.Version} - {message.Message}";
            Console.WriteLine(message.DataBusData.Value.Length);
            
            await RequestTimeout<TestSagaTimeout>(context, TimeSpan.FromSeconds(10)).ConfigureAwait(false);

            if (Data.Version == 10)
            {
                throw new Exception();
            }
        }

        public Task Timeout(TestSagaTimeout state, IMessageHandlerContext context)
        {
            Logger.InfoFormat("[{0}] \tSaga instance timeout fired", Data.UserId);
            return Task.FromResult(0);
        }
    }

    public class TestSagaTimeout
    {
        
    }

    public class TestSagaData : IContainSagaData
    {
        public virtual int UserId { get; set; }

        [DocumentVersion]
        public virtual int Version { get; set; }

        public virtual string Message { get; set; }

        public virtual Guid Id { get; set; }
        public virtual string OriginalMessageId { get; set; }
        public virtual string Originator { get; set; }
    }
}
