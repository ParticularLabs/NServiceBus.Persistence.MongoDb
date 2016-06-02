using System;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_related_entities : MongoFixture
    {
        protected TestSaga entity;
        protected RelatedClass relatedClass;
        protected TestSaga savedEntity;

        [SetUp]
        public override void SetupContext()
        {
            base.SetupContext();

            entity = new TestSaga { Id = Guid.NewGuid() };
            entity.RelatedClass = new RelatedClass { Id = Guid.NewGuid() };
            relatedClass = entity.RelatedClass;

            SaveSaga(entity).Wait();

            savedEntity = LoadSaga<TestSaga>(entity.Id);
        }

        [Test]
        public void Related_entities_should_also_be_persisted()
        {
            Assert.AreEqual(relatedClass.Id, savedEntity.RelatedClass.Id);
        }
    }
}