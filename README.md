NServiceBus.Persistence.MongoDb
===============================

MongoDB Persistence for NServiceBus Sagas.  It handles concurrency using a document versioning scheme.  

### Not for production use
This is experimental and has not been put into production use.

### TODO
* Timeout Persistence

### Requirements
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

### Usage
To enable MongoDB persistence, use the MongoDB extention methods when calling Configure.  

| Method                          | Documentation                                                                                                                                               |
|---------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `.MongoDbPersistence()`         | Configures the MongoDB database connection. With no arguments, it automatically uses the connectionString with the name `NServiceBus/Persistence/MongoDB.`  |
| `.MongoDbSagaPersister()`       | Enables Saga Persister.                                                                                                                                     |
| `.MongoDbSubscriptionStorage()` | Enables Subscription Storage                                                                                                                                |

### Example configuration:
```csharp
using NServiceBus;
using NServiceBus.Persistence.MongoDB.Configuration;

namespace Example
{
    public class EndpointConfiguration : IConfigureThisEndpoint, AsA_Publisher, IWantCustomInitialization
    {
        public void Init()
        {
                Configure.With()
                    .DefaultBuilder()
                    .MongoDbPersistence()
                    .MongoSagaPersister()
                    .MongoDbSubscriptionStorage()
                    .UseInMemoryTimeoutPersister();
        }
    }
}
```

### Handling Concurrency With MongoDb
Sagas are a great feature of NServiceBus.  The key concurrency safeguards that sagas guarantee (see: http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency) depend heavily on the underlying data store.  The two specific cases that NServiceBus relies on the underling data store are [concurrent access to non-existing saga instances](http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency#concurrent-access-to-non-existing-saga-instances) and [concurrent access to existing saga instances](http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency#concurrent-access-to-existing-saga-instances).

#### Concurrent access to non-existing saga instances

The persister uses MongoDb's [Unique Indexes](http://docs.mongodb.org/manual/core/index-unique/) to ensure only one document can contain the unique data.

#### Concurrent access to existing saga instances
The persister uses a document versioning scheme built on top of MongoDb's [findAndModify](http://docs.mongodb.org/manual/reference/command/findAndModify/) command to atomically update the existing persisted data only if it has not been changed since it was retrieved.  Since the update is atomic, it will ensure that if multiple simultaneous updates to a saga only one will succeed.

### Credits
A major fork of https://github.com/justinsaraceno/NServicebus-Mongo.
