using System;
using UnityEngine;

namespace VoiceChat
{
    public static class VoiceChatUtils
    {
        static void ToShortArray(this float[] input, short[] output)
        {
            if (output.Length < input.Length)
            {
                throw new System.ArgumentException("in: " + input.Length + ", out: " + output.Length);
            }

            for (int i = 0; i < input.Length; ++i)
            {
                output[i] = (short)Mathf.Clamp((int)(input[i] * 32767.0f), short.MinValue, short.MaxValue);
            }
        }

        static void ToFloatArray(this short[] input, float[] output, int length)
        {
            if (output.Length < length || input.Length < length)
            {
                throw new System.ArgumentException();
            }

            for (int i = 0; i < length; ++i)
            {
                output[i] = input[i] / (float)short.MaxValue;
            }
        }

        static byte[] ALawCompress(float[] input)
        {
            byte[] output = new byte[VoiceChatSettings.sampleSize];

            for (int i = 0; i < input.Length; ++i)
            {
                int scaled = (int)(input[i] * 32767.0f);
                short clamped = (short)Mathf.Clamp(scaled, short.MinValue, short.MaxValue);
                output[i] = NAudio.Codecs.ALawEncoder.LinearToALawSample(clamped);
            }

            return output;
        }

        static float[] ALawDecompress(byte[] input, int length)
        {
            float[] output = new float[VoiceChatSettings.sampleSize];

            for (int i = 0; i < length; ++i)
            {
                short alaw = NAudio.Codecs.ALawDecoder.ALawToLinearSample(input[i]);
                output[i] = alaw / (float)short.MaxValue;
            }

            return output;
        }

        static NSpeex.SpeexEncoder speexEnc = new NSpeex.SpeexEncoder(NSpeex.BandMode.Narrow);
        static NSpeex.SpeexDecoder speexDec = new NSpeex.SpeexDecoder(NSpeex.BandMode.Narrow);

        static byte[] SpeexCompress(float[] input, out int length)
        {
            short[] shortBuffer = new short[VoiceChatSettings.sampleSize];
            byte[] encoded = new byte[VoiceChatSettings.sampleSize];
            input.ToShortArray(shortBuffer);
            length = speexEnc.Encode(shortBuffer, 0, input.Length, encoded, 0, encoded.Length);
           // Debug.Log("compress length=" + length + " speexEnc.FrameSize" + speexEnc.FrameSize
           //     + " speexEnc.SampleRate" + speexEnc.SampleRate + " speexEnc.VBR" + speexEnc.VBR);
            return encoded;
        }

        static float[] SpeexDecompress(byte[] data, int dataLength)
        {
            float[] decoded = new float[VoiceChatSettings.sampleSize];
            short[] shortBuffer = new short[VoiceChatSettings.sampleSize];
            //Debug.Log("data.len="+ data .Length + "dataLength=" + dataLength);
            speexDec.Decode(data, 0, dataLength, shortBuffer, 0, false);
            shortBuffer.ToFloatArray(decoded, shortBuffer.Length);
            return decoded;
        }

        public static byte[] Compress(float[] sample, out int resultLen)
        {
            resultLen = 0;
            switch (VoiceChatSettings.compression)
            {
                case VoiceChatCompression.Alaw:
                    {
                        resultLen = sample.Length;
                        return ALawCompress(sample);
                    }
                case VoiceChatCompression.Speex:
                    {
                        byte[] resultBytes= SpeexCompress(sample, out resultLen);
                        Array.Resize(ref resultBytes, resultLen);
                        return resultBytes;
                    }
            }
            return null;
        }

        public static int Decompress( VoiceChatPacket packet, out float[] finalData)
        {
            finalData = new float[0];
            switch (VoiceChatSettings.compression)
            {
                case VoiceChatCompression.Speex:
                    {
                        finalData = SpeexDecompress( packet.Data, packet.CompressedSampleLen);
                        return finalData.Length;
                    }

                case VoiceChatCompression.Alaw:
                    {
                        finalData = ALawDecompress(packet.Data, packet.CompressedSampleLen);
                        return packet.CompressedSampleLen;
                    }
            }
            return 0;
        }
    } 
}