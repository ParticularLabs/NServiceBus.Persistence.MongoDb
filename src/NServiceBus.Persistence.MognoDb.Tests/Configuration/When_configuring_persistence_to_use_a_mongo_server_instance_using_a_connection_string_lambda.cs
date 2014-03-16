using System;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.Configuration;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Configuration
{
    [TestFixture]
    public class When_configuring_persistence_to_use_a_mongo_server_instance_using_a_connection_string_lambda
    {
        Func<string> _connectionStringFunc;
        MongoDatabase _db;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _connectionStringFunc = () => "mongodb://localhost:8080/connectionString_lambda";

            var config = Configure.With(new[] { GetType().Assembly })
                .DefineEndpointName("UnitTests")
                .DefaultBuilder()
                .MongoDbPersistence(_connectionStringFunc);

            _db = config.Builder.Build<MongoDatabase>();
        }

        [Test]
        public void It_should_use_a_document_store()
        {
            Assert.IsNotNull(_db);
        }

        [Test]
        public void It_should_configure_the_document_store_with_the_connection_string_lambda()
        {
            Assert.AreEqual("localhost", _db.Server.Settings.Server.Host);
            Assert.AreEqual(8080, _db.Server.Settings.Server.Port);
            Assert.AreEqual("connectionString_lambda", _db.Name);
        }
    }
}