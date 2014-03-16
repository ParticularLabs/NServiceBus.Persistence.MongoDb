using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NServiceBus.Persistence.MongoDB.SubscriptionPersistence
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
