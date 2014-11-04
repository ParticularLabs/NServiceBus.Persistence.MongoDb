using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NServiceBus.Persistence.MongoDB.Sagas;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_updating_a_saga_property_that_does_not_have_a_unique_attribute : MongoFixture
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
        }
    }

    public class When_updating_a_saga_without_unique_properties : MongoFixture
    {
        [Test]
        public void It_should_persist_successfully()
        {
            var saga1 = new SagaWithoutUniqueProperties()
                            {
                                Id = Guid.NewGuid(),
                                UniqueString = "whatever",
                                NonUniqueString = "notUnique"
                            };

            SaveSaga(saga1);
            
            UpdateSaga<SagaWithoutUniqueProperties>(saga1.Id, s =>
                                                                  {
                                                                      s.NonUniqueString = "notUnique2";
                                                                      s.UniqueString = "whatever2";
                                                                  });
        }


        [Test]
        [ExpectedException(typeof(SagaMongoDbConcurrentUpdateException))]
        public void It_should_throw_when_version_changed()
        {
            var saga1 = new SagaWithoutUniqueProperties()
                            {
                                Id = Guid.NewGuid(),
                                UniqueString = "whatever",
                                NonUniqueString = "notUnique"
                            };

            SaveSaga(saga1);

            UpdateSaga<SagaWithoutUniqueProperties>(saga1.Id, s =>
            {
                Assert.AreEqual(s.Version, 0);
                s.NonUniqueString = "notUnique2";
                s.UniqueString = "whatever2";

                ChangeSagaVersionManually<SagaWithoutUniqueProperties>(s.Id, 1);
            });
        }
    }
}