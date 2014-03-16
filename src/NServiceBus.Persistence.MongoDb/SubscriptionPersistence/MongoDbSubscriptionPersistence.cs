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
            foreach (var messageType in messageTypes)
            {
                if(_subscriptions.AsQueryable().Where(x => x.TypeName == messageType.TypeName && x.SubscriberEndpoint == client.ToString()).ToList().Any(x => new MessageType(x.TypeName, x.Version) == messageType))
                    continue;
                
                _subscriptions.Save(new Subscription
                                        {
                                            SubscriberEndpoint = client.ToString(),
                                            Version = messageType.Version.ToString(),
                                            TypeName = messageType.TypeName
                                        });
            }
        }

        public void Unsubscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            var typeNames = messageTypes.Select(mt => mt.TypeName);
            var subscriptions = _subscriptions.AsQueryable().Where(x => x.TypeName.In(typeNames) && x.SubscriberEndpoint == client.ToString()).ToList();

            foreach (var subscription in subscriptions)
                _subscriptions.Remove(Query<Subscription>.EQ(s => s.Id, subscription.Id));
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes)
        {
            var typeNames = messageTypes.Select(mt => mt.TypeName);
            return _subscriptions.AsQueryable()
                    .Where(s => s.TypeName.In(typeNames)).ToList()
                    .Where(s => messageTypes.Contains(new MessageType(s.TypeName, s.Version)))
                    .Select(s => Address.Parse(s.SubscriberEndpoint))
                    .Distinct();
        }
    }
}
