using System;
using System.IO;
using System.Threading.Tasks;
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

        public async Task<Stream> Get(string key)
        {
            var stream = new MemoryStream();
            await _fs.DownloadToStreamAsync(ObjectId.Parse(key), stream).ConfigureAwait(false);
            stream.Position = 0;
            return stream;
        }

        public async Task<string> Put(Stream stream, TimeSpan timeToBeReceived)
        {
            var key = await _fs.UploadFromStreamAsync(Guid.NewGuid().ToString(), stream).ConfigureAwait(false);
            return key.ToString();
        }
        
        Task IDataBus.Start()
        {
            return Task.FromResult(0);
        }
    }
}