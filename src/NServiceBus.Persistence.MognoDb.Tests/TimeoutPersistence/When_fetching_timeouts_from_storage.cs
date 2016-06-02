using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Support;
using NServiceBus.Timeout.Core;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.TimeoutPersistence
{
    [TestFixture]
    public class When_fetching_timeouts_from_storage : MongoFixture
    {
        [Test]
        public async Task Should_persist_with_common_nservicebus_headers()
        {
            var nextTime = DateTime.UtcNow.AddHours(1);

            await Storage.Add(new TimeoutData
            {
                Time = nextTime,
                Destination = $"timeouts@{RuntimeEnvironment.MachineName}",
                SagaId = Guid.NewGuid(),
                State = new byte[] { 0, 0, 133 },
                Headers = new Dictionary<string, string> { { Headers.MessageId, Guid.NewGuid().ToString() }, { Headers.NServiceBusVersion, Guid.NewGuid().ToString() }, { Headers.OriginatingAddress, Guid.NewGuid().ToString() } },
                OwningTimeoutManager = "MyTestEndpoint",
            }, null);
        }

        [Test]
        public async Task Should_return_the_complete_list_of_timeouts()
        {
            const int numberOfTimeoutsToAdd = 10;

            for (var i = 0; i < numberOfTimeoutsToAdd; i++)
            {
                await Storage.Add(new TimeoutData
                {
                    Time = DateTime.UtcNow.AddHours(-1),
                    Destination = $"timeouts@{RuntimeEnvironment.MachineName}",
                    SagaId = Guid.NewGuid(),
                    State = new byte[] { 0, 0, 133 },
                    Headers = new Dictionary<string, string> { { "Bar", "34234" }, { "Foo", "aString1" }, { "Super", "aString2" } },
                    OwningTimeoutManager = "MyTestEndpoint",
                }, null);
            }
            
            
            Assert.AreEqual(numberOfTimeoutsToAdd, (await Storage.GetNextChunk(DateTime.UtcNow.AddYears(-3))).DueTimeouts.Count());
        }

        [Test]
        public async Task Should_return_the_next_time_of_retrieval()
        {
            var nextTime = DateTime.UtcNow.AddHours(1);

            await Storage.Add(new TimeoutData
            {
                Time = nextTime,
                Destination = $"timeouts@{RuntimeEnvironment.MachineName}",
                SagaId = Guid.NewGuid(),
                State = new byte[] { 0, 0, 133 },
                Headers = new Dictionary<string, string> { { "Bar", "34234" }, { "Foo", "aString1" }, { "Super", "aString2" } },
                OwningTimeoutManager = "MyTestEndpoint",
            }, null);

            var result = await Storage.GetNextChunk(DateTime.UtcNow.AddYears(-3));

            Assert.IsTrue((nextTime - result.NextTimeToQuery).TotalSeconds < 1);
        }
    }
}
