using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_a_DateTime_property : MongoFixture
    {
        TestSaga _entity;
        TestSaga _savedEntity;
        
        [SetUp]
        public async Task Setup()
        {
            _entity = new TestSaga
            {
                Id = Guid.NewGuid(),
                DateTimeProperty = DateTime.Parse("12/02/2010 12:00:00.01").ToUniversalTime()
            };

            await SaveSaga(_entity).ConfigureAwait(false);

            _savedEntity = await LoadSaga<TestSaga>(_entity.Id).ConfigureAwait(false);
        }

        [Test]
        public void Datetime_property_should_be_persisted()
        {
            Assert.AreEqual(_entity.DateTimeProperty, _savedEntity.DateTimeProperty);
        }
    }
}