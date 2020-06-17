using System.IO;

namespace Network
{
    public class PlaySound00: IncomingPacket
    {

        public PlaySound00()
        {

        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {

                int val= ReadInt32(ms);
                SoundManager.instance.Add((SoundManager.Sound)val);
                CheckEndPacket__(packetId, ms);
            }
        }
    }
}
