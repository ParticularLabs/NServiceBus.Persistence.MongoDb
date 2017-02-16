using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.SagaPersistence
{
    public class When_persisting_a_saga_entity_with_related_entities : MongoFixture
    {
        RelatedClass _relatedClass;
        TestSaga _savedEntity;
        
        [SetUp]
        public async Task Setup()
        {
            var entity = new TestSaga {Id = Guid.NewGuid(), RelatedClass = new RelatedClass {Id = Guid.NewGuid()}};
            _relatedClass = entity.RelatedClass;

            await SaveSaga(entity).ConfigureAwait(false);

            _savedEntity = await LoadSaga<TestSaga>(entity.Id).ConfigureAwait(false);
        }


        [Test]
        public void Related_entities_should_also_be_persisted()
        {
            Assert.AreEqual(_relatedClass.Id, _savedEntity.RelatedClass.Id);
        }
    }
}