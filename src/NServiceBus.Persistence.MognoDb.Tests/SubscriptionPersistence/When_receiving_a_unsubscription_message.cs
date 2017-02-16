using System.Threading.Tasks;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Subscriptions;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    [TestFixture]
    public class When_receiving_a_unsubscription_message : MongoFixture
    {
        [Test]
        public async Task All_subscription_entries_for_specfied_message_types_should_be_removed()
        {
            foreach(var messageType in MessageTypes.All)
                await Storage.Subscribe(TestClients.ClientA, messageType, null).ConfigureAwait(false);

            foreach (var messageType in MessageTypes.All)
                await Storage.Unsubscribe(TestClients.ClientA, messageType, null).ConfigureAwait(false);

            var builder = Builders<Subscription>.Filter;
            var query = builder.Ne(s => s.Subscribers, null) & !builder.Size(s => s.Subscribers, 0);

            var count = Subscriptions.Count(query);
            Assert.AreEqual(0, count);
        }
    }
}