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
        public ObjectId Id { get; set; }
        
        public string SubscriberEndpoint { get; set; }
        public string Version { get; set; }
        public string TypeName { get; set; }
    }
}
