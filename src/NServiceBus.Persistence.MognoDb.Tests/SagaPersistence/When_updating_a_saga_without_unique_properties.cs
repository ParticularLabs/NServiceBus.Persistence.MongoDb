using System;
using System.Threading.Tasks;
using NServiceBus.Persistence.MongoDB.Sagas;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_updating_a_saga_without_unique_properties : MongoFixture
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var saga1 = new SagaWithoutUniqueProperties()
            {
                Id = Guid.NewGuid(),
                UniqueString = "whatever",
                NonUniqueString = "notUnique"
            };

            await SaveSaga(saga1);
            
            await UpdateSaga<SagaWithoutUniqueProperties>(saga1.Id, s =>
            {
                s.NonUniqueString = "notUnique2";
                s.UniqueString = "whatever2";
            });

            saga1 = await LoadSaga<SagaWithoutUniqueProperties>(saga1.Id);
            Assert.AreEqual("notUnique2", saga1.NonUniqueString);
        }


        [Test]
        public async Task It_should_throw_when_version_changed()
        {
            var saga1 = new SagaWithoutUniqueProperties()
            {
                Id = Guid.NewGuid(),
                UniqueString = "whatever",
                NonUniqueString = "notUnique"
            };

            await SaveSaga(saga1);

            Assert.ThrowsAsync<SagaMongoDbConcurrentUpdateException>(() => 
                UpdateSaga<SagaWithoutUniqueProperties>(saga1.Id, s =>
                {
                    Assert.AreEqual(s.Version, 0);
                    s.NonUniqueString = "notUnique2";
                    s.UniqueString = "whatever2";

                    ChangeSagaVersionManually<SagaWithoutUniqueProperties>(s.Id, 1);
                })
            );
        }
    }
}