using GameServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.WebRTC;
using UnityEngine;

public class Menu_Inicial_UI : MonoBehaviour
{
    internal static Menu_Inicial_UI instance;
    private void Awake()
    {
        instance = this;
        Definir_IP("127.0.0.1");
    }
    private void Start()
    {
        Meu_ip_Area.SetTextWithoutNotify(Local_Client.meu_IP);
    }
    #region Meu próprio IP publico
    /* O ip publico é algo tranquilo de se compartilhar.
     * Só tenha em mente que é tipo CPF, não é para sair dando pra todo mundo
     */
    public TMP_InputField Meu_ip_Area;
    public void Definir_Proprio_IP(string ip)
    {
        // Por enquanto que a conexão é apenas por IPv4, ele precisa ter o padrão IPv4
        if (ip.Contains(":") || ip.Any(char.IsLetter))
        {
            Meu_ip_Area.SetTextWithoutNotify("--");
            Avisos.instance.Mostrar_Aviso("O ip colocado não é válido");
            return;
        }

        Local_Client.meu_IP = ip;
        Opcoes.SalvarConfiguracoes();
    }
    public void Copiar_Meu_IP()
    {
        GUIUtility.systemCopyBuffer = Meu_ip_Area.text;
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
    public void Cenctar()
    {
        //Tenta se conectar com o Host, caso dê tudo certo, abra a aba de SALA
        Local_Client.instance.ConnectToServer();
    }
    public TMP_InputField ip_Area;
    public void Definir_IP(string ip)
    {
        if (Local_Client.instance != null)
        {
            ip_Area.SetTextWithoutNotify(ip);
            string[] ip_port = ip.Split(new char[] { ':' });
            Local_Client.instance.ip = ip_port[0];
            if(ip_port.Length > 1)
            {
                if (int.TryParse(ip_port[1], out int port))
                    Local_Client.instance.port = port;
                else
                    //Precisa ativar o aviso de port invalido
                    throw new KeyNotFoundException();
            }
        }
    }
    public void AbrirOpções()
    {
        Menu_Inicial.SetActive(false);
        Opções.SetActive(true);
    }
    public void FecharOpções()
    {
        if(Local_Client.Conectado)
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
