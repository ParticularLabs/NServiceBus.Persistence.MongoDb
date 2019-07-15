**Archived documentation from https://docs.particular.net**

Uses [MongoDB](https://www.mongodb.com/) for DataBus storage.


## Usage

```
var persistence = endpointConfiguration.UsePersistence<MongoDbPersistence>();
persistence.SetConnectionString("mongodb://localhost/databaseName");
endpointConfiguration.UseDataBus<MongoDbDataBus>();
```


Note that the connection string used for the databus is shared by the MongoDB persistence.