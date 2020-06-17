using System.IO;

namespace Network
{
    public class SystemMessage00 : IncomingPacket
    {

        public SystemMessage00() 
        {
        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                string text = ReadString(ms);
                int val1 = ReadInt32(ms);
                int val2 = ReadInt32(ms);
                int val3 = ReadInt32(ms);
                int val4 = ReadInt32(ms);

                ChatController.instance.AddMessage(ChatController.ChatType.systemMessage,
                    "", LanguageController.GetWordFromSystemMessage(text, val1, val2, val3, val4));
                CheckEndPacket__( packetId, ms);
            }
        }
    }
}
