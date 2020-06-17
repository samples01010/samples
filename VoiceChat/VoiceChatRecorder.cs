using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace VoiceChat
{

    public class VoiceChatRecorder : MonoBehaviour
    {

        static VoiceChatRecorder instance;

        public static VoiceChatRecorder Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType(typeof(VoiceChatRecorder)) as VoiceChatRecorder;
                return instance;
            }
        }

        [HideInInspector]
        public AudioClip audioClip { get { return clip; } }

        public   List<VoiceChatPlayer> voiceChatPlayers = new List<VoiceChatPlayer>();
        private  List<GameObject> voicePlayerPanel = new List<GameObject>();

        public GameObject voicePlayerPanelPrefab;
        public Transform voicePlayerPanelParent;

        int packetId=10;
        int previousPosition = -1;
        int sampleIndex = 0;
        AudioClip clip = null;
        bool recording = false;
        int recordFrequency = 0;
        int recordSampleSize = 0;
        float[] sampleBuffer = null;
        int totalSendedBytes = 0;
        int delayForStopRecording = 0;

        public void AwakeChat()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                Debug.LogError("Only one instance of VoiceChatRecorder can exist");
                return;
            }
            instance = this;

            foreach (VoiceChatPlayer player in voiceChatPlayers)
            {
                 player.StartChat();
            }

            Application.RequestUserAuthorization(UserAuthorization.Microphone);

            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("Mic not found");
                return;
            }
           // VoiceChatSettings.Preset = VoiceChatPreset.Speex_8K;
            VoiceChatClient.StartClient();
        }

        void Update()
        {
            if (!Settings.voiceChatEnabled)
                return;

            if (InputManager.KeyIsPressedNow00("VoiceChat"))
            {
                delayForStopRecording = 0;
                if (!recording)
                {
                    StartRecording();
                }
            }
            else
            if (InputManager.KeyIsUnPressNow00("VoiceChat"))
            {
                if (recording)
                    delayForStopRecording = 30;                 
            }
            if (delayForStopRecording>0)
            {
                delayForStopRecording--;
                if (delayForStopRecording == 1)
                {
                    StopRecording();
                    delayForStopRecording = 0;
                }
            }

            if (!recording)
                return;

            int currentPosition = Microphone.GetPosition(null);

            if (currentPosition < previousPosition && previousPosition>=0)
            {
                while (sampleIndex < recordFrequency)
                {
                    ReadSample(recording);
                }
                sampleIndex = 0;
            }

            previousPosition = currentPosition;

            while (sampleIndex + recordSampleSize <= currentPosition)
            {
                ReadSample(recording);
            }
        }

        void Resample(float[] src, float[] dst)
        {
            if (src.Length == dst.Length)
            {
                Array.Copy(src, 0, dst, 0, src.Length);
            }
            else
            {
                float rec = 1.0f / (float)dst.Length;

                for (int i = 0; i < dst.Length; ++i)
                {
                    float interp = rec * (float)i * (float)src.Length;
                    dst[i] = src[(int)interp]*Settings.micVolume;
                }
            }
        }

        void ReadSample(bool transmit)
        {
            clip.GetData(sampleBuffer, sampleIndex);

            float[] targetSampleBuffer = new float[VoiceChatSettings.sampleSize];
            Resample(sampleBuffer, targetSampleBuffer);

         //   string str = "";
         //   for (int i = 0; i < 50; i++)
         //   {
         //       str += " " + targetSampleBuffer[i];
         //   }
         //   Debug.Log("a=" + str);

            sampleIndex += recordSampleSize;
            
            if (transmit)
                TransmitBuffer(targetSampleBuffer);
        }

        VoiceChatPacket finalPacket = null;
        void TransmitBuffer(float[] buffer)
        {
            int compressedSize = 0;
            byte[] compressedData = VoiceChatUtils.Compress(buffer, out compressedSize);
            if (finalPacket != null)
            {
                //Добавляем новый кусок в конец массива в пакете
                int endOfPacket = finalPacket.Data.Length;
                byte[] resultArray = new byte[endOfPacket + compressedData.Length];
                for (int i = 0; i < finalPacket.Data.Length; i++)
                    resultArray[i] = finalPacket.Data[i];
                for (int i = 0; i < compressedData.Length; i++)
                    resultArray[i + endOfPacket] = compressedData[i];
                finalPacket.Data = resultArray;

                //если массив достаточно маленький или сжатых аудиокусков мало не отправляем.
                if (finalPacket.Data.Length < 400 && (finalPacket.Data.Length/ finalPacket.CompressedSampleLen) < 4 )
                    return;
            }
            else
            {
                finalPacket = new VoiceChatPacket();
                finalPacket.CompressedSampleLen = compressedSize;
                finalPacket.Data = compressedData;

                packetId += 20;
                finalPacket.PacketId = packetId;

                //Debug.Log("create Packet. base Length= " + finalPacket.CompressedSampleLen + " array Length= " + finalPacket.Data.Length );
                
                //алав жмет плохо. поэтому пакет отправляется сразу
                if (VoiceChatSettings.compression == VoiceChatCompression.Speex)
                    return;
            }
           // Debug.Log("send Packet. base Length= " + finalPacket.CompressedSampleLen +  " array Length= " + finalPacket.Data.Length);

            totalSendedBytes += finalPacket.Data.Length;
           // Debug.Log("totalSendedBytes=" + totalSendedBytes);   
            VoiceChatClient.SendPacket(finalPacket);
            finalPacket = null;
        }

        public bool StartRecording()
        {
            if (recording)
            {
                Debug.LogError("Already recording");
                return false;
            }
            finalPacket = null;

            int minFreq;
            int maxFreq;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

            recordFrequency = minFreq == 0 && maxFreq == 0 ? 44100 : maxFreq;
            recordSampleSize = recordFrequency / (VoiceChatSettings.frequency / VoiceChatSettings.sampleSize);

            clip = Microphone.Start(null, true, 1, recordFrequency);
            sampleBuffer = new float[recordSampleSize];
            recording = true;
            return recording;
        }

        public void StopRecording()
        {
            Microphone.End(null);
            Destroy(clip);
            clip = null;
            recording = false;
            finalPacket = null;
            sampleIndex = 0;
            previousPosition = -1;
        }

        public bool CheckMutedAndIndicateTalk(int enemyId)
        {
            foreach (GameObject go in voicePlayerPanel)
            {
                VoiceChatIndicatorContoller controller = go.GetComponent<VoiceChatIndicatorContoller>();
                if (controller.voicePlayer.enemyServerId == enemyId)
                {
                    if (controller.muted)
                        return true;
                    else
                    {
                        controller.OnPlayerTalk();
                        return false;
                    }
                    
                }
            }
            //Debug.Log("no player in voice chat=" + enemyId);
            return false;
        }

        private bool notifed = false;
        public void addPacketToPlayer(VoiceChatPacket newPacket, int enemyId)
        {
            for (int i = 0; i < voiceChatPlayers.Count; i++)
            {
                if (voiceChatPlayers[i].enemyServerId == enemyId)
                {
                    voiceChatPlayers[i].OnNewPacket(newPacket);
                    return;
                }
            }
            if (Settings.clientIsInLocalDevelopMode)
            {
                //когда сервер пересылает пакет  обратно для отладки.
                if (!notifed)
                    Debug.Log("send back");
                notifed = true;
                voiceChatPlayers[0].OnNewPacket(newPacket);
            }
            else
                Debug.Log("not find player in voice chat=" + enemyId);
        }

        //очишаем инфо о игроках в руме и голосовом чате на панельке
        public void ClearPlayersIds()
        {
            foreach (VoiceChatPlayer player in voiceChatPlayers)
                player.enemyServerId = 0;
            foreach (GameObject go in voicePlayerPanel)
            {
                Destroy(go);
            }
            voicePlayerPanel.Clear();
        }

        public void AddPlayerId(int enemyId, string name)
        {
            Debug.Log("enemyId=" + enemyId + " name=" + name + " c=" + 1);
            for (int i=0; i<voiceChatPlayers.Count; i++)  
            {
                VoiceChatPlayer player = voiceChatPlayers[i];
                if (player.enemyServerId == 0)
                {
                    player.enemyServerId = enemyId;
                    GameObject clone = Instantiate(voicePlayerPanelPrefab, voicePlayerPanelParent, true) as GameObject;
                    clone.SetActive(true);
                    clone.transform.SetParent(voicePlayerPanelParent, false);
                    clone.GetComponentInChildren<UnityEngine.UI.Text>().text = name;
                    clone.GetComponent<VoiceChatIndicatorContoller>().voicePlayer = player;
                    voicePlayerPanel.Add(clone);
                    return;
                }
            }
            Debug.LogWarning("no free players in voice chat=" + enemyId);
        }


    }



}