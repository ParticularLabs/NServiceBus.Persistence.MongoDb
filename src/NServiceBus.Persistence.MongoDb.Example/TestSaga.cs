using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Config;
using NServiceBus.Logging;
using NServiceBus.Persistence.MongoDB;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public class TestStartup : IWantToRunWhenEndpointStartsAndStops
    {
        static ILog Logger = LogManager.GetLogger<TestStartup>();
        
        public Task Start(IMessageSession session)
        {
            Task.Run(async () => 
            {
                while (true)
                {
                    Logger.Info("Enter an int to send in a test message: ");
                    
                    int id = 0;
                    if (Int32.TryParse(Console.ReadLine(), out id))
                    {
                        await session.SendLocal<TestMessage>(message =>
                        {
                            message.UserId = id; //console input
                            message.Message = Guid.NewGuid().ToString(); //random text
                            message.DataBusData = new DataBusProperty<byte[]>(new byte[1024*1024*5]); //5MB
                        }).ConfigureAwait(false);

                        Logger.InfoFormat("Message sent with Id = {0}", id);
                    }
                    else
                    {
                        Logger.Error("Error: Console input did not parse to an int.");
                    }
                }
            });

            return Task.FromResult(0);
        }
        

        public Task Stop(IMessageSession session)
        {
            return Task.FromResult(0);
        }
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
