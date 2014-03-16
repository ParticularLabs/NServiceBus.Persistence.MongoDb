using System;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_completing_a_saga_with_the_mongo_persister : MongoFixture
    {

        [Test]
        public void Should_delete_the_saga()
        {
            var sagaId = Guid.NewGuid();

            SaveSaga(new TestSaga { Id = sagaId });
            CompleteSaga<TestSaga>(sagaId);

            Assert.Null(SagaPersister.Get<TestSaga>(sagaId));
        }
    }
}