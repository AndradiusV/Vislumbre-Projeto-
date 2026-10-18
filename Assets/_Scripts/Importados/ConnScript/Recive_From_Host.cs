using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace GameServer
{
    public static class Recive_From_Host
    {
        #region Recebidos do servidor
        #region Basicos
        public static void WelcomeReceived(int _From_Client, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            Local_Client.instance.myId = _clientIdCheck;
            string _mensagem = _packet.ReadString();

            //Coleta os outros conectados
            int quant_Server = _packet.ReadInt();
            bool tem_alguem_transmitindo = _packet.ReadBool();
            Sala_UI.instance.Alguem_Transmitindo(tem_alguem_transmitindo);
            Avisos.instance.Mostrar_Aviso(_mensagem, 10f);

            Sala_UI.instance.Atualizar_Integrantes_Quant(quant_Server);

            //Abre para ouvir os outros na própria porta
            Send_Client_Packet.Client_Saudação_Para_Servidor();

            Local_Client.instance.udp.Connect(((IPEndPoint)Local_Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        }
        /// <summary>
        /// Recebe o ping vindo do server feito por alguém. seja lá quem for
        /// </summary>
        public static void Ping(int _From_Client, Packet _packet)
        {
            int _Clind_ping_ID = _packet.ReadInt();
            int _ping_ID = _packet.ReadInt();

            SoundManager.instance.Pingar(_ping_ID);

            Avisos.instance.Mostrar_Aviso("Alguém pingou no servidor");
        }
        public  static void Kick(int _From_Client, Packet _packet)
        {
            Local_Client.instance.Disconnect();
        }
        public static void UDPTest(int _From_Client, Packet _packet)
        {
            string _msg = _packet.ReadString();

            Debug.Log($"Received packet via UDP. Contains message: {_msg}");
            Send_Client_Packet.UDPTestReceived();
        }
        public static void WebRTC_Message(int _fromClient, Packet _packet)
        {
            Avisos.instance.Mostrar_Aviso("Recebi uma mensagem WebRTC");
            byte[] _msg_Bytes = _packet.ReadBytes(_packet.UnreadLength());
            string _msg = System.Text.Encoding.UTF8.GetString(_msg_Bytes);
            Local_Client.instance.webRTC.ProcessarDados(_msg);
        }
        #endregion
        #region Retornos
        /// <summary>
        /// Recebe e processa a informação do host sobre a permissão de transmissão de tela
        /// </summary>
        /// <param name="_toClient"></param>
        public static void Retorno_sobre_permissão_de_transmitir_tela(int _From_Client, Packet _packet)
        {
            bool permissão = _packet.ReadBool();
            Local_Client.instance.transmitindo = permissão;

            //TODO : FAZER ALGO
            if (permissão)
            {
                Sala_UI.instance.Abrir_Escolha_Janela();
            }
            else
            {
                Sala_UI.instance.Abir_Espera();
            }
        }
        /// <summary>
        /// Recebe e processa a informação do host sobre a permissão de transmissão de tela
        /// </summary>
        /// <param name="_toClient"></param>
        public static void Retorno_sobre_permissão_de_Assistir_tela(int _From_Client, Packet _packet)
        {
            bool permissão = _packet.ReadBool();
            Local_Client.instance.assistindo = permissão;

            //ThreadManager.ExecuteOnMainThread(() =>
            //{ Avisos.instance.Mostrar_Aviso_Fixo($"recebi minha permissão e, {permissão} foi o resultado "); });
            //TODO : FAZER ALGO
            if (permissão)
            {
                Server.Desenho_width = _packet.ReadInt();
                Server.Desenho_height = _packet.ReadInt();

                Audio_Trans_Manager.instance.Iniciar_Reciver();
                Desenho_Na_Tela.Iniciar_Assistir();
                Sala_UI.instance.Abrir_Visão();

                Server.Cliente_Escolheu_Tela = _packet.ReadBool();
                Server.Client_Permi_Desenho_Type = (Permissão_De_Desenho)_packet.ReadInt();
            }
            else
            {
                Sala_UI.instance.Abir_Espera();
            }
        }
        #endregion
        #region Atualizar
        public static void Atualizar_Alguem_Transmitindo(int _From_Client, Packet _packet)
        {
            bool tem_Alguem = _packet.ReadBool();
            Server.Cliente_Escolheu_Tela = tem_Alguem;
            if(!tem_Alguem && Local_Client.instance.assistindo)
            {
                Audio_Trans_Manager.instance.Parar_Reciver();
                Sala_UI.instance.Abir_Espera();
            }
            Sala_UI.instance.Alguem_Transmitindo(tem_Alguem);
        }
        public static void Atualizar_Tela_Escolhida(int _From_Client, Packet _packet)
        {
            Server.Cliente_Escolheu_Tela = _packet.ReadBool();
            return;
        }
        /// <summary>
        /// Manda todos os jogadores atualizarem as informações importantes
        /// </summary>
        public static void AtualizarIntegrantes(int _From_Client, Packet _packet)
        {
            int quant = _packet.ReadInt();

            //TODO : Atualiza quantos jogadores tem
            Sala_UI.instance.Atualizar_Integrantes_Quant(quant);
        }
        /// <summary>
        /// Manda todos os jogadores atualizarem as informações importantes
        /// </summary>
        public static void Atualizar_Permi_Desenho(int _From_Client, Packet _packet)
        {
            int permi = _packet.ReadInt();

            //TODO : Atualiza quantos jogadores tem
            Server.Client_Permi_Desenho_Type = (Permissão_De_Desenho)permi;
        }
        /// <summary>
        /// Recebi do host que tem alguém querendo assistir então farei o mouse dele
        /// </summary>
        public static void Atualizar_Novo_Cliente_Assistindo(int _From_Client, Packet _packet)
        {
            int cl_ID = _packet.ReadInt();
            bool Entrou = _packet.ReadBool();
            if(Entrou)
                Mouses_Manager.instance.Adicionar_Mouse(cl_ID);
            else
                Mouses_Manager.instance.Remover_Mouse(cl_ID);
        }
        #endregion
        #region Envios
        /// <summary>
        /// Recebe o Som de outro cliente
        /// </summary>
        public static void Som(int _From_Client, Packet _packet)
        {
            byte[] fragmento = _packet.ReadBytes(_packet.UnreadLength());

            Audio_Trans_Manager.instance.Processar_Som(fragmento);

        }
        /// <summary>
        /// Recebe este apenas se estiver transmitindo, seja o caso então isto é o mouse de outro cliente assistindo.
        /// </summary>
        public static void Mouse(int _From_Client, Packet _packet)
        {
            int _From_Client_On_Packet = _packet.ReadInt();

            BrushType tipo = (BrushType)_packet.ReadInt();
            Permissão_De_Desenho tipo_Desenho = (Permissão_De_Desenho)_packet.ReadInt();
            Mouse_Type mouse_Type = (Mouse_Type)_packet.ReadInt();
            Vector2 pos = _packet.ReadVector2();
            UnityEngine.Color cor = _packet.ReadColor();
            float size = _packet.ReadFloat();

            Desenho_Na_Tela.instance.Receber_Mouse(_From_Client_On_Packet, mouse_Type, tipo, tipo_Desenho, pos, cor, size);
        }
        #endregion

        #endregion
    }
}
