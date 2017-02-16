﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_using_semantic_versioning_of_messages : MongoFixture
    {
        [Test]
        public async Task Only_changes_in_major_version_should_effect_subscribers()
        {
            await Storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA, null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientB, MessageTypes.MessageAv11, null).ConfigureAwait(false);
            await Storage.Subscribe(TestClients.ClientC, MessageTypes.MessageAv2, null).ConfigureAwait(false);

            Assert.AreEqual(2, (await Storage.GetSubscriberAddressesForMessage(new[] {MessageTypes.MessageA }, null).ConfigureAwait(false)).Count());
            Assert.AreEqual(2, (await Storage.GetSubscriberAddressesForMessage(new[] {MessageTypes.MessageAv11 }, null).ConfigureAwait(false)).Count());
            Assert.AreEqual(1, (await Storage.GetSubscriberAddressesForMessage(new[] { MessageTypes.MessageAv2}, null).ConfigureAwait(false)).Count());
        }
    }
}