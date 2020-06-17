using System.IO;

namespace Network
{
    public class TradeResult00: IncomingPacket
    {


        public TradeResult00() 
        {

        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                int r=ReadInt32(ms);
                TradeController.instance.ProcessTradeResult(r);
                CheckEndPacket__( packetId, ms);
            }
        }
    }
}
