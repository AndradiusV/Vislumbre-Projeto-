using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Send_Host_Packet : MonoBehaviour
{
    #region Funções Basicas
    /// <summary>
    /// Dá um "Bem vindo" ao jogador que acabou de se conectar.
    /// </summary>
    /// <param name="_toClient"></param>
    /// <param name="_msg"></param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.saudação))
        {
            _packet.Write(_toClient);
            _packet.Write(_msg);
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
            _packet.Write(quant);
            _packet.Write(Server.Cliente_Escolheu_Tela);

            SendTCPData(_toClient, _packet);
            Atualizar_Integrantes(_toClient);

        }
    }
    /// <summary>
    /// Reenvia o ping do cliente para todos os outros
    /// </summary>
    public static void Pingar(int _Sender_Client, int ping_sound)
    {
        using (Packet _packet = new Packet((int)ServerPackets.pingar))
        {
            _packet.Write(_Sender_Client);
            _packet.Write(ping_sound);

            SendTCPDataToAll(_packet);
        }
    }
    /// <summary>
    /// Reenvia para o cliente que ele será desconectado do servidor.
    /// </summary>
    public static void Kick(int _Client_alvo)
    {
        using (Packet _packet = new Packet((int)ServerPackets.kick))
        {
            SendTCPData(_Client_alvo, _packet);
        }
    }
    public static void UDPTest(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.udpTest))
        {
            _packet.Write("A test packet for UDP.");

            Debug.Log("Enviado para: " + _toClient);
            SendUDPData(_toClient, _packet);
        }
    }
    public static void Send_Signal(int _Client_alvo)
    {

    }
    #endregion
    #region Atualizações
    /// <summary>
    /// Manda todos os jogadores atualizarem as informações importantes
    /// </summary>
    public static void Atualizar_Integrantes(int _excluind_ID)
    {
        using (Packet _packet = new Packet((int)ServerPackets.atualizar_Integrantes))
        {
            //verifica quantos sockets estão conectados e outras infos que vou decidir para enviar aos jogadores
            int quant = 1;

            foreach (var item in Server.clients)
                if (item.Value.tcp.socket != null && item.Value.tcp.socket.Connected)
                    quant++;

            _packet.Write(quant);

            if (_excluind_ID != -1)
                SendTCPDataToAll(_excluind_ID, _packet);
            else
                SendTCPDataToAll(_packet);
        }
    }
    public static void Atualizar_Alguem_Transmitindo(int _Client_Excludent, bool permissão)
    {
        using (Packet _packet = new Packet((int)ServerPackets.atualizar_Alguem_Transmitindo))
        {
            _packet.Write(permissão);

            SendTCPDataToAll(_Client_Excludent, _packet);
        }
    }
    public static void Atualizar_Tela_Escolhida(int _Client_Excludent)
    {
        using (Packet _packet = new Packet((int)ServerPackets.atualizar_tamanho_da_tela))
        {
            _packet.Write(Server.Cliente_Escolheu_Tela);

            SendTCPDataToAll(_Client_Excludent, _packet);
        }
    }
    public static void Atualizar_Desenho_Permi(int _Client_Excludent, Permissão_De_Desenho permi)
    {
        using (Packet _packet = new Packet((int)ServerPackets.atualizar_Permi_Desenho))
        {
            _packet.Write((int)permi);

            SendTCPDataToAll(_Client_Excludent, _packet);
        }
    }
    public static void atualizar_Novo_Cliente_Assistindo(int _client_Sender, bool assistindo)
    {
        using (Packet _packet = new Packet((int)ServerPackets.atualizar_Permi_Desenho))
        {
            _packet.Write(_client_Sender);
            _packet.Write(assistindo);

            if(Server.Cliente_Transmitindo < 0)
            {
                Debug.LogWarning("Reveja este sessão");
                return;
            }
            SendTCPData(Server.Cliente_Transmitindo, _packet);
        }
    }
    #endregion
    #region Respostas
    /// <summary>
    /// Responde a um jogador que perguntou se ele podia termitir a tela dele.
    /// </summary>
    /// <param name="_toClient"></param>
    public static void Responde_Permissão_De_Transmissão(int _Client_Algo, bool permissão)
    {
        using (Packet _packet = new Packet((int)ServerPackets.Resposta_permissão_de_transmitir_tela))
        {
            _packet.Write(permissão);

            SendTCPData(_Client_Algo, _packet);
        }
    }
    public static void Responde_Permissão_De_Assistir(int _Client_Algo, bool permissão)
    {
        using (Packet _packet = new Packet((int)ServerPackets.Resposta_permissão_de_Assistir_tela))
        {
            _packet.Write(permissão);
            _packet.Write(Server.Desenho_width);
            _packet.Write(Server.Desenho_height);

            _packet.Write(Server.Cliente_Escolheu_Tela);
            _packet.Write((int)Server.Client_Permi_Desenho_Type);
            SendTCPData(_Client_Algo, _packet);
        }
    }
    #endregion
    #region Envio
    /// <summary>
    /// Retransmite a tela do cliente
    /// </summary>
    public static void Enviar_tela(int _Sender_Client, byte[] _packet, AsyncCallback callback)
    {
        foreach (var item in Server.clients)
        {
            if (item.Key != _Sender_Client && item.Value.udp.endPoint != null && item.Value.assistindo)
            {
                Server.clients[item.Key].udp.SendData(_packet, callback);
            }
        }
    }
    internal static void Enviar_Som(int _Sender_Client, byte[] data)
    {
        using (Packet _packet = new Packet((int)ServerPackets.enviar_Som))
        {
            _packet.Write(data);

            foreach (var item in Server.clients)
            {
                if (item.Key != _Sender_Client && item.Value.udp.endPoint != null && item.Value.assistindo)
                {
                    Server.clients[item.Key].udp.SendData(_packet);
                }
            }
        }
    }
    /// <summary>
    /// Retransmite o mouse para quem estiver transmitindo a tela
    /// </summary>
    public static void Enviar_Mouse(int _Sender_Client, Mouse_Type mouse_Type, BrushType tipo, Permissão_De_Desenho Tipo_Desenho, Vector2 pos, Color cor, float size)
    {
        using (Packet _packet = new Packet((int)ServerPackets.enviar_mouse))
        {
            _packet.Write(_Sender_Client);

            _packet.Write((int)tipo);
            _packet.Write((int)Tipo_Desenho);
            _packet.Write((int)mouse_Type);
            _packet.Write(pos);
            _packet.Write(cor);
            _packet.Write(size);

            if (Server.Cliente_Transmitindo != -1)
                SendUDPData(Server.Cliente_Transmitindo, _packet);
        }
    }
    #endregion


    #region Conn Funcs
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPData(int _toClient, Packet _packet)
    {
        //_packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        //_packet.WriteLength();
        for (int i = 0; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        //_packet.WriteLength();
        for (int i = 0; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #endregion
}
