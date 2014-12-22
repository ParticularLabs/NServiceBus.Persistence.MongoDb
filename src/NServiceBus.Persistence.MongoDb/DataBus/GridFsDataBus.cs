using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using NServiceBus.DataBus;

namespace NServiceBus.Persistence.MongoDB.DataBus
{
    public class GridFsDataBus : IDataBus
    {
        private readonly MongoGridFS _fs;

        public GridFsDataBus(MongoDatabase database)
        {
            _fs = database.GridFS;
        }

        public Stream Get(string key)
        {
            return _fs.OpenRead(key);
        }

        public string Put(Stream stream, TimeSpan timeToBeReceived)
        {
            var key = Guid.NewGuid().ToString();

            _fs.Upload(stream, key);

            return key;
        }

        public void Start()
        {
            
        }
    }
}