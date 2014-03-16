using System;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Configuration;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Configuration
{
    [TestFixture]
    public class When_configuring_persistence_to_use_a_mongo_server_instance_with_the_default_settings
    {
        MongoDatabase _db;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var config = Configure.With(new[] {GetType().Assembly})
                .DefineEndpointName("UnitTests")
                .DefaultBuilder()
                .MongoDbPersistence();

            _db = config.Builder.Build<MongoDatabase>();
        }

        [Test]
        public void It_should_configure_the_document_store_to_use_the_default_url()
        {
            Assert.AreEqual("localhost", _db.Server.Settings.Server.Host);
            Assert.AreEqual(27017, _db.Server.Settings.Server.Port);
        }

        [Test]
        public void It_should_configure_the_document_store_to_use_the_endpoint_name_as_the_database()
        {
            Console.WriteLine("EndpointName: {0}", Configure.EndpointName);
            Assert.AreEqual(Configure.EndpointName, _db.Name);
        }
    }
}