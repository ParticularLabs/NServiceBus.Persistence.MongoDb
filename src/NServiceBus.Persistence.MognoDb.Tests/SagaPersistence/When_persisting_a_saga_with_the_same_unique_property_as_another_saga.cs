using System;
using MongoDB.Driver;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_with_the_same_unique_property_as_another_saga : MongoFixture
    {
        [Test]
        public void It_should_enforce_uniqueness()
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

            SaveSaga(saga1);
            try
            {
                SaveSaga(saga2);
                Assert.Fail("SaveSaga should throw an exception");
            }
            catch (MongoWriteException aggEx)
            {
                // Check for "E11000 duplicate key error"
                // https://docs.mongodb.org/manual/reference/command/insert/

                Assert.AreEqual(11000, aggEx.WriteError?.Code);
            }

        }
    }
}