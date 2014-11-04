using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Persistence.MongoDB.Configuration;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Configuration
{
    [TestFixture]
    public class When_configuring_persistence_to_use_a_mongo_server_instance_using_a_connection_string
    {
        string _connectionStringName;
        MongoDatabase _db;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _connectionStringName = "MongoDB";

            var config = new BusConfiguration();
            config.UsePersistence<MongoDbPersistence>();
            config.EndpointName("UnitTests");
            
            

            _db = config.Builder.Build<MongoDatabase>();
        }

        [Test]
        public void It_should_use_a_database()
        {
            Assert.IsNotNull(_db);
        }

        [Test]
        public void It_should_configure_the_database_with_the_connection_string()
        {
            var s = _db;
            Assert.AreEqual("localhost", _db.Server.Settings.Server.Host);
            Assert.AreEqual(27017, _db.Server.Settings.Server.Port);
            Assert.AreEqual("NServiceBus-Persistence-MognoDb-Tests", _db.Name);
        }


    }
}