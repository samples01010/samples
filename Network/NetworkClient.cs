using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{

    //основной класс, отправляющий и принимающий пакеты.
    public class NetworkClient
    {
        const int SEND_BUFFER_SIZE = 16384;
        const int READ_BUFFER_SIZE = 1024 * 1024;
        const int MAX_INCOMING_PACKET_SIZE = 65 * 1024;
        const int PACKET_HEADER_SIZE = 4;

        public TcpClient client;
        private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
        private byte[] receivedBuffer = new byte[READ_BUFFER_SIZE];
        private int receivedBufferPos = 0;

        public int key = 0;

        private NetworkClientIncomingPacketParser packetParser;

        private bool clientConnected = false;

        public NetworkClient(NetworkClientIncomingPacketParser packParser)
        {
            packetParser = packParser;
        }

        // =============== коннект =============================

        public bool ConnectToServer()
        {
            //if (testMode)
            //{
            //    clientConnected = true;
            //    testRun();
            //    return false;
            //}

            string serverHost = Settings.serverHost77;
            if (Settings.clientIsInLocalDevelopMode)
                serverHost = Settings.serverLocalHost77;
            try
            {
                client = new TcpClient
                {
                    NoDelay = true,
                    SendBufferSize = SEND_BUFFER_SIZE,
                    ReceiveBufferSize = READ_BUFFER_SIZE
                };

                var result = client.BeginConnect(serverHost, Settings.serverPort77, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                if (!success)
                {
                    throw new Exception("Failed to connect.");
                }
                client.EndConnect(result);

                client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
                if (Settings.packetLoggingEnabled)
                    Debug.Log("Connection Succeeded");
                clientConnected = true;
                return true;
            }
            catch (Exception e)
            {
                client.Close();
                Debug.Log(e);
                if (Settings.packetLoggingEnabled)
                    Debug.Log("SERVER OFFLINE");
                return false;
            }
        }

        public void Disconnect(bool showErrorWindow)
        {
            ClearBuffer();
            client.Close();
            clientConnected = false;
            if (showErrorWindow)
                ReportConnectionError_();
            if (Game.instance.inTheWorld)
                CharMotor.blockControlsAfterDisconnect = true;
        }



        // =============== отправка пакетов=============================
        public void SendPacket(OutgoingPacket packet)
        {
            if (!clientConnected)
                return;

            byte[] data = packet.GetData();

            ClientLog.LogPacket("client=>server ", 0, data, false);

            //добавляем длину пакета
            var list = new List<byte>(data);

            byte[] cryptCheaper = new byte[2];
            int a1 = UnityEngine.Random.Range(6, 250);
            int a2 = UnityEngine.Random.Range(1, 31);
            cryptCheaper[0] = (byte)a1;
            cryptCheaper[1] = (byte) a2;

            list.InsertRange(0, cryptCheaper);

            list.InsertRange(0, BitConverter.GetBytes((Int16)(data.Length + 4)));

            data = list.ToArray();

            bool thisIsLogoutPacket = false;
            if (packet is SendLogout00)
                thisIsLogoutPacket = true;

            SendData(data, thisIsLogoutPacket);
        }

        private void SendData(byte[] sendBuffer, bool thisIsLogoutPacket)
        {
            try
            {
                if (!thisIsLogoutPacket)
                {
                    client.GetStream().BeginWrite(sendBuffer, 0, sendBuffer.Length, new AsyncCallback(DowriteEnd), null);
                }
                else
                {
                    BinaryWriter writer = new BinaryWriter(client.GetStream());
                    writer.Write(sendBuffer);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        // =============== прием пакетов=============================

        private void DoRead(IAsyncResult ar)
        {
            if (!clientConnected)
            {
                ClearBuffer();
                return;
            }

            int countBytesRead;
            try
            {
                //if (!testMode)
                countBytesRead = client.GetStream().EndRead(ar);

                //это код ошибки соединения. разрываем соединение.
                if (countBytesRead < 1)
                {
                    Disconnect(true);
                    return;
                }

                //if (testMode)
                //    countBytesRead = testreciviedCountBytes;

                //сохраняем в рабочий буфер, то что пришло из только что полученного.
                Array.Copy(readBuffer, 0, receivedBuffer, receivedBufferPos, countBytesRead);
                receivedBufferPos += countBytesRead;
                if (receivedBufferPos < 5)
                {
                    Debug.Log("not bug. dont worry. info-packet header no full");
                    BeginRead();
                    return;
                }

                int packetSizeWithHeader = GetPacketSizeWithHeaderSize(0);

                //ClientLog.LogFullBuffer(readBuffer);
                // ClientLog.LogBuffer(receivedBuffer, 200);

                if (IsWrongPacketSize(packetSizeWithHeader))
                {
                    ClearBuffer();
                    BeginRead();
                    return;
                }

                // проверка того что пакет пришел полностью. в буфере меньше чем длина пакета.
                if (packetSizeWithHeader > receivedBufferPos)
                {
                    BeginRead();
                    return;
                }

                int iterations = 0;
                //читаем пакеты из массива перемещая индекс на положение нового пакета
                int NextPacketIndex = 0;
                while (iterations < 300)
                {
                    iterations++;

                    //определяем размер пакета без заголовка
                    
                    int packetSize = GetPacketSizeWithHeaderSize(NextPacketIndex) - PACKET_HEADER_SIZE;
                    if (IsWrongPacketSize(packetSize))
                    {
                        ClearBuffer();
                        BeginRead();
                        return;
                    }

                    //Debug.Log("a=" + "b=" + packetSize);

                    //создаем массив содержащий пакет без заголовка                   
                    byte[] packetBuffer = new byte[packetSize];
                    Array.Copy(receivedBuffer, NextPacketIndex + PACKET_HEADER_SIZE, packetBuffer, 0, packetSize);

                    //передача единичного пакета в основной поток на обработку. 
                    packetParser.AddPacket(packetBuffer);
                    if (Settings.packetLoggingEnabled)
                        ClientLog.LogPacket("add", packetBuffer[0], packetBuffer, true);

                    //увеличиваем индекс на положение следующего пакета
                    NextPacketIndex += packetSize + PACKET_HEADER_SIZE;

                    //буфер достиг конца.
                    if (NextPacketIndex >= receivedBufferPos)
                    {
                        ClearBuffer();
                        BeginRead();
                        return;
                    }

                    //в буфере еще не пришла вся инфо о длине следующего пакета
                    if (receivedBufferPos - NextPacketIndex < 5)
                    {
                        // Debug.Log("not bug2. dont worry. packet header no full");
                        CompactBuffer(NextPacketIndex);
                        BeginRead();
                        return;
                    }

                    //этот пакет пришел не до конца. ждем когда придет оставщийся кусок.
                    int packetSizeWithHeader2 = GetPacketSizeWithHeaderSize(NextPacketIndex);
                    if (packetSizeWithHeader2 + NextPacketIndex > receivedBufferPos)
                    {
                        //Debug.Log("not bug. packet no full");
                        CompactBuffer(NextPacketIndex);
                        BeginRead();
                        return;
                    }
                    //значит в буфере есть еще готовый пакет. возвращаемся к началу цикла.
                }
            }
            catch (IOException ex)
            {
                Debug.Log("errorIO: " + ex);
                Disconnect(true);
                return;
            }
            catch (Exception ex)
            {
                Debug.Log("error : " + ex);
                Disconnect(true);
            }
        }


        private bool IsWrongPacketSize(int packetSize)
        {
            // слишком длинный  или слишком короткий пакет.
            if (packetSize <= 0 || packetSize > MAX_INCOMING_PACKET_SIZE)
            {
                Debug.LogWarning("wrong size incoming packet. size=" + packetSize);
                return true;
            }
            return false;
        }

        private void DowriteEnd(IAsyncResult arr)
        {
        }

        private int GetPacketSizeWithHeaderSize(int index)
        {
            byte[] sizeArray = new byte[4];
            sizeArray[0] = receivedBuffer[index];
            sizeArray[1] = receivedBuffer[index + 1];
            sizeArray[2] = receivedBuffer[index + 2];
            sizeArray[3] = receivedBuffer[index + 3];
            int packetSize = (int)BitConverter.ToInt32(sizeArray, 0);
            return packetSize;
        }

        private void BeginRead()
        {
            //if (testMode)
            //    return;
            client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
        }

        private void ClearBuffer()
        {
            receivedBuffer = new byte[READ_BUFFER_SIZE];
            receivedBufferPos = 0;
        }

        //обрезаем буфер до длины первого пакета.
        private void CompactBuffer(int index)
        {
            //ClientLog.LogBuffer(receivedBuffer, 500);
            byte[] newBuffer = new byte[READ_BUFFER_SIZE];
            Array.Copy(receivedBuffer, index, newBuffer, 0, receivedBufferPos - index);
            receivedBuffer = newBuffer;
            receivedBufferPos = receivedBufferPos - index;
           // ClientLog.LogBuffer(receivedBuffer, 500);
        }

        private void ReportConnectionError_()
        {
            GUIWindowsProducer.instance.CreateNewWindow99(LanguageController.GetWord(Words.LOST_CONNECTION_TO_SERVER), true, true, false);
        }




    }

}