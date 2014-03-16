using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.Persistence.MongoDB.Exceptions
{
    public class SagaMongoDbConcurrentUpdateException : Exception
    {
        public int ExpectedVersion { get; set; }

        public SagaMongoDbConcurrentUpdateException() : base("Concurrency issue.")
        {}

        public SagaMongoDbConcurrentUpdateException(int expectedVersion)
            : base(String.Format("Concurrency issue.  Version expected = {0}", expectedVersion))
        {
            ExpectedVersion = expectedVersion;
        }
    }
}
