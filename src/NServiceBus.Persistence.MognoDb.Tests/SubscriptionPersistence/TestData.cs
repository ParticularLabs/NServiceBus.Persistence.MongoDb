using System;
using System.Collections.Generic;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

namespace NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence
{
    public interface ISomeInterface
    {
    }
    public interface ISomeInterface2
    {
    }
    public interface ISomeInterface3
    {
    }

    public class MessageB
    {
    }

    public class MessageA
    {

    }
    public class MessageTypes
    {
        public static MessageType MessageA = new MessageType(typeof(MessageA).FullName, new Version(1, 0, 0, 0));
        public static MessageType MessageAv2 = new MessageType(typeof(MessageA).FullName, new Version(2, 0, 0, 0));
        public static MessageType MessageAv11 = new MessageType(typeof(MessageA).FullName, new Version(1, 1, 0, 0));
        public static MessageType MessageB = new MessageType(typeof(MessageB));

        public static IEnumerable<MessageType> All = new[] { new MessageType(typeof(MessageA)), new MessageType(typeof(MessageB)) };
    }

    public class TestClients
    {
        public static Subscriber ClientA = new Subscriber("ClientA", "EndpointA");
        public static Subscriber ClientB = new Subscriber("ClientB", "EndpointB");
        public static Subscriber ClientC = new Subscriber("ClientC", "EndpointC");
    }

}