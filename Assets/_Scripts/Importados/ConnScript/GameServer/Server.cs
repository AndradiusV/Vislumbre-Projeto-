using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.WebRTC;
using UnityEngine;

namespace GameServer
{
    /// <summary>
    /// Server irá apenas facilitar a conexão entre os clientes.
    /// tem fim de remover e adiministrar todos na conexão.
    /// </summary>
    class Server
    {
        public static int MaxPlayers { get; private set; }
        /// <summary>
        /// Isso se refere a uma porta no seu moldem ou máquina, alguma merda assim. o q importa é que
        /// precisamos escolher uma que facilite e seja "privada" ent por padrão escolheremos "50000"
        /// </summary>
        public static int Port { get; private set; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        internal static int Cliente_Transmitindo;
        internal static bool Cliente_Escolheu_Tela
        {
            get { return _Cliente_Escolheu_Tela; }
            set
            {
                _Cliente_Escolheu_Tela = value;
                Sala_UI.instance.Alguem_Transmitindo(value);
                if (!value && Local_Client.instance != null && Local_Client.instance.assistindo)
                {
                    Audio_Trans_Manager.instance.Parar_Reciver();
                    Sala_UI.instance.Abir_Espera();
                }
                if (Online)
                {
                    Send_Host_Packet.Atualizar_Alguem_Transmitindo(Cliente_Transmitindo, _Cliente_Escolheu_Tela);
                }
            }
        }
        static bool _Cliente_Escolheu_Tela;
        internal static Permissão_De_Desenho Client_Permi_Desenho_Type
        {
            get { return _Cliente_Permite_Desenho; }
            set
            {
                _Cliente_Permite_Desenho = value;
                if (Online)
                {
                    Send_Host_Packet.Atualizar_Desenho_Permi(-2, value);
                }
            }
        }
        static Permissão_De_Desenho _Cliente_Permite_Desenho;
        internal static bool Online;
        internal static int Desenho_width;
        internal static int Desenho_height;
        private static TcpListener tcpListener;
        private static UdpClient udpListener;
        #region Transmissão de Video
        internal static uint? FPS_Target = 15;
        internal static ulong? BitRate = 2_000_000;
        internal static double? scaleResolution = 2.0;
        internal static VideoStreamTrack videoTrack;
        internal static void Iniciar_Transmissão()
        {
            videoTrack = new VideoStreamTrack(WebRTC_Video_S_R.textura);

            foreach (var item in clients)
            {
                if(item.Value.assistindo && item.Key != Cliente_Transmitindo && !item.Value.webRTC.conec_Tela)
                {
                    item.Value.webRTC.Iniciar_Transmissão();
                }
            }
        }
        internal static void Cliente_Pediu_Para_Assistir(int client_ID)
        {
            clients[client_ID].webRTC.Iniciar_Transmissão();
        }
        internal static void Cliente_Parou_Assistir(int client_ID)
        {
            clients[client_ID].webRTC.Parar_Transmissão();
        }
        #endregion

        /// <summary>
        /// Inicia servidor
        /// </summary>
        /// <param name="_maxPlayers"></param>
        /// <param name="_port"></param>
        public static void Start(int _maxPlayers)
        {
            MaxPlayers = _maxPlayers;
            Online = true;

            Debug.Log("Starting server...");
            Iniciar_Slots_De_Clientes(MaxPlayers);
            Conn_Interactions.InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, 0);
            tcpListener.Start();
            Port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);


            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Porta escolhida: {Port}");
        }
        public static void Fechar_Servidor()
        {
            Online = false;
            Local_Client.Conectado = false;
            //Ordena todos os clientes se desconectarem ou escolherem um novo Host, ainda preciso decidir
            foreach (var cl in clients)
            {
                cl.Value.Disconect();
            }
            clients.Clear();

            tcpListener.Stop();
            udpListener.Close();
            udpListener.Dispose();
        }

        /// <summary>
        /// É chamado quando um client, do script Local_Client tentou conectar com o servidor.
        /// </summary>
        /// <param name="_result"></param>
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            try
            {
                //Pega a conexão e salva com oum TCPClient, ele tem tudo sobre essa conexão
                TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
                //Define para o TCP procurar callback de conexões
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

                ThreadManager.ExecuteOnMainThread(() =>
                { Avisos.instance.Mostrar_Aviso($"Conexão vinda de: {_client.Client.RemoteEndPoint}..."); });

                //Vê o próximo slot de client vazio, assim o conecta
                for (int i = 0; i <= MaxPlayers; i++)
                {
                    if (clients[i].tcp.socket == null)
                    {
                        clients[i].tcp.Connect(_client);
                        return;
                    }
                }

                Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Avisos.instance.Mostrar_Aviso($"Erro ao conectar: {e}");
            }
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    // TODO: disconnect
                    return;
                }

                // Pega a janela dos 4 primeiros bytes (índice 0, tamanho 4)
                ReadOnlySpan<byte> pedaco = _data.AsSpan(0, 4);

                // Converte os bytes de volta para int
                int _clientId = BinaryPrimitives.ReadInt32LittleEndian(pedaco);

                if (_clientId == -1)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_data);
                }

                //using (Packet _packet = new Packet(_data))
                //{
                //    int _clientId = _packet.ReadInt();

                //    if (_clientId == -1)
                //    {
                //        return;
                //    }

                //    if (clients[_clientId].udp.endPoint == null)
                //    {
                //        clients[_clientId].udp.Connect(_clientEndPoint);
                //        return;
                //    }

                //    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                //    {
                //        clients[_clientId].udp.HandleData(_packet);
                //    }
                //}
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving UDP data: {_ex}");
                // TODO: disconnect
            }
        }
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }
        public static void SendUDPData(IPEndPoint _clientEndPoint, byte[] _packet, AsyncCallback callback = null)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet, _packet.Length, _clientEndPoint, callback, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }
        /// <summary>
        /// Abre os slots dos clientes, cada slopt será uma cõnexão de cliente - Servidor
        /// </summary>
        internal static void Iniciar_Slots_De_Clientes(int MaxPlayers)
        {
            Cliente_Transmitindo = -1;
            _Cliente_Escolheu_Tela = false;
            _Cliente_Permite_Desenho = Permissão_De_Desenho.nenhuma;

            clients.Clear();
            for (int i = 0; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}
