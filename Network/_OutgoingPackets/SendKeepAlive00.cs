using System.IO;

namespace Network
{
    public class SendKeepAlive00 : OutgoingPacket
    {

        public SendKeepAlive00()
        {
        }

        protected override byte[] GetPacketData()
        {
            using (var ms = new MemoryStream())
            {
                WriteInt8(1, ms);
                WriteInt8(UnityEngine.Random.Range(1, 250), ms);
                WriteInt8(UnityEngine.Random.Range(1, 250), ms);
                return ms.ToArray();
            }
        }
    }
}