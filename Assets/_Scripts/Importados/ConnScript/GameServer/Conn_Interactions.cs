using System;
using System.Collections.Generic;

namespace GameServer
{
    static class Conn_Interactions
    {
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers_Server;
        public static Dictionary<int, PacketHandler> packetHandlers_Clients;
        internal static void InitializeServerData()
        {
            Server.Cliente_Transmitindo = -2;

            packetHandlers_Clients = new Dictionary<int, PacketHandler>()
            {
                #region Basicos
                { (int)ClientPackets.saudação_para_servidor, Recived_From_Clients.ReceberSaudação },
                { (int)ClientPackets.pingar, Recived_From_Clients.Client_Pingou },
                { (int)ClientPackets.updTestReceived, Recived_From_Clients.UDPTestReceived },
                { (int)ClientPackets.webRTC_Message, Recived_From_Clients.WebRTC_Message },
#endregion
                #region Informar
                { (int)ClientPackets.informar_Tela_Escolhida, Recived_From_Clients.Atualização_Tela_Escolhida },
                { (int)ClientPackets.informar_Permi_Desenho, Recived_From_Clients.Atualização_Permi_Desenho },
                #endregion
                #region Pedidos
                { (int)ClientPackets.pedir_Para_Transmitir, Recived_From_Clients.Pedido_Para_Transmitir_Tela },
                { (int)ClientPackets.pedir_para_Ver_Tela, Recived_From_Clients.Pedido_Para_Ver_Tela },
#endregion
                #region Envios
                { (int)ClientPackets.envio_de_mouse, Recived_From_Clients.Mouse },
                { (int)ClientPackets.envio_de_Som, Recived_From_Clients.Som },
#endregion
            };
            packetHandlers_Server = new Dictionary<int, PacketHandler>()
            {
                #region Basicos
                { (int)ServerPackets.saudação, Recive_From_Host.WelcomeReceived },
                { (int)ServerPackets.pingar, Recive_From_Host.Ping },
                { (int)ServerPackets.udpTest, Recive_From_Host.UDPTest },
                { (int)ServerPackets.kick, Recive_From_Host.Kick },
                { (int)ServerPackets.webRTC_Message, Recive_From_Host.WebRTC_Message },
#endregion
                #region Atualização
                { (int)ServerPackets.atualizar_Integrantes, Recive_From_Host.AtualizarIntegrantes },
                { (int)ServerPackets.atualizar_tamanho_da_tela, Recive_From_Host.Atualizar_Tela_Escolhida },
                { (int)ServerPackets.atualizar_Alguem_Transmitindo, Recive_From_Host.Atualizar_Alguem_Transmitindo },
                { (int)ServerPackets.atualizar_Permi_Desenho, Recive_From_Host.Atualizar_Permi_Desenho },
                { (int)ServerPackets.atualizar_Novo_Cliente_Assistindo, Recive_From_Host.Atualizar_Novo_Cliente_Assistindo },
                #endregion
                #region Respostas
                { (int)ServerPackets.Resposta_permissão_de_transmitir_tela, Recive_From_Host.Retorno_sobre_permissão_de_transmitir_tela },
                { (int)ServerPackets.Resposta_permissão_de_Assistir_tela, Recive_From_Host.Retorno_sobre_permissão_de_Assistir_tela },
#endregion
                #region Envios
                { (int)ServerPackets.enviar_mouse, Recive_From_Host.Mouse },
                { (int)ServerPackets.enviar_Som, Recive_From_Host.Som },
#endregion
            };
            Console.WriteLine("Initialized packets.");
        }
    }

    class FrameBuffer
    {
        public int totalFragments;
        public byte[][] fragments;
        public int received;

        public FrameBuffer(int total)
        {
            totalFragments = total;
            fragments = new byte[total][];
        }
    }


    [Serializable]
    public class SignalMessage
    {
        public string type;
        public string sdp;

        public string candidate;
        public string sdpMid;
        public int? sdpMLineIndex;
    }
}
