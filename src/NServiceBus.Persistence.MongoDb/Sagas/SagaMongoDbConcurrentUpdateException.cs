using System;

namespace NServiceBus.Persistence.MongoDB.Sagas
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
