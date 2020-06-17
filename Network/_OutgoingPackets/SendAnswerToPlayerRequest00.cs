using System.IO;

namespace Network
{
    public class SendAnswerToPlayerRequest00: OutgoingPacket
    {

        private bool _yes;

        public SendAnswerToPlayerRequest00(bool yes)
        {
            _yes = yes;
        }

        protected override byte[] GetPacketData()
        {
            using (var ms = new MemoryStream())
            {
                WriteInt8(19, ms);
                WriteBool(_yes, ms);
                return ms.ToArray();
            }
        }
    }
}