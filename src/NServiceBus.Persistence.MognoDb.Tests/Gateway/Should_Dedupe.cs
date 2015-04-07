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
        public void Returns_True_With_Unique_Id()
        {
            var timeReceived = DateTime.Today;

            Assert.IsTrue(Deduplication.DeduplicateMessage("test", timeReceived));
            Assert.IsTrue(Deduplication.DeduplicateMessage("test2", timeReceived));
        }

        [TestCase]
        public void Returns_False_With_Duplicate_Id()
        {
            var timeReceived = DateTime.Today;

            Assert.IsTrue(Deduplication.DeduplicateMessage("test", timeReceived));
            Assert.IsFalse(Deduplication.DeduplicateMessage("test", timeReceived));
        }
    }
}
