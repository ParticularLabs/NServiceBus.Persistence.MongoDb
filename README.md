**NOTE: This repository is no longer maintained. If you're looking for an officially supported MongoDB persistence for NServiceBus Versions 7 and above, please use [NServiceBus.Storage.MongoDB](https://github.com/particular/nservicebus.storage.mongodb). For documentation and samples, please refer to: https://docs.particular.net/persistence/mongodb/** 

## NServiceBus.Persistence.MongoDb  [![Build status](https://ci.appveyor.com/api/projects/status/9cfq3u3vd0rf4kl2/branch/master?svg=true)](https://ci.appveyor.com/project/tekmaven/nservicebus-persistence-mongodb/branch/master) [![NuGet version](https://badge.fury.io/nu/NServiceBus.Persistence.MongoDb.svg)](http://badge.fury.io/nu/NServiceBus.Persistence.MongoDb)##

This package includes MongoDB persistence implementations for NServiceBus v6:

- Timeouts
- Subscriptions
- Sagas
- DataBus

## Install ##
Add the `NServiceBus.Persistence.MongoDb` package to your NServiceBus service host project.

 ```Install-Package NServiceBus.Persistence.MongoDb```   

## Configuration ##
**1** Set the `EndpointConfiguration` object to use `MongoDbPersistence`

```csharp
using NServiceBus;
using NServiceBus.Persistence.MongoDB;

namespace Example
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration configuration)
        {
            configuration.UsePersistence<MongoDbPersistence>();
        }
    }
}
```

**2** Add your MongoDB connection string to your ```app.config```:

```xml
<connectionStrings>
    <add name="NServiceBus/Persistence/MongoDB"
		connectionString="mongodb://localhost/databaseName"/>
</connectionStrings>
```

**3** Hit F5. Yes, it is that simple.

## Custom Connection String Options ##
The persistence configuration model provides a reach API. This enables to override the default  
connection string name by calling ```.SetConnectionStringName(string)``` extension method.

```csharp
config
	.UsePersistence<MongoDbPersistence>()
	.SetConnectionStringName("SharedConnectionString");
```

If you are resolving your configuration setting from a different source at run-time which is  
very common in cloud based deployments. Then you can use  ```.SetConnectionString(string)```  
to provide it.

```csharp
config
	.UsePersistence<MongoDbPersistence>()
	.SetConnectionString("mongodb://localhost/databaseName");
```

## Saga definition guideline##
In order to get Sagas working correctly you need to enforce following

* your saga state should implement ```IContainSagaData```
* requires a property ```Version``` decorated with attribute ```[DocumentVersion]```

Here is an example

```csharp
public class OrderBillingSagaData : IContainSagaData
{
    public string OrderId { get; set; }

    [DocumentVersion]
    public int Version { get; set; }

    public bool Canceled { get; set; }

    public Guid Id { get; set; }
    public string Originator { get; set; }
    public string OriginalMessageId { get; set; }
}
```
## Dealing with concurrency ##
The key concurrency safeguards that sagas guarantee depend heavily on the underlying data store.   
The two specific cases that NServiceBus relies on the underling data store are [concurrent access to   
   non-existing saga instances](http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency#concurrent-access-to-non-existing-saga-instances) and [concurrent access to existing saga instances](http://docs.particular.net/NServiceBus/nservicebus-sagas-and-concurrency#concurrent-access-to-existing-saga-instances).

**Here is how we deal with them**  

*Concurrent access to non-existing saga instances*. The persister uses MongoDb's [Unique Indexes](http://docs.mongodb.org/manual/core/index-unique/)  
 to ensure only one document can contain the unique data.  

*Concurrent access to existing saga instances*. The persister uses a document versioning  
scheme built on top of MongoDb's [findAndModify](http://docs.mongodb.org/manual/reference/command/findAndModify/) command to atomically update the existing  
persisted data only if it has not been changed since it was retrieved. Since the update   
is atomic, it will ensure that if there are multiple simultaneous updates to a saga, only one  
will succeed.


## DataBus ##
Do you use [DataBus](http://docs.particular.net/nservicebus/attachments-databus-sample)?  We also supply an implimentation that is backed with MongoDB's GridFS.  To configure, just add this line to your busConfiguration:

```csharp
using NServiceBus;
using NServiceBus.Persistence.MongoDB;

namespace Example
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration configuration)
        {
            configuration.UsePersistence<MongoDbPersistence>();
            configuration.UseDataBus<MongoDbDataBus>(); //add this line!
        }
    }
}
```

## NServiceBus Documentation Sample
http://docs.particular.net/samples/mongodb/

## Thanks to our contributors ##
[@ruslanrusu](https://twitter.com/ruslanrusu)  
[CRuppert](https://github.com/CRuppert)
