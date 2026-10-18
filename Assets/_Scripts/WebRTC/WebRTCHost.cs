using GameServer;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.WebRTC;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class WebRTCHost : MonoBehaviour
{
    public int signalingPort = 5000;
    /// <summary>
    /// Isso se refere a uma porta no seu moldem ou máquina, alguma merda assim. o q importa é que
    /// precisamos escolher uma que facilite e seja "privada" ent por padrão escolheremos "50000"
    /// </summary>
    public static int Port { get; private set; }

    private TcpListener listener;
    private TcpClient client;
    private static UdpClient udpListener;
    private NetworkStream stream;

    private RTCPeerConnection peer;
    private RTCDataChannel dataChannel;
    public RawImage debugImage;
    private RenderTexture videoTexture;
    private VideoStreamTrack videoTrack;
    //Dictionary<string, RTCPeerConnection> peers;
    //Dictionary<string, RTCDataChannel> channels;
    public bool ativo;
    [SerializeField] TextMeshProUGUI st_txt;
    private readonly Queue messages = new Queue();

    void Start()
    {
        if (ativo)
            StartCoroutine(WebRTC.Update()); var codecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;

        foreach (var codec in codecs)
        {
            if (codec.mimeType == "video/H264")
            {
                Debug.Log(
                    $"H264 | " +
                    $"profile-level-id={codec.sdpFmtpLine} | " +
                    $"clockRate={codec.clockRate} | " +
                    $"channels={codec.channels}"
                );
            }
        }
    }

    public void IniciarHost()
    {
        listener = new TcpListener(IPAddress.Any,signalingPort);

        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        st_txt.text += "\n Aguardando jogador...";

        listener.BeginAcceptTcpClient(ClienteConectado,null);
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
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
            // TODO: disconnect
        }
    }
    void EnviarMensagem(string mensagem)
    {
        if (dataChannel != null)
            dataChannel.Send(Encoding.UTF8.GetBytes(mensagem));
    }

    void ClienteConectado(IAsyncResult result)
    {
        try
        {
            ThreadManager.ExecuteOnMainThread(()=> CriarPeer());
            client = listener.EndAcceptTcpClient(result);
            stream = client.GetStream();

            ThreadManager.ExecuteOnMainThread(() => st_txt.text += "\n Jogador conectado!");


            ThreadManager.ExecuteOnMainThread(() => StartCoroutine(CriarOffer()));
            ComecarLeitura();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    void CriarVideo()
    {
        Texture origem = Captura_De_Tela.instance.window.texture;

        videoTexture = new RenderTexture(origem.width,origem.height,0,GraphicsFormat.B8G8R8A8_SRGB);

        videoTexture.Create();

        videoTrack = new VideoStreamTrack(videoTexture);
        RTCRtpSender sender = peer.AddTrack(videoTrack);

        var parameters = sender.GetParameters();

        //SetVideoCodec(sender);
        //Check_Actual_Encoder();

        foreach (var encoding in parameters.encodings)
        {
            encoding.maxBitrate = 2_000_000;
            encoding.maxFramerate = 30;
        }
        // Get all available video codecs.
        var codecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;

        // Filter codecs.
        var h264Codecs = codecs.Where(codec => codec.mimeType == "video/H264");
        //foreach (var item in codecs)
        //{
        //    print(item.mimeType);
        //}
        sender.SetParameters(parameters);

        //print(l.codecs.Length);
        //print("sla vamo ver:" + l.encodings[0].rid);
        debugImage.texture = videoTexture;

        //videoTrack = new VideoStreamTrack(videoTexture);
        //RTCRtpTransceiverInit init = new RTCRtpTransceiverInit()
        //{
        //    direction = RTCRtpTransceiverDirection.SendOnly,
        //    sendEncodings = new RTCRtpEncodingParameters[] {
        //    new RTCRtpEncodingParameters { maxFramerate = 30 }
        //},
        //};
        //MediaStreamTrack videoTrack = new VideoStreamTrack(videoTexture);
        //RTCRtpTransceiver transceiver = peer.AddTransceiver(videoTrack, init);

        //print(videoTrack.Kind.ToString());
    }
    private void SetVideoCodec(RTCRtpSender videoSender)
    {
        var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
        var availableCodecs = capabilities.codecs.ToArray();
        RTCRtpCodecCapability[] codecs = null;
        RTCRtpCodecCapability preferredCodec = availableCodecs.First(codec => codec.mimeType == "video/H264");

         if (preferredCodec == null)
        {
            preferredCodec = availableCodecs[0];
            Debug.Log($"Codec not found using default one {preferredCodec.mimeType}");
        }

        foreach (var availableCodec in availableCodecs)
        {
            Debug.Log($"availableCodec {availableCodec.mimeType}");
        }

        codecs = new[] { preferredCodec };
        RTCRtpTransceiver transceiver = peer.GetTransceivers().First();

        Debug.Log($"----->preferredCodec : {preferredCodec.mimeType} ");
        RTCErrorType error = transceiver.SetCodecPreferences(codecs);
        foreach (var codecSet in codecs)
        {
            Debug.Log($"----->Codec set with SetCodecPreferences {codecSet.mimeType} ");
        }

        if (error != RTCErrorType.None)
        {
            Debug.LogErrorFormat("RTCRtpTransceiver.SetCodecPrefer  ences failed. {0}", error);
        }

        var codecsInTransceiverSender = transceiver.Sender.GetParameters().codecs;
        foreach (var codecInTransceiver in codecsInTransceiverSender)
        {
            Debug.Log($"-->> Codec set in transceiver.Sender {codecInTransceiver.mimeType}");
        }


    }
    void CriarPeer()
    {
        RTCConfiguration config = new RTCConfiguration();

        peer = new RTCPeerConnection(ref config);

        //CriarVideo();
        // eu host achei um canditado MEU, e vou enviar
        peer.OnIceCandidate = candidate =>
        {
            if (candidate == null)
            {
                st_txt.text += "\n Veio um ICE VAZIO";
                return;
            }

            Enviar(new SignalMessage
            {
                type = "ice",
                candidate = candidate.Candidate,
                sdpMid = candidate.SdpMid,
                sdpMLineIndex = (int)candidate.SdpMLineIndex
            });
        };
        peer.OnIceGatheringStateChange = state =>
        {
            st_txt.text += "\n ICE: " + state;
        };
        peer.OnConnectionStateChange = state =>
        {
            st_txt.text += "\n WebRTC: " + state;
        };

        dataChannel = peer.CreateDataChannel("dados");

        dataChannel.OnOpen = () =>
        {
            st_txt.text += "\n DATA CHANNEL ABERTO!";
        };

        dataChannel.OnMessage = data =>
        {
            st_txt.text += "\n Recebi: " +
                Encoding.UTF8.GetString(data);
        };
    }

    IEnumerator CriarOffer()
    {
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

        Enviar(new SignalMessage
        {
            type = "offer",
            sdp = offer.sdp
        });
    }

    void ComecarLeitura()
    {
        byte[] buffer = new byte[65536];

        stream.BeginRead(
            buffer,
            0,
            buffer.Length,
            Ler,
            buffer
        );
    }

    void Ler(IAsyncResult result)
    {
        try
        {
            int tamanho = stream.EndRead(result);

            if (tamanho <= 0)
                return;

            byte[] buffer = (byte[])result.AsyncState;

            string json = Encoding.UTF8.GetString(
                buffer,
                0,
                tamanho
            );

            lock (messages)
                messages.Enqueue(json);

            ComecarLeitura();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    private float proximoFrame;
    private float intervalo = 1f / 30f;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            //SetVideoCodec(peer.GetSenders().FirstOrDefault());
            //IEnumerable<RTCRtpSender> sender = peer.GetSenders();

            //foreach (var item in sender)
            //{
            //    try
            //    {
            //        StartCoroutine(peido(item));
            //        StartCoroutine(leite(item));
            //    }
            //    catch
            //    {
            //        continue;
            //    }
            //}
        }
        if (Time.time > proximoFrame)
        {
            proximoFrame = Time.time + intervalo;

            if (videoTexture != null && Captura_De_Tela.instance != null && Captura_De_Tela.instance.isValid)
                Graphics.Blit(Captura_De_Tela.instance.window.texture, videoTexture);
        }
        while (true)
        {
            string json = null;

            lock (messages)
            {
                if (messages.Count == 0)
                    break;

                json = (string)messages.Dequeue();
            }

            ProcessarDados(json);
        }
    }
    private void FixedUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    videoTrack = new VideoStreamTrack(Captura_De_Tela.instance.window.texture);
        //}
    }

    string dadosRecebidos = "";

    void ProcessarDados(string dados)
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
        print(json);
        SignalMessage msg =
            JsonUtility.FromJson<SignalMessage>(json);

        if (msg.type == "answer")
        {
            StartCoroutine(ReceberAnswer(msg.sdp));
        }
        else if (msg.type == "ice")
        {
            AdicionarIce(msg);
        }
    }

    IEnumerator ReceberAnswer(string sdp)
    {
        RTCSessionDescription answer =
            new RTCSessionDescription
            {
                type = RTCSdpType.Answer,
                sdp = sdp
            };

        var op = peer.SetRemoteDescription(
            ref answer
        );

        yield return op;

        st_txt.text += "\n Answer recebida!";
    }

    void AdicionarIce(SignalMessage msg)
    {
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

    void Enviar(SignalMessage msg)
    {
        ThreadManager.ExecuteOnMainThread(() => st_txt.text += "\n Enviei mensagem");
        string json = JsonUtility.ToJson(msg) + "\n";

        byte[] data = Encoding.UTF8.GetBytes(json);

        stream.Write(data, 0, data.Length);
    }

    void OnDestroy()
    {
        dataChannel?.Close();
        videoTrack?.Stop();
        peer?.Close();
        peer?.Dispose();

        stream?.Close();
        client?.Close();
        listener?.Stop();
    }
}