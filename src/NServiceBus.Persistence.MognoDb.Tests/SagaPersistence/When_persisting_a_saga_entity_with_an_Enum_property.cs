using System;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_an_Enum_property : MongoFixture
    {
        protected TestSaga entity;
        protected TestSaga savedEntity;

        [SetUp]
        public override void SetupContext()
        {
            base.SetupContext();

            entity = new TestSaga { Id = Guid.NewGuid() };
            entity.Status = StatusEnum.AnotherStatus;

            SagaPersister.Save(entity);

            savedEntity = SagaPersister.Get<TestSaga>(entity.Id);
        }

        [Test]
        public void Enums_should_be_persisted()
        {
            Assert.AreEqual(entity.Status, savedEntity.Status);
        }
    }
}