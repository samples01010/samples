using System;
using System.IO;
using System.Text;
using UnityEngine;


namespace Network
{
    public abstract class IncomingPacket
    {

        public abstract void SetData(int packetId, byte[] buffer);

        //обработчик для конца парсинга пакета.
        protected void CheckEndPacket__(int packetId, MemoryStream ms)
        {
            if (ms.Position != ms.Length)
            {
                Debug.Log("Packet read no finish: id="+packetId+" ms.Position= " 
                    + ms.Position + " ms.Length = " + ms.Length );
                ClientLog.LogBuffer(ms.ToArray(), 0);
            }
        }

        protected byte[] ReadBytes(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[(int)(ms.Length - ms.Position)];
            ms.Read(tmpBuffer, 0, (int)(ms.Length - ms.Position));
            return tmpBuffer;
        }

        protected string ReadString(MemoryStream ms)
        {
            string text = "";
            byte[] tmpBuffer = new byte[ms.Length + 2];
            byte[] smallBuffer = new byte[2];
            int bufferIndex = 0;
            while (ms.Position < ms.Length - 1)
            {
                ms.Read(smallBuffer, 0, 2);
                byte readByte = smallBuffer[0];
                byte readByteSecond = smallBuffer[1];
                if (readByte == 0 && readByteSecond == 0)
                {
                    break;
                }
                else
                {
                    tmpBuffer[bufferIndex] = readByte;
                    bufferIndex++;
                    tmpBuffer[bufferIndex] = readByteSecond;
                    bufferIndex++;
                }
            }
            byte[] outArray = new byte[bufferIndex];
            Array.Copy(tmpBuffer, outArray, bufferIndex);
            text = Encoding.Unicode.GetString(outArray);
            return text;
        }

        protected float ReadFloat(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[sizeof(long)];
            ms.Read(tmpBuffer, 0, sizeof(float));
            return BitConverter.ToSingle(tmpBuffer, 0);
        }

        protected long ReadInt64(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[sizeof(long)];
            ms.Read(tmpBuffer, 0, sizeof(long));
            return BitConverter.ToInt64(tmpBuffer, 0);
        }

        protected int ReadInt32(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[sizeof(int)];
            ms.Read(tmpBuffer, 0, sizeof(int));
            return BitConverter.ToInt32(tmpBuffer, 0);
        }
        

        protected int ReadInt16(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[sizeof(short)];
            ms.Read(tmpBuffer, 0, sizeof(short));
            return (int)BitConverter.ToInt16(tmpBuffer, 0);
        }

        protected int ReadInt8(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[1];
            ms.Read(tmpBuffer, 0, 1);
            return (int)tmpBuffer[0];
        }

        protected bool ReadBool(MemoryStream ms)
        {
            byte[] tmpBuffer = new byte[1];
            ms.Read(tmpBuffer, 0, 1);
            if (tmpBuffer[0] == 1)
                return true;
            else
                return false;
        }

        protected EnemyPacketData ReadEnemyShortInfo( MemoryStream ms, bool withNick)
        {
            EnemyPacketData data = new EnemyPacketData();

            data.id=ReadInt32(ms);
            if (data.id == 0)
                return data;

            if (withNick)
                data.nick=ReadString(ms);

            data.x=ReadFloat(ms);
            data.y = ReadInt16(ms) / 128f;
            data.z= ReadFloat(ms);

            data.h = ReadInt8(ms) * 3;
            data.hY = ReadInt8(ms) * 2;

            data.isDead= ReadBool(ms);
            data.weapon = ReadInt16(ms);

            return data;
        }

        protected ItemW ReadItem(MemoryStream ms)
        {
            ItemW item = new ItemW();
            int id = ReadInt16(ms);
            if (id == 0)
                return null;

            item.id = id;
            item.count = ReadInt32(ms);
            item.enchantLvl = ReadInt8(ms);
            item.lifeTime = ReadInt16(ms);
            return item;
        }


    }
}
