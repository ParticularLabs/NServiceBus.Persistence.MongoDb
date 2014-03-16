using System;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_a_concrete_class_property : MongoFixture
    {
        protected TestSaga entity;
        protected TestSaga savedEntity;

        [SetUp]
        public override void SetupContext()
        {
            base.SetupContext();

            entity = new TestSaga { Id = Guid.NewGuid() };
            entity.TestComponent = new TestComponent { Property = "Prop" };

            SagaPersister.Save(entity);

            savedEntity = SagaPersister.Get<TestSaga>(entity.Id);
        }

        [Test]
        public void Public_setters_and_getters_of_concrete_classes_should_be_persisted()
        {
            Assert.AreEqual(entity.TestComponent, savedEntity.TestComponent);
        }
    }
}