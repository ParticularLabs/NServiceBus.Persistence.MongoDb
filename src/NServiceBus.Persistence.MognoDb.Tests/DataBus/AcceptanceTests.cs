using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.DataBus
{

    [TestFixture]
    public class AcceptanceTests : MongoFixture
    {
        [Test]
        public void Should_handle_be_able_to_read_stored_values()
        {
            const string content = "Test";

            var key = Put(content, TimeSpan.MaxValue);
            using (var stream = GridFsDataBus.Get(key))
            {
                Assert.AreEqual(content, new StreamReader(stream).ReadToEnd());
            }
        }

        [Test]
        public void Should_handle_be_able_to_read_stored_values_concurrently()
        {
            const string content = "Test";

            var key = Put(content, TimeSpan.MaxValue);

            Parallel.For(0, 10, i =>
            {
                using (var stream = GridFsDataBus.Get(key))
                {
                    Assert.AreEqual(content, new StreamReader(stream).ReadToEnd());
                }
            });
        }

        [Test]
        [Ignore("Does not support TTL")]
        public void Should_handle_max_ttl()
        {
            //Put("Test", TimeSpan.MaxValue);
            //Assert.True(Directory.Exists(Path.Combine(basePath, DateTime.MaxValue.ToString("yyyy-MM-dd_HH"))));
        }

        [Test]
        [Ignore("Does not support TTL")]
        public void Should_honor_the_ttl_limit()
        {
            //dataBus.MaxMessageTimeToLive = TimeSpan.FromDays(1);

            Put("Test", TimeSpan.MaxValue);
            //Assert.True(Directory.Exists(Path.Combine(basePath, DateTime.Now.AddDays(1).ToString("yyyy-MM-dd_HH"))));
        }
    }
}
