using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NServiceBus.Persistence.MongoDB.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_receiving_a_unsubscription_message : MongoFixture
    {
        [Test]
        public void All_subscription_entries_for_specfied_message_types_should_be_removed()
        {
            Storage.Subscribe(TestClients.ClientA, MessageTypes.All);

            Storage.Unsubscribe(TestClients.ClientA, MessageTypes.All);

            var builder = Builders<Subscription>.Filter;
            var query = builder.Ne(s => s.Subscribers, null) & !builder.Size(s => s.Subscribers, 0);

            var count = Subscriptions.Count(query);
            Assert.AreEqual(0, count);
        }
    }
}