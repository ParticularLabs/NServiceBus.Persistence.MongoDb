using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_a_concrete_class_property : MongoFixture
    {
        TestSaga _entity;
        TestSaga _savedEntity;
        
        [SetUp]
        public async Task Setup()
        {
            _entity = new TestSaga {Id = Guid.NewGuid(), TestComponent = new TestComponent {Property = "Prop"}};

            await SaveSaga(_entity).ConfigureAwait(false);

            _savedEntity = await LoadSaga<TestSaga>(_entity.Id).ConfigureAwait(false);
        }

        [Test]
        public void Public_setters_and_getters_of_concrete_classes_should_be_persisted()
        {
            Assert.AreEqual(_entity.TestComponent, _savedEntity.TestComponent);
        }
    }
}