using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Config;
using NServiceBus.Logging;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Saga;

namespace NServiceBus.Persistence.MongoDb.Example
{
    public class TestStartup : IWantToRunWhenConfigurationIsComplete
    {
        static ILog Logger = LogManager.GetLogger<TestStartup>();

        public IBus Bus { get; set; }
        public void Run(Configure config)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Logger.Info("Enter an int to send in a test message: ");
                    
                    int id = 0;
                    if (Int32.TryParse(Console.ReadLine(), out id))
                    {
                        Bus.SendLocal<TestMessage>(message =>
                        {
                            message.UserId = id; //console input
                            message.Message = Guid.NewGuid().ToString(); //random text
                            message.DataBusData = new DataBusProperty<byte[]>(new byte[1024*1024*5]); //5MB
                        });

                        Logger.InfoFormat("Message sent with Id = {0}", id);
                    }
                    else
                    {
                        Logger.Error("Error: Console input did not parse to an int.");
                    }
                }
            });
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

        public void Handle(TestMessage message)
        {
            if (Data.UserId != message.UserId)
            {
                Logger.InfoFormat("[{0}] \tNew saga instance", message.UserId);
            }
            else
            {
                Logger.InfoFormat("[{0}] \tExisting saga retrieved.  Current version: {1}", message.UserId, Data.Version);
            }

            Data.UserId = message.UserId;
            Data.Message = $"{Data.Version} - {message.Message}";
            Console.WriteLine(message.DataBusData.Value.Length);

            RequestTimeout<TestSagaTimeout>(TimeSpan.FromSeconds(10));

            if (Data.Version == 10)
            {
                throw new Exception();
            }
        }

        public void Timeout(TestSagaTimeout state)
        {
            Logger.InfoFormat("[{0}] \tSaga instance timeout fired", Data.UserId);
        }
    }

    public class TestSagaTimeout
    {
        
    }

    public class TestSagaData : IContainSagaData
    {
        [Unique]
        public virtual int UserId { get; set; }

        [DocumentVersion]
        public virtual int Version { get; set; }

        public virtual string Message { get; set; }

        public virtual Guid Id { get; set; }
        public virtual string OriginalMessageId { get; set; }
        public virtual string Originator { get; set; }
    }
}
