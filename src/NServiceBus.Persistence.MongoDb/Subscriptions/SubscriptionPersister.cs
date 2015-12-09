using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Database;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

namespace NServiceBus.Persistence.MongoDB.Subscriptions
{
    public class SubscriptionPersister : ISubscriptionStorage
    {
        private readonly IMongoCollection<Subscription> _subscriptions;

        public SubscriptionPersister(IMongoDatabase database)
        {
            _subscriptions = database.GetCollection<Subscription>(MongoPersistenceConstants.SubscriptionCollectionName);
        }
        
        public void Init()
        {
            _subscriptions.Indexes.CreateOne(
                new IndexKeysDefinitionBuilder<Subscription>().Ascending(s => s.Id).Ascending(s => s.Subscribers));
        }

        public void Subscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            foreach (var key in GetMessageTypeKeys(messageTypes))
            {
                var update = new UpdateDefinitionBuilder<Subscription>().AddToSet(s => s.Subscribers, client.ToString());

                _subscriptions.UpdateOne(s => s.Id == key, update, new UpdateOptions() {IsUpsert = true});
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
                var update = new UpdateDefinitionBuilder<Subscription>().Pull(s => s.Subscribers, client.ToString());
                
                _subscriptions.UpdateOne(s => s.Id == key && s.Subscribers.Contains(client.ToString()), update, new UpdateOptions() {IsUpsert = false});
            }
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes)
        {
            var keys = GetMessageTypeKeys(messageTypes);

            return _subscriptions.Find(s => keys.Contains(s.Id))
                .ToList()
                .SelectMany(s => s.Subscribers)
                .Distinct()
                .Select(Address.Parse);
        }
    }
}
