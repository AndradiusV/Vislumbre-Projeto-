using GameServer;
using System.Security.Claims;
using UnityEngine;

internal static class Recived_From_Clients
{
    #region Basicos
    internal static void ReceberSaudação(int _fromClient, Packet p)
    {
        Avisos.instance.Mostrar_Aviso("Recebi saudação!");
        //Envia todos os conectados do servidor
        int quant = 1;
        foreach (var item in Server.clients)
        {
            if (Server.clients[item.Key].tcp.socket != null &&
                Server.clients[item.Key].tcp.socket.Connected)
            {
                quant++;
            }
        }
        Sala_UI.instance.Atualizar_Integrantes_Quant(quant);
    }
    /// <summary>
    /// Servidor Recebe isso.
    /// </summary>
    /// <param name="_fromClient"></param>
    /// <param name="p"></param>
    internal static void Client_Pingou(int _fromClient, Packet p)
    {
        int ping_id = p.ReadInt();
        Send_Host_Packet.Pingar(_fromClient, ping_id);
        SoundManager.instance.Pingar(ping_id);
    }
    public static void UDPTestReceived(int _fromClient, Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {_msg}");
    }
    public static void WebRTC_Message(int _fromClient, Packet _packet)
    {
        Avisos.instance.Mostrar_Aviso("Recebi uma mensagem WEBRTC do player");
        byte[] _msg_Bytes = _packet.ReadBytes(_packet.UnreadLength());
        string _msg = System.Text.Encoding.UTF8.GetString(_msg_Bytes);
        Server.clients[_fromClient].webRTC.ProcessarDados(_msg);
    }
    #endregion
    #region Pedidos
    /// <summary>
    /// O servidor que recebe essa mensagem. 
    /// </summary>
    /// <param name="_fromClient"></param>
    /// <param name="p"></param>
    internal static void Pedido_Para_Transmitir_Tela(int _fromClient, Packet p)
    {
        bool intenção = p.ReadBool();

        bool estava_Transmitindo = Server.clients[_fromClient].transmitindo;

        bool pode_Transmitir = intenção && !Local_Client.instance.transmitindo;
        foreach (var item in Server.clients)
        {
            if (item.Value.transmitindo)
            {
                pode_Transmitir = false;
                break;
            }
        }

        Server.clients[_fromClient].transmitindo = pode_Transmitir;

        if (Server.clients[_fromClient].transmitindo)
        {
            int width = p.ReadInt();
            int height = p.ReadInt();
            int permi_desenho = p.ReadInt();
            Server.Desenho_width = width;
            Server.Desenho_height = height;

            Server.Cliente_Escolheu_Tela = false;
            Server.Cliente_Transmitindo = _fromClient;
            Server.Client_Permi_Desenho_Type = (Permissão_De_Desenho)permi_desenho;
        }
        else if (estava_Transmitindo && !Server.clients[_fromClient].transmitindo)
        {
            //Avisa para todos os que estão assistindo que eles não podem mais assistir
            Server.Cliente_Transmitindo = -2;
            Server.Cliente_Escolheu_Tela = false;
        }
        Send_Host_Packet.Responde_Permissão_De_Transmissão(_fromClient, pode_Transmitir);
    }
    internal static void Pedido_Para_Ver_Tela(int _fromClient, Packet p)
    {
        //Pega a intenção do  cliente, se ele quer ver ou quer parar de ver a stream
        bool intenção = p.ReadBool();

        //Se alguém estiver transmitindo ele poderá. No caso a tela de transmissão já deve ter sido escolhida.
        bool pode_Assistir = Server.Cliente_Escolheu_Tela;
        pode_Assistir &= intenção;

        Server.clients[_fromClient].assistindo = pode_Assistir;

        Avisos.instance.Mostrar_Aviso($"O cliente de id {_fromClient} pediu para assistir, ele {(pode_Assistir? "pode":"não pode")} assistir");
        if (pode_Assistir)
        {
            //Caso ele possa assistir, e vá. Ativa a transmissão/retransmissão para ele além de enviar a informação do mouse para quem está transmitindo
            if (Local_Client.instance.transmitindo)
            {
                Mouses_Manager.instance.Adicionar_Mouse(_fromClient);
            }
            else
            {
                Send_Host_Packet.atualizar_Novo_Cliente_Assistindo(_fromClient, true);
            }
            Server.Cliente_Pediu_Para_Assistir(_fromClient);
        }
        else
        {
            if (Local_Client.instance.transmitindo)
            {
                Mouses_Manager.instance.Remover_Mouse(_fromClient);
            }
            else
            {
                Send_Host_Packet.atualizar_Novo_Cliente_Assistindo(_fromClient, false);
            }
            Server.Cliente_Parou_Assistir(_fromClient);
        }

        Send_Host_Packet.Responde_Permissão_De_Assistir(_fromClient, pode_Assistir);
    }
    #endregion
    #region Atualização
    public static void Atualização_Tela_Escolhida(int _fromClient, Packet p)
    {
        if (Server.clients[_fromClient].transmitindo)
        {
            Server.Cliente_Escolheu_Tela = p.ReadBool();

            Send_Host_Packet.Atualizar_Tela_Escolhida(_fromClient);

            //Atialização do host
            Sala_UI.instance.Alguem_Transmitindo(Server.Cliente_Escolheu_Tela);
        }
    }
    public static void Atualização_Permi_Desenho(int _fromClient, Packet p)
    {
        if (Server.clients[_fromClient].transmitindo)
        {
            int permi = p.ReadInt();

            Server.Client_Permi_Desenho_Type = (Permissão_De_Desenho)permi;
        }
        else
        {
            Avisos.instance.Mostrar_Aviso("<color=red>Algum jogador está definindo se é permitido desenhar na tela dos outros. porem ele não está transmitindo.</color>");
        }
    }
    #endregion
    #region Envios
    /// <summary>
    /// Mouse de algum cliente.
    /// </summary>
    /// <param name="_fromClient"></param>
    /// <param name="p"></param>
    internal static void Mouse(int _fromClient, Packet p)
    {
        if (Server.Cliente_Transmitindo == -2)
            return;

        BrushType tipo = (BrushType)p.ReadInt();
        Permissão_De_Desenho tipo_Desenho = (Permissão_De_Desenho)p.ReadInt();
        Mouse_Type mouse_Type= (Mouse_Type)p.ReadInt();
        Vector2 pos = p.ReadVector2();
        Color cor = p.ReadColor();
        float size = p.ReadFloat();

        if (Local_Client.instance.transmitindo)
        {
            Desenho_Na_Tela.instance.Receber_Mouse(_fromClient, mouse_Type, tipo, tipo_Desenho, pos, cor, size);
        }
        else
        {
            Send_Host_Packet.Enviar_Mouse(_fromClient, mouse_Type, tipo, tipo_Desenho, pos, cor, size);
        }
    }
    /// <summary>
    /// Som de algum cliente.
    /// </summary>
    /// <param name="_fromClient"></param>
    /// <param name="p"></param>
    public static void Som(int _fromClient, Packet p)
    {
        byte[] fragmento = p.ReadBytes(p.UnreadLength());

        Audio_Trans_Manager.instance.Processar_Som(fragmento);

        Send_Host_Packet.Enviar_Som(_fromClient, fragmento);
    }
    #endregion
}
