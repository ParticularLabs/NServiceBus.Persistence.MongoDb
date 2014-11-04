NServiceBus.Persistence.MongoDb
===============================

MongoDB Persistence for NServiceBus 5.  

### Installation
There are two ways:
* Grab the source and compile it yourself :-)
* Install the NuGet Package `NServiceBus.Persistence.MongoDb` by typing in the Package Manager Console: 
  ```
  Install-Package NServiceBus.Persistence.MongoDb
  ```

### Usage
To enable MongoDB persistence, call `configuration.UsePersistence<MongoDbPersistence>()` on your BusConfiguration instance.  

### Example configuration:
```csharp
using NServiceBus;
using NServiceBus.Persistence.MongoDB;

namespace Example
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UsePersistence<MongoDbPersistence>();
        }
    }
}
```

By default, the `NServiceBus/Persistence/MongoDB` connection string is used.
```xml
<connectionStrings>
    <add name="NServiceBus/Persistence/MongoDB" connectionString="mongodb://localhost/databaseName"/>
</connectionStrings>
```

A custom connection string name can be supplied via the `.SetConnectionStringName(string)` extension method:
```csharp
configuration.UsePersistence<MongoDbPersistence>().SetConnectionStringName("MyConnectionString");
```

A full connection string name can be supplied via the `.SetConnectionString(string)` extension method:
```csharp
configuration.UsePersistence<MongoDbPersistence>().SetConnectionString("mongodb://localhost/databaseName");
```

### Saga Data Requirements
To use the saga persister, your IContainsSagaData requires a property that has the `[DocumentVersion]` attribute. A property containing the `[Unique]` attribute is also recommended.  Example:

```csharp
public class MySagaData : IContainSagaData
{
    [Unique]
    public string MyEntityId { get; set; }

    [DocumentVersion]
    public int Version { get; set; }
    
    public bool Canceled { get; set; }
    
    public Guid Id { get; set; }
    public string Originator { get; set; }
    public string OriginalMessageId { get; set; }
}
```

### Handling Concurrency in Sagas With MongoDb
Sagas are a great feature of NServiceBus.  The key concurrency safeguards that sagas guarantee (see: http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency) depend heavily on the underlying data store.  The two specific cases that NServiceBus relies on the underling data store are [concurrent access to non-existing saga instances](http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency#concurrent-access-to-non-existing-saga-instances) and [concurrent access to existing saga instances](http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency#concurrent-access-to-existing-saga-instances).

#### Concurrent access to non-existing saga instances

The persister uses MongoDb's [Unique Indexes](http://docs.mongodb.org/manual/core/index-unique/) to ensure only one document can contain the unique data.

#### Concurrent access to existing saga instances
The persister uses a document versioning scheme built on top of MongoDb's [findAndModify](http://docs.mongodb.org/manual/reference/command/findAndModify/) command to atomically update the existing persisted data only if it has not been changed since it was retrieved.  Since the update is atomic, it will ensure that if there are multiple simultaneous updates to a saga, only one will succeed.

### Credits
A major fork of https://github.com/justinsaraceno/NServicebus-Mongo.

### Thanks to our contributors
[@ruslanrusu](https://twitter.com/ruslanrusu)  
[CRuppert](https://github.com/CRuppert)
