using System.IO;

namespace Network
{
    public class SendLogout00 : OutgoingPacket
    {

        public SendLogout00()
        {
        }

        protected override byte[] GetPacketData()
        {
            using (var ms = new MemoryStream())
            {
                WriteInt8(5, ms);
                WriteInt8(0, ms);
                WriteInt8(0, ms);
                WriteInt8(0, ms);
                return ms.ToArray();
            }
        }
    }
}