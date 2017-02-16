using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Unicast.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_listing_subscribers_for_message_types : MongoFixture
    {
        [Test]
        public async Task The_names_of_all_subscribers_should_be_returned()
        {
            await Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA, null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageB, null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientB, MessageTypes.MessageA, null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageAv2, null).ConfigureAwait(false);

            var subscriptionsForMessageType = await Storage.GetSubscriberAddressesForMessage(new [] { MessageTypes.MessageA }, null).ConfigureAwait(false);

            Assert.AreEqual(2, subscriptionsForMessageType.Count());
            var firstSub = subscriptionsForMessageType.First();
            Assert.AreEqual(TestClients.ClientA.Endpoint.ToString(), firstSub.Endpoint.ToString());
            Assert.AreEqual(TestClients.ClientA.TransportAddress, firstSub.TransportAddress);
        }

        [Test]
        public async Task Duplicates_should_not_be_generated_for_interface_inheritance_chains()
        {
            await Storage.Subscribe(TestClients.ClientA, new MessageType(typeof(ISomeInterface)), null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientA, new MessageType(typeof(ISomeInterface2)), null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientA, new MessageType(typeof(ISomeInterface3)), null).ConfigureAwait(false);

            var subscriptionsForMessageType = await Storage.GetSubscriberAddressesForMessage(new[] { new MessageType(typeof(ISomeInterface)), new MessageType(typeof(ISomeInterface2)), new MessageType(typeof(ISomeInterface3)) }, null).ConfigureAwait(false);

            Assert.AreEqual(1, subscriptionsForMessageType.Count());
        }
    }
}