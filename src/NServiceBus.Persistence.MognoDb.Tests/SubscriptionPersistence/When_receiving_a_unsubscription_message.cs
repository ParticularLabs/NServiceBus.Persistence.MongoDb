using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;
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


            var count = Subscriptions.AsQueryable().Count(s => s.Subscribers != null && s.Subscribers.Any());
            Assert.AreEqual(0, count);
        }
    }
}