using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_with_the_same_unique_property_as_a_completed_saga : MongoFixture
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var saga1 = new SagaWithUniqueProperty
            {
                Id = Guid.NewGuid(),
                UniqueString = "whatever"
            };

            var saga2 = new SagaWithUniqueProperty
            {
                Id = Guid.NewGuid(),
                UniqueString = "whatever"
            };

            await SaveSaga(saga1).ConfigureAwait(false);
            await CompleteSaga<SagaWithUniqueProperty>(saga1.Id).ConfigureAwait(false);
            await SaveSaga(saga2).ConfigureAwait(false);
        }
    }
}