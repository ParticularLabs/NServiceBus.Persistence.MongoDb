## NServiceBus.Persistence.MongoDb  [![Build status](https://ci.appveyor.com/api/projects/status/9cfq3u3vd0rf4kl2/branch/master?svg=true)](https://ci.appveyor.com/project/tekmaven/nservicebus-persistence-mongodb/branch/master) [![NuGet version](https://badge.fury.io/nu/NServiceBus.Persistence.MongoDb.svg)](http://badge.fury.io/nu/NServiceBus.Persistence.MongoDb)##

**NServiceBus** the most developer-friendly service bus for .NET.

This package includes persistence implementations for:

- Timeouts 
- Subscriptions
- Sagas

All the boilerplate code is encapsulated in the implementation details which we took care of.  
No leaking abstractions, code against the well known Api.



## Install ##
To enable MongoDb persistence capability install NServiceBus.Persistence. MongoDb   
package in your NServiceBus service host project. At the moment there are 2 options 

1. NuGet package
	*  Package Manager Console:  ```Install-Package NServiceBus.Persistence.MongoDb```   
2. From source
	* ```git clone https://github.com/tekmaven/NServiceBus.Persistence.MongoDb``` 
	* ```.\build.cmd```




## Configuration ##
The minimal configuration you need to get up and running in 3 steps:

**1** Instruct the service host to use MongoDb persistence 

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

**2** Add in the ```app.config``` following

```xml
<connectionStrings>
    <add name="NServiceBus/Persistence/MongoDB" 
		connectionString="mongodb://localhost/databaseName"/>
</connectionStrings>
```

**3** Hit F5. Yes, is that simple.

## When default configuration is not an option ##
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
* the correlation id property should be decorated with attribute ```[Unique]```

Here is an example

```csharp
public class OrderBillingSagaData : IContainSagaData
{
    [Unique]
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

## Thanks to our contributors ##
A major fork of https://github.com/justinsaraceno/NServicebus-Mongo.

[@ruslanrusu](https://twitter.com/ruslanrusu)  
[CRuppert](https://github.com/CRuppert)
