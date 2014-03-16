using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NServiceBus.Logging;
using NServiceBus.Persistence.MongoDB.Configuration;
using NServiceBus.Persistence.Raven.SubscriptionStorage;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

namespace NServiceBus.Persistence.MongoDB.SubscriptionPersistence
{
    public class MongoDbSubscriptionPersistence : ISubscriptionStorage
    {
        private readonly MongoCollection<Subscription> _subscriptions;

        public MongoDbSubscriptionPersistence(MongoDatabase database)
        {
            _subscriptions = database.GetCollection<Subscription>(MongoPersistenceConstants.SubscriptionCollectionName);
        }

        public void Init()
        {
        }

        public void Subscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            foreach (var key in GetMessageTypeKeys(messageTypes))
            {
                var query = Query<Subscription>.EQ(s => s.Id, key);
                var update = Update<Subscription>.AddToSet(s => s.Subscribers, client.ToString());

                _subscriptions.Update(query, update, UpdateFlags.Upsert);
            }
        }

        private IEnumerable<SubscriptionKey> GetMessageTypeKeys(IEnumerable<MessageType> messageTypes)
        {
            return messageTypes.Select(t => new SubscriptionKey {TypeName = t.TypeName, Version = t.Version.Major.ToString()});
        }

        public void Unsubscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            foreach (var key in GetMessageTypeKeys(messageTypes))
            {
                var query = Query.And(Query<Subscription>.EQ(s => s.Id, key), Query<Subscription>.EQ(s => s.Subscribers, client.ToString()));
                var update = Update<Subscription>.Pull(s => s.Subscribers, client.ToString());

                _subscriptions.Update(query, update, UpdateFlags.None);
            }
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes)
        {
            var keys = GetMessageTypeKeys(messageTypes);

            return _subscriptions.AsQueryable()
                .Where(s => keys.Contains(s.Id))
                .ToList()
                .SelectMany(s => s.Subscribers)
                .Distinct()
                .Select(Address.Parse);
        }
    }
}
