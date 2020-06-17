using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Network;
using System;

//инициализация сетевого обмена и авторизация. 
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public enum LoginProgress { Start, ProtocolVersionSended, ReadyForAuth, AuthSended00, ErrorAuth00, Authed000, Done };
    [HideInInspector]
    public LoginProgress currentProgress = LoginProgress.Start;

    //кнопки которые блокируются на время процесса логина.
    public Button loginButton;
    public Button registrationButton;

    //используется для тестового запуска нескольких копий этого класса и симуляции подключения большого количества игроков.
    [HideInInspector]
    public bool inTestMode = false;
    [HideInInspector]
    public string testClientLogin = "";
    [HideInInspector]
    public string testClientPassword = "";

    public NetworkClient netClient = null;
    //   private bool tryConnectingNow = false;
    private NetworkClientIncomingPacketParser packetParser;

    private DateTime TimeSendKeepAlivePacket;
    private DateTime AnswerTimeKeepAlivePacket;
    [HideInInspector]
    public string login_00;
    [HideInInspector]
    public string password_00;
    [HideInInspector]
    public string nick_00;
    [HideInInspector]
    public string email;
    [HideInInspector]
    public bool registerNewAccount00;

    public bool doConnectToServer00 = false;

    [HideInInspector]
    public string currentServerPing = "";

    void Awake()
    {
        if (instance != null)
        {
            if (!Settings.clientIsInTestingServerMode)
                Debug.Log("Warning. This Is never Happens. Check it. instance != null");
            return;
        }
        instance = this;
    }

    void Start()
    {
        packetParser = new NetworkClientIncomingPacketParser();
    }

    void Update()
    {
        if (!doConnectToServer00)
            return;

        //непосредственно разбирать полученные пакеты можно только в основном потоке.
        packetParser.CheckAndParseIncomingPackets(this);

        switch (currentProgress)
        {
            //процедура авторизации.
            case LoginProgress.Start:
                netClient = new NetworkClient(packetParser);
                if (netClient.ConnectToServer())
                {
                    netClient.SendPacket(new SendProtocolVersion00());
                    currentProgress = LoginProgress.ProtocolVersionSended;
                }
                else
                {
                    UnlockLoginButtons();
                    doConnectToServer00 = false;
                    GUIWindowsProducer.instance.CreateNewWindow99(
                        LanguageController.GetWord(Words.COULD_NOT_CONNECT_TO_SERVER_), true, true, false);
                }
                break;

            case LoginProgress.ReadyForAuth:
                netClient.SendPacket(new SendAuth00(this));
                currentProgress = LoginProgress.AuthSended00;
                break;

            case LoginProgress.ErrorAuth00:
                UnlockLoginButtons();
                doConnectToServer00 = false;
                currentProgress = LoginProgress.Start;
                break;

            case LoginProgress.Authed000:
                //при успешном входе в игру сохраняем авторизационные данные  
                Settings.clientLogin77 = login_00;
                Settings.clientPassword77 = password_00;
                Settings.SaveSettings();

                currentProgress = LoginProgress.Done;
                StartCoroutine(KeepAlivePacketSender());
                netClient.SendPacket(new SendPlayerEnter00());
                break;
            case LoginProgress.Done:
                //процесс завершен.
                break;
            default:
                break;
        }
    }

    //посылаем серверу пакет, что бы он знал что клиент на связи.
    private IEnumerator KeepAlivePacketSender()
    {
        const int KEEPALIVE_PACKET_SEND_PERIOD_SECONDS = 20;
        while (true)
        {
            if (currentProgress == LoginProgress.Done)
            {
                yield return new WaitForSeconds(KEEPALIVE_PACKET_SEND_PERIOD_SECONDS / 2f);

                //если так, то значит прежний ответ не был получен.
                if (TimeSendKeepAlivePacket > AnswerTimeKeepAlivePacket)
                    currentServerPing = "<color=#ff0000ff>-1</color>";

                yield return new WaitForSeconds(KEEPALIVE_PACKET_SEND_PERIOD_SECONDS / 2f);
                netClient.SendPacket(new SendKeepAlive00());
                TimeSendKeepAlivePacket = DateTime.Now;
            }
            yield return null;
        }
    }

    //получив ответ на KeepAlive пакет узнаем пинг.
    public void OnKeepAlivePacketRecive()
    {
        AnswerTimeKeepAlivePacket = DateTime.Now;
        long ping = (AnswerTimeKeepAlivePacket.Ticks - TimeSendKeepAlivePacket.Ticks) / 10000;
        if (ping > 300)
            currentServerPing = "<color=#ff0000ff>" + ping.ToString() + "</color>";
        else
            currentServerPing = ping.ToString();
    }

    public void DoConnectToserver(string login____, string password_____, string nick____, string email____, bool registerNewAccount____)
    {
        string possibleErrors = "";
        if (login____.Length < 4)
            possibleErrors += LanguageController.GetWord(Words.ERROR_LOGIN_IS_TOO_SHORT) + "\n";
        if (password_____.Length < 4)
            possibleErrors += LanguageController.GetWord(Words.ERROR_PASSWORD_IS_TOO_SHORT) + "\n";
        if (registerNewAccount____)
        {
            if (nick____.Length < 4)
                possibleErrors += LanguageController.GetWord(Words.ERROR_NICK_IS_TOO_SHORT) + "\n";
        }

        //строка ошибок не пуста значит не запускаем процесс подключения к серверу
        if (possibleErrors.Length > 0)
        {
            GUIWindowsProducer.instance.CreateNewWindow99(possibleErrors, true, true, false);
            return;
        }

        login_00 = login____;
        password_00 = password_____;
        nick_00 = nick____;
        email = email____;
        registerNewAccount00 = registerNewAccount____;
        doConnectToServer00 = true;
        LockLoginButtons();
    }

    public void SendPacket(OutgoingPacket op)
    {
        if (netClient != null)
            netClient.SendPacket(op);
    }

    public void Disconnect()
    {
        if (netClient != null)
            netClient.Disconnect(false);
    }

    void LockLoginButtons()
    {
        registrationButton.interactable = false;
        loginButton.interactable = false;
    }

    void UnlockLoginButtons()
    {
        registrationButton.interactable = true;
        loginButton.interactable = true;
    }

}
