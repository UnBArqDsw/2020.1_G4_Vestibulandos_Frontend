using Data;
using Serialization;

namespace Network.Transport.Tcp
{
    public interface ISocketListener
    {
        //----------------------------------------------------------------------------------------------------
        void NetworkLog(LogLevel enLevel, string strMsg);

        //----------------------------------------------------------------------------------------------------
        void OnStatusChanged(EnStatusCode enStatusCode);

        //----------------------------------------------------------------------------------------------------
        void OnPacketReceived(IPacket packet);
    }
}