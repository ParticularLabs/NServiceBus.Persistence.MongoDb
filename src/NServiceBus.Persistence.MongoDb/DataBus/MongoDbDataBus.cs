using System;
using NServiceBus.DataBus;

namespace NServiceBus.Persistence.MongoDB.DataBus
{
    public class MongoDbDataBus : DataBusDefinition
    {
        protected override Type ProvidedByFeature()
        {
            return typeof (MongoDbDataBusPersistence);
        }
    }
}
