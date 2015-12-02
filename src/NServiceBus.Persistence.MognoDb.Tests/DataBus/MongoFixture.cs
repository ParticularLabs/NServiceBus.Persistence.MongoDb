using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using MongoDB.Driver;
using NServiceBus.Persistence.MongoDB.DataBus;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.DataBus
{
    public class MongoFixture
    {
        
        private IMongoDatabase _database;
        private MongoClient _client;
        private GridFsDataBus _gridFsDataBus;
        private string _databaseName;


        [SetUp]
        public void SetupContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            _client = new MongoClient(connectionString);
            _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            _database = _client.GetDatabase(_databaseName);
            _gridFsDataBus = new GridFsDataBus(_database);
        }

        protected GridFsDataBus GridFsDataBus
        {
            get { return _gridFsDataBus; }
        }

        protected string Put(string content, TimeSpan timeToLive)
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            using (var stream = new MemoryStream(byteArray))
            {
                return GridFsDataBus.Put(stream, timeToLive);
            }
        }

        [TearDown]
        public void TeardownContext()
        {
            _client.DropDatabaseAsync(_databaseName).Wait();
        }
    }
}