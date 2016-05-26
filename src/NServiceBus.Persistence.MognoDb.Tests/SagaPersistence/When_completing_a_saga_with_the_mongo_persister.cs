using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_completing_a_saga_with_the_mongo_persister : MongoFixture
    {

        [Test]
        public async Task Should_delete_the_saga()
        {
            var sagaId = Guid.NewGuid();

            await SaveSaga(new TestSaga { Id = sagaId });
            await CompleteSaga<TestSaga>(sagaId);

            Assert.Null(await LoadSaga<TestSaga>(sagaId));
        }
    }
}