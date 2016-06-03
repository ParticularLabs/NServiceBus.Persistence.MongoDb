using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence;
using NServiceBus.Persistence.MongoDB.Gateway;
using NServiceBus.Persistence.MongoDB.Timeout;
using NServiceBus.Support;
using NServiceBus.Timeout.Core;
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

            Assert.IsTrue(await Deduplication.DeduplicateMessage("test", timeReceived, null));
            Assert.IsTrue(await Deduplication.DeduplicateMessage("test2", timeReceived, null));
        }

        [TestCase]
        public async Task Returns_False_With_Duplicate_Id()
        {
            var timeReceived = DateTime.Today;

            Assert.IsTrue(await Deduplication.DeduplicateMessage("test", timeReceived, null));
            Assert.IsFalse(await Deduplication.DeduplicateMessage("test", timeReceived, null));
        }
    }
}
