using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
