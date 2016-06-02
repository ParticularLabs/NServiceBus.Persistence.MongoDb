using System.Threading.Tasks;
using NServiceBus.Unicast.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_receiving_a_subscription_message : MongoFixture
    {
        [Test]
        public async Task A_subscription_entry_should_be_added_to_the_database()
        {
            var messageTypes = new[] {new MessageType(typeof (MessageA)), new MessageType(typeof (MessageB))};

            foreach(var messageType in messageTypes)
                await Storage.Subscribe(TestClients.ClientA, messageType, null);

            var count = Subscriptions.Count();

            Assert.AreEqual(count, 2);
        }

        [Test]
        public async Task Duplicate_subcription_shouldnt_create_additional_db_rows()
        {
            await Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA, null);
            await Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA, null);
            
            var count = Subscriptions.Count();
            Assert.AreEqual(count, 1);
        }
    }
}
