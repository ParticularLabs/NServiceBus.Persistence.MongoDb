using System;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Persistence.MongoDB;

namespace NServiceBus.Persistence.MongoDb.Example
{
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
