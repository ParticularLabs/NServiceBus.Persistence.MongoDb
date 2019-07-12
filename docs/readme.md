**Archived documentation from https://docs.particular.net**

Uses [MongoDB](https://www.mongodb.com/) for storage.

Includes MongoDB persistence implementations for

 * [Timeouts](https://docs.particular.net/nservicebus/sagas/timeouts)
 * Subscriptions
 * [Sagas](https://docs.particular.net/nservicebus/sagas/)


## Usage

To use MongoDB for persistence

```
busConfiguration.UsePersistence<MongoDbPersistence>();
```


## Connection settings

There are several ways to set the MongoDB Connection


### Via code

This allows resolution of configuration setting at run-time.

```
var persistence = busConfiguration.UsePersistence<MongoDbPersistence>();
persistence.SetConnectionString("mongodb://localhost/databaseName");
```


### Via an app.config connection string

```
<connectionStrings>
  <add name="NServiceBus/Persistence/MongoDB"
      connectionString="mongodb://localhost/databaseName"/>
</connectionStrings>
```

Override the default connection string name as follows:

```
var persistence = busConfiguration.UsePersistence<MongoDbPersistence>();
persistence.SetConnectionStringName("SharedConnectionString");
```


## Saga definition guideline

For sagas to work correctly, the following must be enforced:

 * Saga data should implement `IContainSagaData`.
 * Requires a property `Version` decorated with attribute `[DocumentVersion]`.

For example:

```
public class OrderBillingSagaData :
    ContainSagaData
{
    public string OrderId { get; set; }

    [DocumentVersion]
    public int Version { get; set; }

    public bool Canceled { get; set; }
}
```


## Dealing with concurrency

The key concurrency safeguards that sagas guarantee depend heavily on the underlying data store. The two specific cases that NServiceBus relies on the underling data store are [concurrent access to non-existing saga instances](https://docs.particular.net/nservicebus/sagas/concurrency#concurrent-access-to-non-existing-saga-instances) and [concurrent access to existing saga instances](https://docs.particular.net/nservicebus/sagas/concurrency#concurrent-access-to-existing-saga-instances).


### Concurrent access to non-existing saga instances

The persister uses [unique indexes](https://docs.mongodb.com/manual/core/index-unique/) to ensure only one document can contain the unique data.


### Concurrent access to existing saga instances

The persister uses a document versioning scheme built on the [findAndModify](https://docs.mongodb.com/manual/reference/command/findAndModify/) command to atomically update the existing persisted data only if it has not been changed since it was retrieved. Since the update is atomic, it will ensure that only one update to a saga will succeed if there are multiple simultaneous updates made to it.
