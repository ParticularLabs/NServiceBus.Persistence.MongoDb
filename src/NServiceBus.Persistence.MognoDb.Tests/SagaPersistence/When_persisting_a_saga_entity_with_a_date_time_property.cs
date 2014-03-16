using System;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_a_DateTime_property : MongoFixture
    {
        protected TestSaga entity;
        protected TestSaga savedEntity;

        [SetUp]
        public override void SetupContext()
        {
            base.SetupContext();

            entity = new TestSaga { Id = Guid.NewGuid() };
            entity.DateTimeProperty = DateTime.Parse("12/02/2010 12:00:00.01").ToUniversalTime();

            SagaPersister.Save(entity);

            savedEntity = SagaPersister.Get<TestSaga>(entity.Id);
        }

        [Test]
        public void Datetime_property_should_be_persisted()
        {
            Assert.AreEqual(entity.DateTimeProperty, savedEntity.DateTimeProperty);
        }
    }
}