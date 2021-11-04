using System.Collections.Generic;
using XRMultiplayer.Models;
using LiteNetLib;

namespace XRMultiplayer.Networking
{
    public interface IProcessor
    {
        Queue<BaseMessageType> IncomingMessages { get; set; }

        Queue<BaseMessageType> OutgoingMessages { get; set; }

        bool AddInMessage(byte[] message, NetPeer player);

        bool AddOutMessage(BaseMessageType objectToSend);

        void ProcessIncoming();

        void ProcessOutgoing();
    }
}
