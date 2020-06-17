using UnityEngine;
using System.Collections.Generic;
using System;


namespace Network
{

    //обрабатываем и преобразуем входящие пакеты в виде массива данных, во входящие пакеты-классы
    public class NetworkClientIncomingPacketParser
    {
        private List<byte[]> packets = new List<byte[]>();

        public void AddPacket(byte[] receiveBuffer)
        {
            //если это случается. значит ошибка в коде выше и иногда добавляется пустой массив
            if (receiveBuffer == null)
            {
                Debug.Log("WARNING. trying add null");
                return;
            }
            if (receiveBuffer.Length<1)
            {
                Debug.Log("WARNING. trying add zero");
                return;
            }
            packets.Add(receiveBuffer);
        }

        public void CheckAndParseIncomingPackets(NetworkManager netManager)
        {
            try
            {
                while (packets.Count>0)
                {
                    byte[] packet = packets[0];

                    // редкое событие. иногда массив равен нулл. просто игнорируем этот массив(пакет). 
                    if (packet==null)
                    {
                        Debug.Log("pknull. pkn="+ (packet==null) + "pcc=" + packets.Count);
                        packets.Remove(packet);
                        return;
                    }
                    if (packet.Length < 1)
                    {
                        Debug.Log("pknull2. pkl=" + packet.Length + "pcc=" + packets.Count);
                        packets.Remove(packet);
                        return;
                    }

                    packets.Remove(packet);
                    ParsePacket(packet, netManager);
                }
            }
            catch (Exception exc)
            {
                Debug.Log(exc);
            }
        }

        private void ParsePacket(byte[] packetData, NetworkManager netManager)
        {
            int packetType = packetData[0];           
            try
            {
                IncomingPacket packet = null;

                //remove header
                byte[] tmp = new byte[packetData.Length-1];
                Array.Copy(packetData,1,tmp,0, tmp.Length);
                packetData = tmp;

                switch (packetType)
                {
/////////////////////////////////
                    case 4:
                        packet = new SystemMessage00();
                        break;
                    case 5:
                        packet = new KeepAliveAnswer00(netManager);
                        break;
                    case 6:
                        packet = new AuthSuccess00(netManager);
                        break;
                    case 7:
                        packet = new PlayerInfo00();
                        break;
                    case 8:
                        packet = new SayToChat00();
                        break;
////////////////////////////////////////
						
                    default:
                        ClientLog.LogPacket("UNKNOWN PACKET => ", packetType, packetData, true);
                        break;
                }
                if (packet != null)
                {
                    packet.SetData(packetType, packetData);

                }
                ClientLog.LogPacket("client<=server ", packetType, packetData, false);
            }
            catch (Exception exc)
            {
                ClientLog.LogPacket("PROBLEM PARSING PACKET => ", packetType, packetData, true);
                if (exc.Message != null)
                    Debug.Log("ERR: " + exc.Message);
            }

        }


    }
}
