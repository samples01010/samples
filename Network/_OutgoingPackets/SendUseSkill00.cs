using System.IO;

namespace Network
{
    public class SendUseSkill00 : OutgoingPacket
    {

        private readonly SkillsPanelController.Type _type;
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        public SendUseSkill00(SkillsPanelController.Type type, int x, int y, int z)
        {
            _type = type;
            _x = x;
            _y = y;
            _z = z;
        }

        protected override byte[] GetPacketData()
        {
            using (var ms = new MemoryStream())
            {
                WriteInt8(14, ms);
                WriteInt8((int)_type, ms);
                WriteInt32(_x, ms);
                WriteInt32(_y, ms);
                WriteInt32(_z, ms);

                return ms.ToArray();
            }
        }
    }
}