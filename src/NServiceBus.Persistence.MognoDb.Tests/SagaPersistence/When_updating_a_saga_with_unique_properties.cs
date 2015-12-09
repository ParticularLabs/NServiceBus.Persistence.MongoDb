using System;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_updating_a_saga_with_unique_properties : MongoFixture
    {
        [Test]
        public void It_should_persist_successfully()
        {
            var saga1 = new SagaWithUniqueProperty()
                            {
                                Id = Guid.NewGuid(),
                                UniqueString = "whatever",
                                NonUniqueString = "notUnique"
                            };

            SaveSaga(saga1);

            UpdateSaga<SagaWithUniqueProperty>(saga1.Id, s => s.NonUniqueString = "notUnique2");

            saga1 = LoadSaga<SagaWithUniqueProperty>(saga1.Id);
            Assert.AreEqual("notUnique2", saga1.NonUniqueString);
        }

        [Test]
        public void It_should_should_enforce_uniqueness()
        {
            SaveSaga(new SagaWithUniqueProperty()
            {
                Id = Guid.NewGuid(),
                UniqueString = "whatever",
                NonUniqueString = "notUnique"
            });

            try
            {
                SaveSaga(new SagaWithUniqueProperty()
                {
                    Id = Guid.NewGuid(),
                    UniqueString = "whatever",
                    NonUniqueString = "notUnique"
                });
            }
            catch (MongoWriteException aggEx)
            {
                // Check for "E11000 duplicate key error"
                // https://docs.mongodb.org/manual/reference/command/insert/

                Assert.AreEqual(11000, aggEx.WriteError?.Code);
                return;
            }

            Assert.Fail("Should have thrown an exception");

        }
    }
}