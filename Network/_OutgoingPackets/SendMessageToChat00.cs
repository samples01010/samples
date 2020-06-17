using System.IO;

namespace Network
{
    public class SendMessageToChat00 : OutgoingPacket
    {

        private readonly int _type;
        private readonly string _target;
        private readonly string _message;
        private readonly int _val1;
        private readonly int _val2;

        public SendMessageToChat00(ChatController.ChatType chatType, string message, int val1, int val2)
        {
            _type = (int)chatType;
            _message = message;
            _target = "no";
            _val1 = val1;
            _val2 = val2;
        }

        public SendMessageToChat00(ChatController.ChatType chatType, string message)
        {
            _type = (int)chatType;
            _message = message;
            _target = "no";
        }

        public SendMessageToChat00 (ChatController.ChatType chatType, string message, string target)
        {
            _type = (int)chatType;
            _message = message;
            _target = target;
        }

        protected override byte[] GetPacketData()
        {
            using (var ms = new MemoryStream())
            {
                WriteInt8(2, ms);
                WriteInt8(_type, ms);
                WriteString(_message, ms);
                WriteString(_target, ms);
                WriteInt32(_val1, ms);
                WriteInt32(_val2, ms);
                return ms.ToArray();
            }
        }
    }
}