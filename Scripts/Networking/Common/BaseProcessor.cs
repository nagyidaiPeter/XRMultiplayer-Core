using hololensMultiplayer.Models;
using LiteNetLib;
using System.Collections.Generic;
using Zenject;

namespace hololensMultiplayer.Networking
{
    public abstract class BaseProcessor : IProcessor
    {
        public Queue<BaseMessageType> IncomingMessages { get; set; } = new Queue<BaseMessageType>();
        public Queue<BaseMessageType> OutgoingMessages { get; set; } = new Queue<BaseMessageType>();

        [Inject]
        protected DataManager dataManager;

        public abstract bool AddInMessage(byte[] message, NetPeer player);

        public abstract bool AddOutMessage(BaseMessageType objectToSend);

        public abstract void ProcessIncoming();

        public abstract void ProcessOutgoing();
    }
}
