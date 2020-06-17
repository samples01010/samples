using System.Linq;
using UnityEngine;

namespace VoiceChat
{
    public enum VoiceChatCompression : byte
    {
        Alaw,
        Speex
    }
    public class VoiceChatPacket
    {
        public int CompressedSampleLen;
        public byte[] Data;
        public int PacketId;
    }
    public enum VoiceChatPreset
    {
        Speex_4K,
        Speex_8K,
        Speex_16K,
        Alaw_4k,
        Alaw_8k,
        Alaw_16k,
    }

    public class VoiceChatSettings 
    {

        public static int frequency = 16000;
        public static int sampleSize = 640;
        public static VoiceChatCompression compression = VoiceChatCompression.Speex;
        private static VoiceChatPreset preset = VoiceChatPreset.Speex_16K;

        public static VoiceChatPreset Preset
        {
            get { return preset; }
            set
            {
                preset = value;
                switch (preset)
                {
                    case VoiceChatPreset.Speex_4K:
                        frequency = 4000;
                        sampleSize = 160;
                        compression = VoiceChatCompression.Speex;
                        break;

                    case VoiceChatPreset.Speex_8K:
                        frequency = 8000;
                        sampleSize = 320;
                        compression = VoiceChatCompression.Speex;
                        break;

                    case VoiceChatPreset.Speex_16K:
                        frequency = 16000;
                        sampleSize = 640;
                        compression = VoiceChatCompression.Speex;
                        break;

                    case VoiceChatPreset.Alaw_4k:
                        frequency = 4096;
                        sampleSize = 128;
                        compression = VoiceChatCompression.Alaw;
                        break;

                    case VoiceChatPreset.Alaw_8k:
                        frequency = 8192;
                        sampleSize = 256;
                        compression = VoiceChatCompression.Alaw;
                        break;

                    case VoiceChatPreset.Alaw_16k:
                        frequency = 16384;
                        sampleSize = 512;
                        compression = VoiceChatCompression.Alaw;
                        break;
                }
            }
        }

        public static  double SampleTime
        {
            get { return (double)sampleSize / (double)frequency; }
        }
    } 
}