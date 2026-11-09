using GameServer;
using System.Linq;
using TMPro;
using UnityEngine;

public class Menu_Inicial_UI : MonoBehaviour
{
    internal static Menu_Inicial_UI instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Meu_ip_Area.SetTextWithoutNotify(Local_Client.meu_IP);
    }
    #region Meu próprio IP publico
    /// <summary>
    /// O ip publico é algo tranquilo de se compartilhar.
    /// Só tenha em mente que é tipo CPF, não é para sair dando pra todo mundo
    /// </summary>
    [SerializeField]
    TMP_InputField Meu_ip_Area;
    TimerToken _TK_Meu_IP_Place_Holder;
    TimerToken _TK_Meu_IP_TEXT;
    [SerializeField]
    TextMeshProUGUI Meu_ip_pacle_Holder;
    [SerializeField]
    TextMeshProUGUI Meu_ip_TEXT;
    public void Definir_Proprio_IP_TMP(string ip)
    {
        // Por enquanto que a conexão é apenas por IPv4, ele precisa ter o padrão IPv4
        if (ip.Contains(":") || ip.Any(char.IsLetter) || ip.Count(c => c == '.') != 3)
        {
            if (_TK_Meu_IP_Place_Holder != null)
                _TK_Meu_IP_Place_Holder.Dispose();

            _TK_Meu_IP_Place_Holder = Timer.FeedBack_De_Texto("Introduza seu IP", "IP Inválido", Meu_ip_pacle_Holder, 1.5f, 2, Color.red);


            if (_TK_Meu_IP_TEXT != null)
                _TK_Meu_IP_TEXT.Dispose();

            _TK_Meu_IP_TEXT = Timer.FeedBack_De_Texto(Local_Client.meu_IP, "", Meu_ip_TEXT, 1.5f, 2, Color.clear);
        }

        Local_Client.meu_IP = ip;
        Opcoes.SalvarConfiguracoes();
    }
    public void Atualizar_Meu_IP()
    {
        Meu_ip_Area.SetTextWithoutNotify(Local_Client.meu_IP);
    }
    public void Copiar_Meu_IP()
    {
        GUIUtility.systemCopyBuffer = Local_Client.meu_IP;
    }
    #endregion
    public GameObject Menu_Inicial;
    public GameObject Sala;
    public GameObject Opções;
    internal void abrir_Sala()
    {
        Local_Client.Conectado = true;
        Menu_Inicial.SetActive(false);
        Sala.SetActive(true);
        Opções.SetActive(false);
        Sala_UI.instance.Definir_Info_Sala();
    }
    internal void abrir_Menu_Inicial()
    {
        Menu_Inicial.SetActive(true);
        Sala.SetActive(false);
        Opções.SetActive(false);
        TransparencyWindowController.instance.RestoreWindow();
    }
    public void Hostear()
    {
        //Abre um servidor e abre a aba de SALA
        Server.Start(10);
        //Local_Client.instance.Hostear();
        abrir_Sala();
    }
    public void Cenectar()
    {
        print(Local_Client.IP_PORT);
        if (NetworkUtils.IS_IP_PORT_Valid(Local_Client.IP_PORT))
        {
            //Tenta se conectar com o Host, caso dê tudo certo, abra a aba de SALA
            Local_Client.instance.ConnectToServer();
        }
        else
            FEEDBACK_IP_INVÁLIDO();
    }
    public TMP_InputField ip_Area;
    TimerToken _TK_Definir_IP;
    [SerializeField]
    TextMeshProUGUI _definir_IP_PlaceHolder;
    public void Definir_IP(string ip)
    {
        if (NetworkUtils.IS_IP_PORT_Valid(ip))
        {
            if (Local_Client.instance != null)
            {
                ip_Area.SetTextWithoutNotify(ip);
                string[] ip_port = ip.Split(new char[] { ':' });
                Local_Client.instance.ip = ip_port[0];
                if (ip_port.Length > 1)
                {
                    if (int.TryParse(ip_port[1], out int port))
                        Local_Client.instance.port = port;
                    else
                        FEEDBACK_IP_INVÁLIDO();
                }
                else
                    FEEDBACK_IP_INVÁLIDO();
            }
            else
                Avisos.instance.Mostrar_Aviso("<color=red>[ERRO]</color> Local_Client não foi iniciado. RETORTE IMEDIATAMENTE!", 10, true);
        }
        else
        {
            FEEDBACK_IP_INVÁLIDO();
        }
    }
    void FEEDBACK_IP_INVÁLIDO()
    {
        if (_TK_Definir_IP != null)
            _TK_Definir_IP.Dispose();

        ip_Area.SetTextWithoutNotify("");
        _TK_Definir_IP = Timer.FeedBack_De_Texto("Insira o IP:Port da Sala", "IP:Port INVÁLIDO!", _definir_IP_PlaceHolder, 1.5f, 2, Color.red);
    }
    public void AbrirOpções()
    {
        Menu_Inicial.SetActive(false);
        Opções.SetActive(true);
    }
    public void FecharOpções()
    {
        if (Local_Client.Conectado)
            abrir_Sala();
        else
            abrir_Menu_Inicial();
    }
    public void Abrir_Canal_Youtube()
    {
        Application.OpenURL("https://www.youtube.com/@flariquee");
    }
    public void Abrir_X()
    {
        Application.OpenURL("https://x.com/_flarique");
    }
    public void Abrir_GIT()
    {
        Application.OpenURL("https://github.com/AndradiusV");
    }
    public void Sair()
    {
        Application.Quit();
    }
}
