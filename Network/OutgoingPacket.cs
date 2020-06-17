using System;
using System.IO;
using System.Text;

namespace Network
{

    public abstract class OutgoingPacket
    {

        public byte[] GetData()
        {
            using (var ms = new MemoryStream())
            {
                var msgBytes = GetPacketData();

                byte[] bytes = new byte[4];
                bytes = BitConverter.GetBytes(NetworkManager.instance.netClient.key);
                ms.Write(bytes, 0, bytes.Length);

                ms.Write(msgBytes, 0, msgBytes.Length);
                return ms.ToArray();
            }
        }

        protected abstract byte[] GetPacketData();

        protected void WriteString(string str, MemoryStream ms)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            byte[] bytesZero = { 0, 0 };
            ms.Write(bytes, 0, bytes.Length);
            ms.Write(bytesZero, 0, bytesZero.Length);
        }

        protected void WriteFloat(float val, MemoryStream ms)
        {
            byte[] bytes = new byte[4];
            bytes = BitConverter.GetBytes(val);
            ms.Write(bytes, 0, bytes.Length);
        }

        protected void WriteInt64(long val, MemoryStream ms)
        {
            byte[] bytes = new byte[8];
            bytes = BitConverter.GetBytes(val);
            ms.Write(bytes, 0, bytes.Length);
        }

        protected void WriteInt32(int val, MemoryStream ms)
        {
            byte[] bytes = new byte[4];
            bytes = BitConverter.GetBytes( val);
            ms.Write(bytes, 0, bytes.Length);
        }

        protected void WriteInt16(int val, MemoryStream ms)
        {          
            byte[] bytes = new byte[2];
            bytes = BitConverter.GetBytes((Int16)val);
            ms.Write(bytes, 0, bytes.Length);
        }

        protected void WriteInt8(int val, MemoryStream ms)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte) val;
            ms.Write(bytes, 0, bytes.Length);
        }

        protected void WriteBool(bool val, MemoryStream ms)
        {
            byte[] bytes = new byte[1];
            if (val)
                bytes[0] = 1;
            ms.Write(bytes, 0, bytes.Length);
        }

    }
}
