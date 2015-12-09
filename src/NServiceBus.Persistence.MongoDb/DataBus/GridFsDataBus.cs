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
            _fs.DownloadToStream(ObjectId.Parse(key), stream);
            stream.Position = 0;
            return stream;
        }

        public string Put(Stream stream, TimeSpan timeToBeReceived)
        {
            var key = _fs.UploadFromStream(Guid.NewGuid().ToString(), stream);
            return key.ToString();
        }

        public void Start()
        {
            
        }
    }
}