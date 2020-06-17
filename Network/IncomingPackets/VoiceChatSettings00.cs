using System.IO;
using VoiceChat;

namespace Network
{
    public class VoiceChatSettings00: IncomingPacket
    {

        public VoiceChatSettings00() 
        {

        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                Settings.voiceChatPort77=ReadInt32(ms);
                VoiceChatSettings.Preset= (VoiceChatPreset) ReadInt32(ms);
                if (!Settings.voiceChatEnabled)
                {
                    VoiceChatRecorder.Instance.AwakeChat();
                    Settings.voiceChatEnabled = true;
                }
                CheckEndPacket__( packetId, ms);
            }
        }
    }
}
