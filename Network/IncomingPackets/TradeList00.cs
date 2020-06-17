using System.Collections.Generic;
using System.IO;

namespace Network
{
    public class TradeList00 : IncomingPacket
    {

        public TradeList00()
        {

        }

        public override void SetData(int packetId, byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer, false))
            {
                List<ItemW> items1 = new List<ItemW>();
                for (int i = 0; i < Player.MAX_TRADE_ITEMS; i++)
                {
                    ItemW newItem = ReadItem(ms);
                    if (newItem == null)
                        continue;

                    newItem.count = ReadInt32(ms);
                    items1.Add(newItem);
                }
                List<ItemW> items2 = new List<ItemW>();
                for (int i = 0; i < Player.MAX_TRADE_ITEMS; i++)
                {
                    ItemW newItem = ReadItem(ms);
                    if (newItem == null)
                        continue;

                    newItem.count = ReadInt32(ms);
                    items2.Add(newItem);
                }

                TradeController.instance.ProcessTradeList(items1, items2);

                CheckEndPacket__(packetId, ms);
            }
        }
    }
}
