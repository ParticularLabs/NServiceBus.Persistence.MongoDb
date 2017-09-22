using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
            var connectionString = "mongodb://localhost/NServiceBus-Persistence-MognoDb-Tests"; // hardcoded for now

            _client = new MongoClient(connectionString);
            _databaseName = "Test_" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            _database = _client.GetDatabase(_databaseName);
            _gridFsDataBus = new GridFsDataBus(_database);
        }

        protected GridFsDataBus GridFsDataBus => _gridFsDataBus;

        protected async Task<string> Put(string content, TimeSpan timeToLive)
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            using (var stream = new MemoryStream(byteArray))
            {
                return await GridFsDataBus.Put(stream, timeToLive).ConfigureAwait(false);
            }
        }

        [TearDown]
        public void TeardownContext() => _client.DropDatabase(_databaseName);
    }
}