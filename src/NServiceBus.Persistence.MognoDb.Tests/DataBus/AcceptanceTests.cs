using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.DataBus
{

    [TestFixture]
    public class AcceptanceTests : MongoFixture
    {
        [Test]
        public async Task Should_handle_be_able_to_read_stored_values()
        {
            const string content = "Test";

            var key = await Put(content, TimeSpan.MaxValue).ConfigureAwait(false);
            using (var stream = await GridFsDataBus.Get(key).ConfigureAwait(false))
            {
                Assert.AreEqual(content, new StreamReader(stream).ReadToEnd());
            }
        }

        [Test]
        public async Task Should_handle_be_able_to_read_stored_values_concurrently()
        {
            const string content = "Test";

            var key = await Put(content, TimeSpan.MaxValue).ConfigureAwait(false);


            var tasks = Enumerable.Range(0, 10).Select(i =>
            {
                return Task.Run(async () =>
                {
                    using(var stream = await GridFsDataBus.Get(key).ConfigureAwait(false))
                    {
                        Assert.AreEqual(content, new StreamReader(stream).ReadToEnd());
                    }
                });
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
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

            //Put("Test", TimeSpan.MaxValue).Wait();
            //Assert.True(Directory.Exists(Path.Combine(basePath, DateTime.Now.AddDays(1).ToString("yyyy-MM-dd_HH"))));
        }
    }
}
