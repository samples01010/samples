using System;
using System.IO;

namespace Network
{
    public class CraftResult00: IncomingPacket
    {

        public CraftResult00()
        {
        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                int time = ReadInt32(ms);
                bool done = Convert.ToBoolean(ReadInt8(ms));
                bool abortBecauseError = Convert.ToBoolean(ReadInt8(ms));
                int fuel = ReadInt16(ms);

                CraftController.instance.ProcessCraftResultPacket(time, done, abortBecauseError, fuel);


                CheckEndPacket__(packetId, ms);
            }
        }
    }
}
