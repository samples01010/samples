using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceChatPlayer : MonoBehaviour
    {
        [HideInInspector]
        public int enemyServerId = 0;

        float lastTime = 0;
        double played = 0;
        double received = 0;
        int index = 0;
        float[] data;
        bool shouldPlay = false;

        SortedList<int, VoiceChatPacket> packetsToPlay = new SortedList<int, VoiceChatPacket>();

        public void StartChat()
        {
            int size = VoiceChatSettings.frequency * 10;
            GetComponent<AudioSource>().loop = true;
            GetComponent<AudioSource>().clip = AudioClip.Create("VoiceChat", size, 1, VoiceChatSettings.frequency, false);
            data = new float[size];
        }

        void Update()
        {
            if (!Settings.voiceChatEnabled)
                return;

            if (GetComponent<AudioSource>().isPlaying)
            {
                if (lastTime > GetComponent<AudioSource>().time)
                    played += GetComponent<AudioSource>().clip.length;

                lastTime = GetComponent<AudioSource>().time;

                if (played + GetComponent<AudioSource>().time >= received)
                {
                    Stop();
                    shouldPlay = false;
                }
            }
            else
            {
                if (shouldPlay)
                {
                    GetComponent<AudioSource>().Play();
                }
            }
            TryPlay();
        }

        void Stop()
        {
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().time = 0;
            index = 0;
            played = 0;
            received = 0;
            lastTime = 0;
            packetsToPlay.Clear();
        }

        public void OnNewPacket(VoiceChatPacket newPacket)
        {
            if (packetsToPlay.ContainsKey(newPacket.PacketId))
            {
                Debug.Log("never happens? 22");
                return;
            }
            //разбиваем пакет на семплы если внутри их больше
            if (newPacket.Data.Length>newPacket.CompressedSampleLen)
            {             
                for (int i=0; i< 20; i++)
                {
                    if (newPacket.CompressedSampleLen * i < newPacket.Data.Length)
                    {
                        byte[] tempCompressData = new byte[newPacket.CompressedSampleLen];
                        for (int i2 = 0; i2 < tempCompressData.Length; i2++)
                            tempCompressData[i2] = newPacket.Data[i2 + i * newPacket.CompressedSampleLen];
                        VoiceChatPacket partPacket = new VoiceChatPacket();
                        partPacket.PacketId = newPacket.PacketId + i;
                        partPacket.Data = tempCompressData;
                        partPacket.CompressedSampleLen = newPacket.CompressedSampleLen;
                        packetsToPlay.Add(partPacket.PacketId, partPacket);
                    }
                    else
                        break;
                }
                return;
            }
            packetsToPlay.Add(newPacket.PacketId, newPacket);
        }

        private  void TryPlay()
        {
            if (packetsToPlay.Count < 10)
                return;

            if (VoiceChatRecorder.Instance.CheckMutedAndIndicateTalk(enemyServerId))
            {
                packetsToPlay.Clear();
                return;
            }

            var pair = packetsToPlay.First();
            var packet = pair.Value;
            packetsToPlay.Remove(pair.Key);

            float[] sample = null;
            int length = VoiceChatUtils.Decompress(packet, out sample);

            if (Settings.voiceVolume != 1f)
            {
                for (int i = 0; i < sample.Length; ++i)
                {
                    sample[i] = sample[i] * Settings.voiceVolume;
                }
            }

            received += VoiceChatSettings.SampleTime;

            Array.Copy(sample, 0, data, index, length);

            index += length;

            if (index >= GetComponent<AudioSource>().clip.samples)
            {
                index = 0;
            }
            GetComponent<AudioSource>().clip.SetData(data, 0);
            if (!GetComponent<AudioSource>().isPlaying)
            {
                shouldPlay = true;
            }
        }
    } 
}