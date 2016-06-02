using System;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_inherited_property : MongoFixture
    {
        protected TestSaga entity;
        protected TestSaga savedEntity;

        [SetUp]
        public override void SetupContext()
        {
            base.SetupContext();

            entity = new TestSaga { Id = Guid.NewGuid() };
            entity.PolymorpicRelatedProperty = new PolymorpicProperty { SomeInt = 9 };

            SaveSaga(entity).Wait();

            savedEntity = LoadSaga<TestSaga>(entity.Id);
        }

        [Test]
        public void Inherited_property_classes_should_be_persisted()
        {
            Assert.AreEqual(entity.PolymorpicRelatedProperty, savedEntity.PolymorpicRelatedProperty);
        }
    }
}