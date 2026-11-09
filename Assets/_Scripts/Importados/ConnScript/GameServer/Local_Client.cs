using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.WebRTC;
using UnityEngine;

namespace GameServer
{
    /// <summary>
    /// Está Classe é para representar a si mesmo na conexão.
    /// Está Classe cuidade enviar as informações
    /// </summary>
    public class Local_Client : MonoBehaviour
    {
        public static Local_Client instance;
        public static int dataBufferSize = 4096;
        public static bool Conectado;

        /// <summary>
        /// IP do servidor
        /// </summary>
        public string ip = "127.0.0.1";
        /// <summary>
        /// Meu endereço para facilitar na hora de copiar e colar
        /// </summary>
        public static string meu_IP = "127.0.0.1";
        /// <summary>
        /// Port do servidor
        /// </summary>
        public int port = 0;
        /// <summary>
        /// Este é o IP:Port Que deve ser redistribuido para que outros entrem na sua sala
        /// </summary>
        public static string IP_PORT => Server.Online ? (meu_IP + ":" + Server.Port) : (instance.ip + ":" + instance.port);
        /// <summary>
        /// Seu ID no servidor
        /// </summary>
        public int myId = -1;
        /// <summary>
        /// Este TCP é exclusivo para troca de informações com o servidor
        /// </summary>
        public TCP tcp;
        /// <summary>
        /// Este UDP é exclusivo para troca de informações com o servidor
        /// </summary>
        public UDP udp;
        /// <summary>
        /// Este WebRTC é exclusivo para parte de transmissão de video
        /// </summary>
        public WebRTC_Local webRTC;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                Server.clients[0].webRTC.Coisar();
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }
        }

        private void Start()
        {
            tcp = new TCP();
            udp = new UDP();
            webRTC = new WebRTC_Local();
        }
        #region Sobre conexão
        [SerializeField] GameObject _con_aviso_GO;
        TimerToken _TK_conectando_Aviso;
        [SerializeField] TextMeshProUGUI _con_Aviso_TXT;
        public void ConnectToServer()
        {
            Aviso_Sobre_conexão();
            webRTC.CriarPeer();
            Conn_Interactions.InitializeServerData();
            tcp.Connect();
        }
        void Aviso_Sobre_conexão()
        {
            _con_aviso_GO.SetActive(true);
            //Abre a janela de "estou conectando
            if (_TK_conectando_Aviso != null)
                _TK_conectando_Aviso.Dispose();

            _TK_conectando_Aviso = Timer.AdicionarToken(30, () =>
            {

            }, t =>
            {
                int atual = (int)(t % 4);
                string pontos = ".";
                string mensagem_Final = "Conectando";
                for (int i = 0; i < atual; i++)
                {
                    mensagem_Final += pontos;
                }
                _con_Aviso_TXT.text = mensagem_Final;
            },
            () => {
                _con_aviso_GO.SetActive(false);
            });
        }
        public void Disconnect()
        {
            if (!Conectado)
                return;

            Server.Limpar_Info();
            Sala_UI.instance.Abir_Espera();
            Menu_Inicial_UI.instance.abrir_Menu_Inicial();
            Conectado = false;
            tcp.Disconect();
            udp.Disconect();
            webRTC.EncerrarConexao();
        }
        #region Status
        bool tela_Interia;
        public bool transmitindo { get { return _transmitindo; } set { _transmitindo = value; } }
        bool _transmitindo;
        internal Complet_Screen_ID ID_Janela_Escolhida;
        public bool assistindo;
        #endregion

        #region Ações do cliente local
        public void Pingar()
        {
            if (Server.Online)
            {
                int ping_id = SoundManager.instance.Pingar();

                Send_Host_Packet.Pingar(-1, ping_id);
            }
            else
                Send_Client_Packet.Pingar();
        }
        #region Transmitir
        public void Pedir_Para_Transmitir_Tela(bool intenção = true)
        {
            if (Server.Online)
            {
                bool alguémTransmitindo = false;
                foreach (var item in Server.clients)
                    if (item.Value.transmitindo)
                    {
                        alguémTransmitindo = true;
                        break;
                    }
                bool estava_Transmitindo = transmitindo;

                transmitindo = !alguémTransmitindo && intenção;
                if (transmitindo)
                {
                    Desenho_Na_Tela.Iniciar_Transmitir();
                    Server.Desenho_width = Desenho_Na_Tela.width;
                    Server.Desenho_height = Desenho_Na_Tela.height;
                    Server.Cliente_Escolheu_Tela = false;
                    Server.Cliente_Transmitindo = -1;
                    Server.Client_Permi_Desenho_Type = Desenho_Na_Tela.permi_Desenho;
                    Sala_UI.instance.Abrir_Escolha_Janela();
                }
                else if (estava_Transmitindo && !transmitindo)
                {
                    //Precisa remover todos que estavam assistindo
                    Server.Cliente_Escolheu_Tela = false;
                    Server.Cliente_Transmitindo = -2;
                }
            }
            else
                Send_Client_Packet.pedir_Para_Transmitir(intenção);
        }
        public void Escolher_Tela_Inteira()
        {
            ID_Janela_Escolhida = new Complet_Screen_ID() { texture_PID = 1234, titulo = "Tela Inteira", window_ID = -2 };
            Iniciar_Transmissão_Tela(true);
        }
        internal void Escolher_Tela(Complet_Screen_ID janela_ID)
        {
            ID_Janela_Escolhida = janela_ID;
            Iniciar_Transmissão_Tela(false);
        }
        void Iniciar_Transmissão_Tela(bool tela_inteira)
        {
            EscolheJanela.instance.Alterar_Estado(false);
            /*Após definir qual janela irá ser capturada.
             * Inicia os processos de captura de tela e som.
             * é importante lembrar que deve-se mandar uma atualização para o servidor dizendo que este
             * cliente escolher uma janela e tem um tamanho definido para os outros poderem ler as informações de transmissão.
            */
            tela_Interia = tela_inteira;
            //Mas antes, vamos preparar a UI para o Usuário
            Sala_UI.instance.Abrir_Transmissão();
            #region Audio
            Iniciar_Audio_Capture();
            #endregion

            #region Tela
            Compartilhando_UI.instance.Iniciar_Captura(ID_Janela_Escolhida);
            #endregion
        }
        public void Iniciar_Audio_Capture()
        {
            int Audio_PID = ID_Janela_Escolhida.texture_PID;
            bool include_Audio = !tela_Interia;
            //Verifica se está tentando pegar todo o audio do pc
            if (tela_Interia)
            {
                //Caso esteja, verifica se há algum que já está definido nas opções para ser ignorado
                if (Opcoes.config_Atual.prog_Audi_Excluido != null && Opcoes.config_Atual.prog_Audi_Excluido != "")
                {
                    include_Audio = false;
                    Audio_PID = EscolheJanela.instance.Get_PID_By_Program_Nam(Opcoes.config_Atual.prog_Audi_Excluido);
                }
            }

            Audio_Trans_Manager.instance.Iniciar_Capture(Audio_PID, include_Audio);
        }
        /// <summary>
        /// Aqui o Cliente irá enviar o som capturado pelas janelas selecionadas.
        /// Caso seja o Host só envia para todos, caso seja Cliente envia para o Host.
        /// </summary>
        /// <param name="data"></param>
        public void Enviar_SOM(byte[] data)
        {
            if (Server.Online)
            {
                if (Captura_De_Tela.instance != null)
                {
                    Send_Host_Packet.Enviar_Som(-1, data);
                }
            }
            else
                Send_Client_Packet.Enviar_Som(data);
        }
        #endregion
        public void pedir_para_Ver_Tela(bool intenção = true)
        {
            if (Server.Online)
            {
                if (intenção)
                {
                    bool alguémTransmitindo = false;
                    foreach (var item in Server.clients)
                    {
                        if (item.Value.transmitindo)
                        {
                            alguémTransmitindo = true;
                            break;
                        }
                    }

                    assistindo = alguémTransmitindo && intenção;
                    if (alguémTransmitindo)
                    {
                        Sala_UI.instance.Abrir_Visão();
                        Audio_Trans_Manager.instance.Iniciar_Reciver();
                        Send_Host_Packet.atualizar_Novo_Cliente_Assistindo(-1, true);
                    }
                }
                else
                {
                    assistindo = false;
                    Sala_UI.instance.Abir_Espera();
                    Send_Host_Packet.atualizar_Novo_Cliente_Assistindo(-1, false);
                }
            }
            else
                Send_Client_Packet.Pedir_Para_Ver_Tela(intenção);
        }
        public void Envio_Mouse(Mouse_Type mouse_Type, BrushType tipo, Permissão_De_Desenho Tipo_Desenho, Vector2 pos, Color cor, float size)
        {
            if (Server.Online)
            {
                Send_Host_Packet.Enviar_Mouse(-1, mouse_Type, tipo, Tipo_Desenho, pos, cor, size);
            }
            else
                Send_Client_Packet.Envio_Mouse(tipo, mouse_Type, Tipo_Desenho, pos, cor, size);
        }
        public void Atulizar_Permissão_Desenho()
        {

        }
        #endregion
        #endregion


        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                print(instance.ip);
                print(instance.port);
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }
            public void Disconect()
            {
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch { }
                socket = null;
                receiveBuffer = null;
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                try
                {
                    socket.EndConnect(_result);

                    if (!socket.Connected)
                    {
                        return;
                    }
                    stream = socket.GetStream();

                    receivedData = new Packet();

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);


                    ThreadManager.ExecuteOnMainThread(Menu_Inicial_UI.instance.abrir_Sala);
                    ThreadManager.ExecuteOnMainThread(() => {
                        if (instance._TK_conectando_Aviso != null)
                            instance._TK_conectando_Aviso.Dispose(); });
                }
                catch (Exception e)
                {
                    ThreadManager.ExecuteOnMainThread(() =>{
                        if (instance._TK_conectando_Aviso != null)
                            instance._TK_conectando_Aviso.Dispose();});
                    Debug.LogError($"Erro ao conectar: {e}");
                }
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error sending data to server via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        // TODO: disconnect
                        ThreadManager.ExecuteOnMainThread(instance.Disconnect);
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch
                {
                    // TODO: disconnect
                    ThreadManager.ExecuteOnMainThread(instance.Disconnect);
                }
            }
            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Conn_Interactions.packetHandlers_Server[_packetId](0, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public void Connect(int _localPort)
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
                socket = new UdpClient(_localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet _packet = new Packet())
                {
                    SendData(_packet);
                }
            }
            public void Disconect()
            {
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch
                {

                }
                socket = null;
                endPoint = null;
            }
            public void SendData(Packet _packet)
            {
                try
                {
                    _packet.InsertInt(instance.myId);
                    if (socket != null)
                    {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error sending data to server via UDP: {_ex}");
                }
            }
            public void SendData(byte[] _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        socket.BeginSend(_packet, _packet.Length, null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error sending data to server via UDP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    byte[] _data = socket.EndReceive(_result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (_data.Length < 4)
                    {
                        // TODO: disconnect
                        ThreadManager.ExecuteOnMainThread(instance.Disconnect);
                        return;
                    }

                    HandleData(_data);
                }
                catch (Exception e)
                {
                    // TODO: disconnect
                    Debug.LogError(e);
                    ThreadManager.ExecuteOnMainThread(instance.Disconnect);
                }
            }

            private void HandleData(byte[] _data)
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetId = _packet.ReadInt();
                        Conn_Interactions.packetHandlers_Server[_packetId](0, _packet);
                    }
                });
            }
        }

        public class WebRTC_Local
        {
            internal RTCPeerConnection peer;
            private RTCDataChannel dataChannel;
            internal bool conectado => candidates && _conectado;
            bool candidates;
            //bool offer_Recebida;
            bool _conectado;
            internal bool conec_Tela;
            internal void Coisar()
            {
                Debug.Log($"Senders: {peer.GetSenders().Count()}");
                Debug.Log($"Receivers: {peer.GetReceivers().Count()}");
                Debug.Log($"Transceivers: {peer.GetTransceivers().Count()}");

                foreach (var t in peer.GetTransceivers())
                {
                    Debug.Log(
                        $"MID: {t.Mid} | " +
                        $"Direction: {t.Direction} | " +
                        $"CurrentDirection: {t.CurrentDirection} | " +
                        $"Kind: {t.Receiver.Track?.Kind}"
                    );
                }
            }
            internal void CriarPeer()
            {
                RTCConfiguration config = new RTCConfiguration();
                peer = new RTCPeerConnection(ref config);

                // Garante transceiver bidirecional compatível com B
                var transceiver = peer.AddTransceiver(
                    TrackKind.Video,
                    new RTCRtpTransceiverInit
                    {
                        direction = RTCRtpTransceiverDirection.SendRecv
                    }
                );
                sender = transceiver.Sender;

                peer.OnIceCandidate = candidate =>
                {
                    if (candidate == null) return;
                    Enviar(new SignalMessage
                    {
                        type = "ice",
                        candidate = candidate.Candidate,
                        sdpMid = candidate.SdpMid,
                        sdpMLineIndex = candidate.SdpMLineIndex
                    });
                };

                peer.OnIceGatheringStateChange = state =>
                {
                    if (state == RTCIceGatheringState.Complete)
                    {
                        candidates = true;
                        ThreadManager.ExecuteOnMainThread(() => Sala_UI.instance.Trans_BT_Control());
                    }
                };

                peer.OnConnectionStateChange = state =>
                {
                    if (state == RTCPeerConnectionState.Connected)
                    {
                        _conectado = true;
                        ThreadManager.ExecuteOnMainThread(() => Sala_UI.instance.Trans_BT_Control());
                    }
                    else if (state == RTCPeerConnectionState.Disconnected || state == RTCPeerConnectionState.Failed)
                    {
                        candidates = false;
                        //offer_Recebida = false;
                        _conectado = false;
                    }
                };

                peer.OnTrack = e =>
                {
                    if (e.Track is VideoStreamTrack videoTrack)
                    {
                        videoTrack.OnVideoReceived += texture =>
                        {
                            WebRTC_Video_S_R.instance.Recive(texture);
                            conec_Tela = true;
                        };
                    }
                };

                peer.OnDataChannel = channel =>
                {
                    dataChannel = channel;

                    dataChannel.OnOpen = () =>
                    {
                        //ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("DATA CHANNEL ABERTO!"));
                    };

                    dataChannel.OnMessage = data =>
                    {
                        //ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Recebi: " + Encoding.UTF8.GetString(data)));
                    };
                };
            }

            internal IEnumerator CriarOffer()
            {
                if (peer == null)
                {
                    Debug.LogError("Peer não foi criado!");
                    yield break;
                }
                var op = peer.CreateOffer();

                yield return op;

                if (op.IsError)
                {
                    Debug.LogError(op.Error);
                    yield break;
                }

                var offer = op.Desc;

                var local = peer.SetLocalDescription(ref offer);

                yield return local;

                if (local.IsError)
                {
                    Debug.LogError(local.Error);
                    yield break;
                }

                Enviar(new SignalMessage { type = "offer", sdp = offer.sdp });
            }
            IEnumerator ReceberAnswer(string sdp)
            {
                RTCSessionDescription answer =
                    new RTCSessionDescription
                    {
                        type = RTCSdpType.Answer,
                        sdp = sdp
                    };

                var op = peer.SetRemoteDescription(ref answer);

                yield return op;

                //FAZ ALGO.
                if (op.IsError)
                {
                    Debug.LogError(
                        $"RTC ERROR:\n" +
                        $"Type: {op.Error.errorType}\n" +
                        $"Message: {op.Error.message}\n" +
                        $"SignalingState: {peer.SignalingState}\n" +
                        $"ConnectionState: {peer.ConnectionState}"
                    );
                    yield break;
                }

                conec_Tela = true;
                //ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Recebi uma resposta!"));
            }
            void AdicionarIce(SignalMessage msg)
            {
                //ThreadManager.ExecuteOnMainThread(
                //    () => Avisos.instance.Mostrar_Aviso("Adicionei o ICE"));
                RTCIceCandidateInit init =
                    new RTCIceCandidateInit
                    {
                        candidate = msg.candidate,
                        sdpMid = msg.sdpMid,
                        sdpMLineIndex = msg.sdpMLineIndex
                    };

                RTCIceCandidate candidate =
                    new RTCIceCandidate(init);

                peer.AddIceCandidate(candidate);
            }
            #region Envio e Recebimento de Mensagens
            void Enviar(SignalMessage msg)
            {
                //ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso(msg.type));
                //ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Enviei mensagem"));
                string json = JsonUtility.ToJson(msg) + "\n";

                byte[] data = Encoding.UTF8.GetBytes(json);

                using (Packet packet = new Packet((int)ClientPackets.webRTC_Message))
                {
                    packet.Write(data);
                    packet.WriteLength();
                    instance.tcp.SendData(packet);
                }
            }
            string dadosRecebidos = "";

            internal void ProcessarDados(string dados)
            {
                dadosRecebidos += dados;

                while (dadosRecebidos.Contains("\n"))
                {
                    int fim = dadosRecebidos.IndexOf('\n');

                    string json = dadosRecebidos.Substring(0, fim);

                    dadosRecebidos =
                        dadosRecebidos.Substring(fim + 1);

                    if (!string.IsNullOrWhiteSpace(json))
                        Processar(json);
                }
            }
            void Processar(string json)
            {
                //Avisos.instance.Mostrar_Aviso(json);
                SignalMessage msg = JsonUtility.FromJson<SignalMessage>(json);

                if (msg.type == "offer")
                {
                    Debug.LogError("Os clientes não deveriam receber offers, apenas enviar. Algo está errado.");
                    //Avisos.instance.Mostrar_Aviso("Foi uma offer");
                    //instance.StartCoroutine(ReceberOffer(msg.sdp));
                }
                else if (msg.type == "answer")
                {
                    instance.StartCoroutine(ReceberAnswer(msg.sdp));
                }
                else if (msg.type == "ice")
                {
                    Avisos.instance.Mostrar_Aviso("Foi um ice");
                    AdicionarIce(msg);
                }
            }
            #endregion
            #region Transmissão
            internal VideoStreamTrack videoTrack;
            internal RTCRtpSender sender;
            internal void Iniciar_Transmissão()
            {
                videoTrack = new VideoStreamTrack(WebRTC_Video_S_R.textura);
                if (sender != null)
                {
                    if (sender.ReplaceTrack(videoTrack))
                        print("consegui fazer meu coiso");
                    var parameters = sender.GetParameters();
                    foreach (var encoding in parameters.encodings)
                    {
                        encoding.maxBitrate = WebRTC_Video_S_R.BitRate;
                        encoding.maxFramerate = WebRTC_Video_S_R.FPS_Target;
                        encoding.scaleResolutionDownBy = WebRTC_Video_S_R.scaleResolutionDownBy;
                    }

                    RTCError erro = sender.SetParameters(parameters);

                    if (erro.errorType != RTCErrorType.None)
                    {
                        Debug.LogError($"ERRO AO CONFIGURAR SENDER: {erro.message}");
                    }
                    else
                    {
                        Debug.Log("Parâmetros do sender configurados com sucesso!");
                    }
                }
                else
                {
                    sender = peer.AddTrack(videoTrack);

                    var parameters = sender.GetParameters();
                    foreach (var encoding in parameters.encodings)
                    {
                        encoding.maxBitrate = WebRTC_Video_S_R.BitRate;
                        encoding.maxFramerate = WebRTC_Video_S_R.FPS_Target;
                        encoding.scaleResolutionDownBy = WebRTC_Video_S_R.scaleResolutionDownBy;
                    }

                    RTCError erro = sender.SetParameters(parameters);

                    if (erro.errorType != RTCErrorType.None)
                    {
                        Debug.LogError($"ERRO AO CONFIGURAR SENDER: {erro.message}");
                    }
                    else
                    {
                        Debug.Log("Parâmetros do sender configurados com sucesso!");
                    }

                    ThreadManager.ExecuteOnMainThread(() => instance.StartCoroutine(CriarOffer()));
                }
            }
            /// <summary>
            /// Para o envio de vídeo, removendo o track do sender e liberando a memória.
            /// Apenas os clientes conectados no servidor devem chamar está função
            /// </summary>
            internal void Parar_Transmissão()
            {
                if (sender == null)
                    Debug.Log("Não há sender para parar a transmissão!");
                if (videoTrack != null)
                {
                    sender.ReplaceTrack(null);
                    videoTrack.Dispose();
                    videoTrack = null;
                }
            }
            int ContarVideoTracks(RTCPeerConnection peer)
            {
                int total = 0;

                foreach (var sender in peer.GetSenders())
                {
                    if (sender.Track != null && sender.Track.Kind == TrackKind.Video)
                        total++;
                }

                return total;
            }
            internal void Mostrar_Sender_Config()
            {
                var parameters = sender.GetParameters();

                foreach (var encoding in parameters.encodings)
                {
                    Debug.Log($"MaxBitrate: {encoding.maxBitrate}");
                    Debug.Log($"MaxFramerate: {encoding.maxFramerate}");
                    Debug.Log($"ScaleResolutionDownBy: {encoding.scaleResolutionDownBy}");
                }
            }
            #endregion

            internal void EncerrarConexao()
            {
                if (peer == null)
                    return;

                peer.OnTrack = null;
                peer.OnConnectionStateChange = null;
                peer.OnIceCandidate = null;

                // Encerra a conexão e seus fluxos de mídia.
                peer.Close();
                peer.Dispose();

                peer = null;

                if (sender != null)
                {
                    sender.Dispose();
                    sender = null;
                }

                WebRTC_Video_S_R.instance.Parar();
                ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Conexão encerrada."));
            }
        }
    }
}
