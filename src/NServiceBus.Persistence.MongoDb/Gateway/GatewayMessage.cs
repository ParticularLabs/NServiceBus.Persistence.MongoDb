using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.Persistence.MongoDB.Gateway
{
    public class GatewayMessage
    {
        /// <summary>
        /// Id of this message.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The time at which the message was received.
        /// </summary>
        public DateTime TimeReceived { get; set; }
    }
}
