using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.WebRTC;
using UnityEngine;

namespace GameServer
{
    /// <summary>
    /// Classe que referencia outros clientes,
    /// servirá para troca de informação direta entre os conectados.
    /// é como um endereço especial de cada pessoa conectada.
    /// Está classe cuida da parte de receber as informações.
    /// </summary>
    internal class Client
    {
        public static int dataBufferSize = 4096;


        public int id;
        public TCP tcp;
        public UDP udp;
        public WebRTC_Local webRTC;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id, this);
            udp = new UDP(id);
            webRTC = new WebRTC_Local(id);
        }
        internal void Disconect()
        {
            transmitindo = false;
            assistindo = false;
            tcp.Disconect();
            udp.Disconect();
            webRTC.EncerrarConexao();
        }

        #region Status
        public bool transmitindo;
        public bool assistindo;
        #endregion

        public class TCP
        {
            public TcpClient socket;
            Client instance;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id, Client instance)
            {
                id = _id;
                this.instance = instance;
            }
            public void Connect(string ip_adress, int port)
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(ip_adress, port, ReceiveCallback, socket);
            }

            /// <summary>
            /// Guarda a conexão já estabelecida
            /// </summary>
            /// <param name="_socket">socket da con</param>
            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                if (Server.Online)
                {
                    Send_Host_Packet.Welcome(id, "Welcome to the server!");
                    ThreadManager.ExecuteOnMainThread(() => Local_Client.instance.StartCoroutine(instance.webRTC.CriarOffer()));
                }
            }
            public void Disconect()
            {
                try
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket.Dispose();
                    }
                }
                catch (Exception e)
                {

                }
                try
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
                catch (Exception e)
                {

                }
                socket = null;
                stream = null;
                receiveBuffer = null;
            }

            /// <summary>
            /// Envia o pacote para o cliente com essa conexão pré estabelecida
            /// </summary>
            /// <param name="_packet"></param>
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
                    Debug.Log($"Error sending data to player {id} via TCP: {_ex}");
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
                        instance.Disconect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error receiving TCP data: {_ex}");
                    // TODO: disconnect
                    instance.Disconect();
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
                            Conn_Interactions.packetHandlers_Clients[_packetId](id, _packet);
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
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
                Send_Host_Packet.UDPTest(id);
            }
            public void Disconect()
            {
                endPoint = null;
            }

            public void SendData(Packet _packet)
            {
                if (endPoint == null)
                    return;

                Server.SendUDPData(endPoint, _packet);
            }

            public void SendData(byte[] _packet, AsyncCallback callback = null)
            {
                if (endPoint == null)
                    return;

                Server.SendUDPData(endPoint, _packet, callback);
            }

            public void HandleData(Packet _packetData)
            {
                if (endPoint == null)
                    return;

                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Conn_Interactions.packetHandlers_Clients[_packetId](id, _packet);
                    }
                });
            }

            public void HandleData(byte[] _data)
            {
                if (endPoint == null)
                    return;

                // Pega o resto da array
                ReadOnlySpan<byte> resto = _data.AsSpan(4, _data.Length - 4);
                byte[] _packetBytes = resto.ToArray();

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Conn_Interactions.packetHandlers_Clients[_packetId](id, _packet);
                    }
                });
            }
        }
        public class WebRTC_Local
        {
            internal RTCPeerConnection peer;
            private RTCDataChannel dataChannel;
            private int id;
            internal bool conectado => candidates && offer_Recebida && _conectado;
            bool candidates;
            bool offer_Recebida;
            bool _conectado;
            internal bool conec_Tela;

            public WebRTC_Local(int _id)
            {
                id = _id;
                CriarPeer();
            }

            void CriarPeer()
            {
                RTCConfiguration config = new RTCConfiguration();

                peer = new RTCPeerConnection(ref config);

                peer.OnIceCandidate = candidate =>
                {
                    if (candidate == null)
                        return;

                    ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Canditado achado"));
                    Enviar(new SignalMessage{
                        type = "ice",
                        candidate = candidate.Candidate,
                        sdpMid = candidate.SdpMid,
                        sdpMLineIndex = candidate.SdpMLineIndex
                    });
                };


                peer.OnIceGatheringStateChange = state =>
                {
                    if (state == RTCIceGatheringState.Complete)
                        candidates = true;
                    ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("ice status change:" + state));
                };

                peer.OnConnectionStateChange = state =>
                {
                    ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Status tá mudando: " + state));
                    if (state == RTCPeerConnectionState.Connected)
                    {
                        _conectado = true;
                        ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Conexão com o WEBRTC estabelecida com sucesso!"));
                    }
                    else if (state == RTCPeerConnectionState.Disconnected || state == RTCPeerConnectionState.Failed)
                    {
                        ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Conexão com o WEBRTC desfeita!"));
                        candidates = false;
                        offer_Recebida = false;
                        _conectado = false;
                        EncerrarConexao();
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

                            conec_Tela = true;
                            //rawImage.texture = texture;
                            WebRTC_Video_S_R.instance.Recive(texture);
                        };
                    }
                };

                dataChannel = peer.CreateDataChannel("dados");

                dataChannel.OnOpen = () =>
                {
                    ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("DATA CHANNEL ABERTO!"));
                };

                dataChannel.OnMessage = data =>
                {
                    ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Recebi: " + Encoding.UTF8.GetString(data)));
                };
            }
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

                if(sender != null)
                {
                    sender.Dispose();
                    sender = null;
                }

                ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("Conexão encerrada."));
            }
            internal IEnumerator CriarOffer()
            {
                conec_Tela = false;
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
                if (op.IsError)
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
                ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("enviei para o cliente uma mensagem pelo webrtc"));
                string json = JsonUtility.ToJson(msg) + "\n";

                byte[] data = Encoding.UTF8.GetBytes(json);

                using (Packet packet = new Packet((int)ServerPackets.webRTC_Message))
                {
                    packet.Write(data);
                    packet.WriteLength();
                    Server.clients[id].tcp.SendData(packet);
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
                    Local_Client.instance.StartCoroutine(ReceberOffer(msg.sdp));
                }
                else if (msg.type == "answer")
                {
                    Local_Client.instance.StartCoroutine(ReceberAnswer(msg.sdp));
                }
                else if (msg.type == "ice")
                {
                    AdicionarIce(msg);
                }
            }
            #endregion
            #region Transmissão
            internal RTCRtpSender sender;
            internal void Iniciar_Transmissão()
            {
                if (sender != null)
                {
                    //peer.RemoveTrack(sender);
                    sender.ReplaceTrack(Server.videoTrack);
                    return;
                }
                sender = peer.AddTrack(Server.videoTrack);

                var parameters = sender.GetParameters();
                if(Local_Client.instance.transmitindo)
                {
                    foreach (var encoding in parameters.encodings)
                    {
                        encoding.maxBitrate = WebRTC_Video_S_R.BitRate;
                        encoding.maxFramerate = WebRTC_Video_S_R.FPS_Target;
                        encoding.scaleResolutionDownBy = WebRTC_Video_S_R.scaleResolution;
                    }
                }
                sender.SetParameters(parameters);

                ThreadManager.ExecuteOnMainThread(() => Local_Client.instance.StartCoroutine(CriarOffer()));
            }

            internal void Parar_Transmissão()
            {
                sender.ReplaceTrack(null);
            }
            #endregion
        }
    }
}
