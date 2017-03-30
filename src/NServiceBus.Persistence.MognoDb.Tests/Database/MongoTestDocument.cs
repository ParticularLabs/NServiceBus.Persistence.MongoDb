using MongoDB.Bson;

namespace NServiceBus.Persistence.MognoDb.Tests.Database
{
    public class MongoTestDocument
    {
        public ObjectId Id { get; set; }

        public string Property1 { get; set; }

        public string Property2 { get; set; }

    }
}
