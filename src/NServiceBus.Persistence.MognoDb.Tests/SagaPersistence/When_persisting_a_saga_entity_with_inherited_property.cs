using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_inherited_property : MongoFixture
    {
        TestSaga _entity;
        TestSaga _savedEntity;
        
        [SetUp]
        public async Task Setup()
        {
            _entity = new TestSaga
            {
                Id = Guid.NewGuid(),
                PolymorpicRelatedProperty = new PolymorpicProperty {SomeInt = 9}
            };

            await SaveSaga(_entity);

            _savedEntity = await LoadSaga<TestSaga>(_entity.Id);
        }

        [Test]
        public void Inherited_property_classes_should_be_persisted()
        {
            Assert.AreEqual(_entity.PolymorpicRelatedProperty, _savedEntity.PolymorpicRelatedProperty);
        }
    }
}