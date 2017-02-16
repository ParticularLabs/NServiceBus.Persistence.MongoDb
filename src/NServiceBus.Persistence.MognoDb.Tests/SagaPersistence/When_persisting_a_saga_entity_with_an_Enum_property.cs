using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_an_Enum_property : MongoFixture
    {
        TestSaga _entity;
        TestSaga _savedEntity;

        [SetUp]
        public async Task Setup()
        {
            _entity = new TestSaga {Id = Guid.NewGuid(), Status = StatusEnum.AnotherStatus};

            await SaveSaga(_entity).ConfigureAwait(false);

            _savedEntity = await LoadSaga<TestSaga>(_entity.Id).ConfigureAwait(false);
        }

        [Test]
        public void Enums_should_be_persisted()
        {
            Assert.AreEqual(_entity.Status, _savedEntity.Status);
        }
    }
}