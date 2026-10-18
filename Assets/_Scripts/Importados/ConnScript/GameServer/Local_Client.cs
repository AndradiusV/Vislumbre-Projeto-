using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        public void ConnectToServer()
        {
            webRTC.CriarPeer();
            Conn_Interactions.InitializeServerData();
            tcp.Connect();
        }
        public void Disconnect()
        {
            tcp.Disconect();
            udp.Disconect();
            Conectado = false;
        }
        #region Status
        bool tela_Interia;
        public bool transmitindo;
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
        public void Envio_Mouse(Mouse_Type mouse_Type,BrushType tipo,Permissão_De_Desenho Tipo_Desenho, Vector2 pos, Color cor, float size)
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

                }
                catch (Exception e)
                {
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
                        instance.Disconnect();
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
                    instance.Disconnect();
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

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

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
                        instance.Disconnect();
                        return;
                    }

                    HandleData(_data);
                }
                catch (Exception e)
                {
                    // TODO: disconnect
                    Debug.LogError(e);
                    instance.Disconnect();
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
            internal bool conectado => candidates && offer_Recebida && _conectado;
            bool candidates;
            bool offer_Recebida;
            bool _conectado;
            internal bool conec_Tela;
            internal void CriarPeer()
            {
                RTCConfiguration config =
                    new RTCConfiguration();

                peer = new RTCPeerConnection(ref config);

                peer.OnIceCandidate = candidate =>
                {
                    if (candidate == null)
                        return;
                    ThreadManager.ExecuteOnMainThread(
                        () => Avisos.instance.Mostrar_Aviso("Canditado achado"));

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
                    if (state == RTCIceGatheringState.Complete){
                        candidates = true;
                        ThreadManager.ExecuteOnMainThread(() => Sala_UI.instance.Trans_BT_Control());
                    }
                    ThreadManager.ExecuteOnMainThread(
                        () => Avisos.instance.Mostrar_Aviso("ice status change:" + state));
                };

                peer.OnConnectionStateChange = state =>
                {
                    ThreadManager.ExecuteOnMainThread(
                        () => Avisos.instance.Mostrar_Aviso("Status tá mudando: " + state));
                    if (state == RTCPeerConnectionState.Connected)
                    {
                        _conectado = true;
                        ThreadManager.ExecuteOnMainThread(() => Sala_UI.instance.Trans_BT_Control());
                        ThreadManager.ExecuteOnMainThread(
                            () => Avisos.instance.Mostrar_Aviso("Conexão com o WEBRTC estabelecida com sucesso!"));
                    }
                    else if (state == RTCPeerConnectionState.Disconnected || state == RTCPeerConnectionState.Failed)
                    {
                        ThreadManager.ExecuteOnMainThread(
                            () => Avisos.instance.Mostrar_Aviso("Conexão com o WEBRTC desfeita!"));
                        candidates = false;
                        offer_Recebida = false;
                        _conectado = false;
                        ThreadManager.ExecuteOnMainThread(() => Sala_UI.instance.Trans_BT_Control());
                    }
                };

                peer.OnTrack = e =>
                {
                    Debug.Log("TRACK RECEBIDA!");

                    if (e.Track is VideoStreamTrack videoTrack)
                    {
                        Debug.Log("VIDEO TRACK RECEBIDA!");

                        videoTrack.OnVideoReceived += texture =>
                        {
                            Debug.Log($"FRAME RECEBIDO: {texture.width}x{texture.height}");

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
                        ThreadManager.ExecuteOnMainThread(
                            () => Avisos.instance.Mostrar_Aviso("DATA CHANNEL ABERTO!"));
                    };

                    dataChannel.OnMessage = data =>
                    {
                        ThreadManager.ExecuteOnMainThread(
                            () => Avisos.instance.Mostrar_Aviso("Recebi: " +
                            Encoding.UTF8.GetString(data)));
                    };
                };
            }

            internal IEnumerator CriarOffer()
            {
                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("Criei offer"));
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
                if(op.IsError)
                {
                    Debug.LogError(op.Error);
                    yield break;
                }

                conec_Tela = true;
                ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Recebi uma resposta!"));
            }
            IEnumerator ReceberOffer(string sdp)
            {
                RTCSessionDescription offer =
                    new RTCSessionDescription
                    {
                        type = RTCSdpType.Offer,
                        sdp = sdp
                    };

                var remote = peer.SetRemoteDescription(
                    ref offer
                );

                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("Iniciei o receber offer"));
                yield return remote;

                if (remote.IsError)
                {
                    Debug.LogError(remote.Error);
                    yield break;
                }

                var op = peer.CreateAnswer();


                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("Fiz a resposta"));
                yield return op;

                if (op.IsError)
                {
                    Debug.LogError(op.Error);
                    yield break;
                }

                var answer = op.Desc;

                var local = peer.SetLocalDescription(ref answer);

                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("fiz o local"));
                yield return local;

                offer_Recebida = true;

                ThreadManager.ExecuteOnMainThread(() => Sala_UI.instance.Trans_BT_Control());

                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("Antes de enviar a resposta"));
                Enviar(new SignalMessage
                {
                    type = "answer",
                    sdp = answer.sdp
                });

                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("Enviei a resposta"));
            }

            void AdicionarIce(SignalMessage msg)
            {
                ThreadManager.ExecuteOnMainThread(
                    () => Avisos.instance.Mostrar_Aviso("Adicionei o ICE"));
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
                ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso(msg.type));
                ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Enviei mensagem"));
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
                Avisos.instance.Mostrar_Aviso(json);
                SignalMessage msg = JsonUtility.FromJson<SignalMessage>(json);

                if (msg.type == "offer")
                {
                    Avisos.instance.Mostrar_Aviso("Foi uma offer");
                    instance.StartCoroutine(ReceberOffer(msg.sdp));
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
                //// Add the track.
                //peerConnection.AddTrack(track);

                //// Create the SDP.
                //RTCAnswerOptions options = default;
                //var op = pc.CreateAnswer(ref options);
                //yield return op;


                videoTrack = new VideoStreamTrack(WebRTC_Video_S_R.textura);
                if(sender != null)
                {
                    peer.RemoveTrack(sender);
                    sender.Dispose();
                }
                sender = peer.AddTrack(videoTrack);

                var parameters = sender.GetParameters();
                foreach (var encoding in parameters.encodings)
                {
                    encoding.maxBitrate = WebRTC_Video_S_R.BitRate;
                    encoding.maxFramerate = WebRTC_Video_S_R.FPS_Target;
                    encoding.scaleResolutionDownBy = WebRTC_Video_S_R.scaleResolution;
                }
                sender.SetParameters(parameters);

                ThreadManager.ExecuteOnMainThread(() => instance.StartCoroutine(CriarOffer()));
            }
            #endregion
        }
    }
}
