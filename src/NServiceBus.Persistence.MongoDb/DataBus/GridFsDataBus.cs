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
        private readonly IGridFSBucket _fs;

        public GridFsDataBus(IMongoDatabase database)
        {
            _fs = new GridFSBucket(database);
        }

        public Stream Get(string key)
        {
            var stream = new MemoryStream();
            _fs.DownloadToStreamAsync(ObjectId.Parse(key), stream).Wait();
            stream.Position = 0;
            return stream;
        }

        public string Put(Stream stream, TimeSpan timeToBeReceived)
        {
            var key = _fs.UploadFromStreamAsync(Guid.NewGuid().ToString(), stream).Result;

            return key.ToString();
        }

        public void Start()
        {
            
        }
    }
}