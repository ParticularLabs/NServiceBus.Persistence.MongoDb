using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_updating_a_saga_with_unique_properties : MongoFixture
    {
        [Test]
        public async Task It_should_persist_successfully()
        {
            var saga1 = new SagaWithUniqueProperty()
                            {
                                Id = Guid.NewGuid(),
                                UniqueString = "whatever",
                                NonUniqueString = "notUnique"
                            };

            await SaveSaga(saga1).ConfigureAwait(false);

            await UpdateSaga<SagaWithUniqueProperty>(saga1.Id, s => s.NonUniqueString = "notUnique2").ConfigureAwait(false);

            saga1 = await LoadSaga<SagaWithUniqueProperty>(saga1.Id).ConfigureAwait(false);
            Assert.AreEqual("notUnique2", saga1.NonUniqueString);
        }

        [Test]
        public async Task It_should_should_enforce_uniqueness()
        {
            var saga = new SagaWithUniqueProperty()
            {
                Id = Guid.NewGuid(),
                UniqueString = "abc",
                NonUniqueString = "notUnique"
            };

            await SaveSaga(saga).ConfigureAwait(false);


            saga.Id = Guid.NewGuid();
            var writeException = Assert.ThrowsAsync<MongoWriteException>(() => SaveSaga(saga));
            
            // Check for "E11000 duplicate key error"
            // https://docs.mongodb.org/manual/reference/command/insert/
            Assert.AreEqual(11000, writeException?.WriteError?.Code);
        }
    }
}