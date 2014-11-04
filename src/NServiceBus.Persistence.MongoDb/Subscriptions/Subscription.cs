using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace NServiceBus.Persistence.MongoDB.Subscriptions
{
    public class Subscription
    {
        [BsonId]
        public SubscriptionKey Id { get; set; }
        
        public List<string> Subscribers { get; set; }
    }

    public class SubscriptionKey
    {
        public string Version { get; set; }
        public string TypeName { get; set; }
    }
}
