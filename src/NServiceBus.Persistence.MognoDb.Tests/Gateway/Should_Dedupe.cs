using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.Gateway
{
    [TestFixture]
    public class Should_Dedupe : MongorFixture
    {
        [TestCase]
        public async Task Returns_True_With_Unique_Id()
        {
            var timeReceived = DateTime.Today;

            Assert.IsTrue(await Deduplication.DeduplicateMessage("test", timeReceived, null).ConfigureAwait(false));
            Assert.IsTrue(await Deduplication.DeduplicateMessage("test2", timeReceived, null).ConfigureAwait(false));
        }

        [TestCase]
        public async Task Returns_False_With_Duplicate_Id()
        {
            var timeReceived = DateTime.Today;

            Assert.IsTrue(await Deduplication.DeduplicateMessage("test", timeReceived, null).ConfigureAwait(false));
            Assert.IsFalse(await Deduplication.DeduplicateMessage("test", timeReceived, null).ConfigureAwait(false));
        }
    }
}
