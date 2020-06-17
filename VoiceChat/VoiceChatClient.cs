using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace VoiceChat
{
    public class VoiceChatClient : MonoBehaviour
    {
        private static  UdpClient udpClient;

        public static Thread receiveThread;
        public static bool shutdownThread = false;
        private static IPEndPoint RemoteIpEndPoint;

        public static void StartClient()
        {
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            string serverHost = Settings.serverHost77;
            if (Settings.clientIsInLocalDevelopMode)
                serverHost = Settings.serverLocalHost77;

            udpClient = new UdpClient(serverHost, Settings.voiceChatPort77);        
            udpClient.Client.ReceiveTimeout = 1000;
            udpClient.Connect(serverHost, Settings.voiceChatPort77);

            receiveThread = new Thread(new ThreadStart(ReceivePacket));
            receiveThread.Start();


        }

        public static void sendInitPacket()
        {
            //Отправить 1 тест пакет для того что бы сервер знал инфо о соединении
            VoiceChatPacket testPacket = new VoiceChatPacket();
            testPacket.CompressedSampleLen = 64;
            testPacket.Data = new byte[64];
            testPacket.PacketId = 0;
            SendPacket(testPacket);
        }

        public static void SendPacket(VoiceChatPacket packet)
        {
            try
            {
                byte[] sendBytes = packet.Data;

                //добавляем заголовок
                var list = new List<byte>(sendBytes);

                byte[] bytes = BitConverter.GetBytes((Int16)packet.CompressedSampleLen);
                list.InsertRange(0, bytes);

                bytes = BitConverter.GetBytes(packet.PacketId);
                list.InsertRange(0, bytes);

                bytes = BitConverter.GetBytes(Player.voiceChatSessionId);
                list.InsertRange(0, bytes);

                bytes = BitConverter.GetBytes((Int16)Player.roomId);
                list.InsertRange(0, bytes);

                bytes = BitConverter.GetBytes((Int16)(list.Count+2));
                list.InsertRange(0, bytes);

                sendBytes = list.ToArray();
                udpClient.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private static void ReceivePacket()
        {
            while (!shutdownThread)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref RemoteIpEndPoint);
                    
                    //убираем заголовок
                    var list = new List<byte>(data);

                    int packetLen =  list[1] * 256 + list[0];
                    if (packetLen != list.Count)
                    {
                        Debug.LogWarning("packet len no match packetLen=" + packetLen + "real Len=" + list.Count);
                        continue;
                    }
                    list.RemoveAt(0);
                    list.RemoveAt(0);

                    //roomid
                    list.RemoveAt(0);
                    list.RemoveAt(0);

                    int enemyId = list[3] * 256 * 256 * 256 + list[2] * 256 * 256 + list[1] * 256 + list[0];
                    list.RemoveAt(0);
                    list.RemoveAt(0);
                    list.RemoveAt(0);
                    list.RemoveAt(0);
                   
                    VoiceChatPacket packet = new VoiceChatPacket();                 

                    packet.PacketId = list[3] * 256 * 256 * 256 + list[2] * 256 * 256 + list[1] * 256 + list[0];
                    list.RemoveAt(0);
                    list.RemoveAt(0);
                    list.RemoveAt(0);
                    list.RemoveAt(0);
                    //Debug.Log("packet.PacketId"+ packet.PacketId);

                    packet.CompressedSampleLen =  list[1] * 256 + list[0];
                    list.RemoveAt(0);
                    list.RemoveAt(0);

                    data = list.ToArray();
                    packet.Data = data;
                    VoiceChatRecorder.Instance.addPacketToPlayer(packet, enemyId);
                }
                catch (Exception ex)
                {
                    if (!(ex is SocketException))  
                         Debug.Log(ex.GetBaseException() + " "+ex.GetType()+" "+ ex.Message );
                }
                finally
                {
                }
            }
            udpClient.Close();
        }
    }

}
