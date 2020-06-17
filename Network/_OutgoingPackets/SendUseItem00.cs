using System.IO;

namespace Network
{
    public class SendUseItem00: OutgoingPacket
    {

        private readonly int _selectedSlotId;

        public SendUseItem00(int selectedSlotId)
        {
            _selectedSlotId = selectedSlotId+ Inventory.FIRST_ID_SHORTCUT;
        }

        protected override byte[] GetPacketData()
        {
            using (var ms = new MemoryStream())
            {
                WriteInt8(18, ms);
                WriteInt32(_selectedSlotId, ms);

                return ms.ToArray();
            }
        }
    }
}