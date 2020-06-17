using System.IO;

namespace Network
{
    public class SayToChat00 : IncomingPacket
    {

        public SayToChat00() 
        {

        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                ChatController.ChatType type = (ChatController.ChatType) ReadInt8(ms);
                string playerName = ReadString(ms);
                string text=ReadString(ms);
                ChatController.instance.AddMessage(type, playerName, text);
                CheckEndPacket__( packetId, ms);
            }
        }
    }
}
