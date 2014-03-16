using System.Linq;
using NServiceBus.Unicast.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_listing_subscribers_for_message_types : MongoFixture
    {
        [Test]
        public void The_names_of_all_subscribers_should_be_returned()
        {
            Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA);
            Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageB);
            Storage.Subscribe(TestClients.ClientB, MessageTypes.MessageA);
            Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageAv2);

            var subscriptionsForMessageType = Storage.GetSubscriberAddressesForMessage(MessageTypes.MessageA);

            Assert.AreEqual(2, subscriptionsForMessageType.Count());
            Assert.AreEqual(TestClients.ClientA, subscriptionsForMessageType.First());
        }

        [Test]
        public void Duplicates_should_not_be_generated_for_interface_inheritance_chains()
        {
            Storage.Subscribe(TestClients.ClientA, new[] { new MessageType(typeof(ISomeInterface)) });
            Storage.Subscribe(TestClients.ClientA, new[] { new MessageType(typeof(ISomeInterface2)) });
            Storage.Subscribe(TestClients.ClientA, new[] { new MessageType(typeof(ISomeInterface3)) });

            var subscriptionsForMessageType = Storage.GetSubscriberAddressesForMessage(new[] { new MessageType(typeof(ISomeInterface)), new MessageType(typeof(ISomeInterface2)), new MessageType(typeof(ISomeInterface3)) });

            Assert.AreEqual(1, subscriptionsForMessageType.Count());
        }
    }
}