using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sala_UI : MonoBehaviour
{
    [SerializeField] Compartilhando_UI comp_OB;
    internal static Sala_UI instance;
    private void Awake()
    {
        instance = this;
        Compartilhando_UI.instance = comp_OB;
    }
    [SerializeField] TextMeshProUGUI ip_Port_TXT;
    public void Definir_Info_Sala()
    {
        if (Server.Online)
            ip_Port_TXT.text = Menu_Inicial_UI.instance.Meu_ip_Area.text + ":" + Server.Port;
        else
            ip_Port_TXT.text = Local_Client.instance.ip + ":" + Local_Client.instance.port;
    }
    [SerializeField] TextMeshProUGUI _quants_Jogadores_TXT;
    public void Atualizar_Integrantes_Quant(int quant)
    {
        _quants_Jogadores_TXT.text = quant.ToString() + "Online";
    }
    public void Copiar_Sala_IP()
    {
        GUIUtility.systemCopyBuffer = ip_Port_TXT.text;
    }
    [SerializeField] GameObject Transmite_Aba;
    [SerializeField] GameObject Escolha_De_Janela_Aba;
    [SerializeField] GameObject Ver_Aba;
    [SerializeField] GameObject Espera_Aba;

    #region Transmissão
    /// <summary>
    /// Recebe do servidor quando o servidor permite que ele transmita a tela
    /// </summary>
    public void Abrir_Transmissão()
    {
        Espera_Aba.SetActive(false);
        Escolha_De_Janela_Aba.SetActive(false);
        Ver_Aba.SetActive(false);
        Transmite_Aba.SetActive(true);
#if !UNITY_EDITOR_WIN
        TransparencyWindowController.instance.EnableTransparentFullscreen();
#endif
    }
    public void Compartilhar_Tela_Inteira()
    {
        Local_Client.instance.Escolher_Tela_Inteira();
    }
    public void Abrir_Escolha_Janela()
    {
        Escolha_De_Janela_Aba.SetActive(true);
        EscolheJanela.instance.Alterar_Estado(true);
        EscolheJanela.instance.Listar_Janelas(null);
    }
    public void Cancelar_Compartilhamento()
    {
        EscolheJanela.instance.Alterar_Estado(false);
        Escolha_De_Janela_Aba.SetActive(false);
        Local_Client.instance.Pedir_Para_Transmitir_Tela(false);
    }
    public Button trans_bt;
    public void Trans_BT_Control()
    {
        trans_bt.interactable = Server.Cliente_Transmitindo == -2 && (Local_Client.instance.webRTC.conectado || Server.Online);
    }
    #endregion
    #region Visão
    public Button ver_bt;
    public void Alguem_Transmitindo(bool pode_ver)
    {
        trans_bt.interactable = !pode_ver && (Local_Client.instance.webRTC.conectado || Server.Online);
        ver_bt.interactable = pode_ver;
    }
    public void Tela_Escolhida_Pelo_Transmissor(bool pode_ver)
    {
        trans_bt.interactable = !pode_ver && (Local_Client.instance.webRTC.conectado || Server.Online);
        ver_bt.interactable = pode_ver;
    }
    /// <summary>
    /// Recebe do servidor quando o servidor permite que ele veja a tela de outro cliente
    /// </summary>
    public void Abrir_Visão()
    {
        Espera_Aba.SetActive(false);
        Ver_Aba.SetActive(true);
        Transmite_Aba.SetActive(false);
        TransparencyWindowController.instance.RestoreWindow();
    }
    #endregion
    public void Abir_Espera()
    {
        Espera_Aba.SetActive(true);
        Ver_Aba.SetActive(false);
        Escolha_De_Janela_Aba.SetActive(false);
        Transmite_Aba.SetActive(false);

        if (Local_Client.instance.transmitindo)
            Local_Client.instance.Pedir_Para_Transmitir_Tela(false);
        if (Local_Client.instance.assistindo)
            Local_Client.instance.pedir_para_Ver_Tela(false);

        TransparencyWindowController.instance.RestoreWindow();

        //Deve avisar que parou de ver a tela ou de transmitir.
        WebRTC_Video_S_R.instance.Parar();

        Audio_Trans_Manager.instance.Parar_Capture();
        Audio_Trans_Manager.instance.Parar_Reciver();
    }
    public void Desconectar()
    {
        //Desconecta da sala
        if (Local_Client.instance.transmitindo)
            Local_Client.instance.Pedir_Para_Transmitir_Tela(false);
        if (Local_Client.instance.assistindo)
            Local_Client.instance.pedir_para_Ver_Tela(false);

        if (Server.Online)
        {
            Server.Fechar_Servidor();
        }
        else
            Local_Client.instance.Disconnect();

        WebRTC_Video_S_R.instance.Parar();

        Menu_Inicial_UI.instance.abrir_Menu_Inicial();
    }
}
