using System.Linq;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_using_semantic_versioning_of_messages : MongoFixture
    {
        [Test]
        public void Only_changes_in_major_version_should_effect_subscribers()
        {
            Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA);
            Storage.Subscribe(TestClients.ClientB, MessageTypes.MessageAv11);
            Storage.Subscribe(TestClients.ClientC, MessageTypes.MessageAv2);

            Assert.AreEqual(2, Storage.GetSubscriberAddressesForMessage(MessageTypes.MessageA).Count());
            Assert.AreEqual(2, Storage.GetSubscriberAddressesForMessage(MessageTypes.MessageAv11).Count());
            Assert.AreEqual(1, Storage.GetSubscriberAddressesForMessage(MessageTypes.MessageAv2).Count());
        }
    }
}