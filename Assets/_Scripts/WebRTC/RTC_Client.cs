using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class RTC_Client : MonoBehaviour
{
    public string hostIP = "26.0.0.1";
    public int port = 5000;

    private TcpClient client;
    private NetworkStream stream;
    [SerializeField] TextMeshProUGUI st_txt;
    public RawImage rawImage;

    private RenderTexture videoTexture;

    private RTCPeerConnection peer;
    private RTCDataChannel dataChannel;
    public bool ativo;
    private readonly Queue messages = new Queue();

    void Start()
    {
        if(ativo)
            StartCoroutine(WebRTC.Update());
    }

    public void Conectar()
    {
        CriarPeer();

        client = new TcpClient();

        client.BeginConnect(hostIP, port, Conectado, null);
    }

    void Conectado(IAsyncResult result)
    {
        try
        {
            client.EndConnect(result);

            stream = client.GetStream();

            ThreadManager.ExecuteOnMainThread(() => st_txt.text += "\n Conectado ao Host!");

            ComecarLeitura();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    void CriarPeer()
    {
        RTCConfiguration config =
            new RTCConfiguration();

        peer = new RTCPeerConnection(ref config);
        
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
                sdpMLineIndex = candidate.SdpMLineIndex
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

        peer.OnTrack = e =>
        {
            Debug.Log("TRACK RECEBIDA!");

            if (e.Track is VideoStreamTrack videoTrack)
            {
                Debug.Log("VIDEO TRACK RECEBIDA!");

                videoTrack.OnVideoReceived += texture =>
                {
                    Debug.Log($"FRAME RECEBIDO: {texture.width}x{texture.height}");

                    rawImage.texture = texture;
                };
            }
        };
        peer.OnDataChannel = channel =>
        {
            dataChannel = channel;

            dataChannel.OnOpen = () =>
            {
                st_txt.text += "\n DATA CHANNEL ABERTO!";
            };

            dataChannel.OnMessage = data =>
            {
                st_txt.text += "\n Recebi: " +
                    Encoding.UTF8.GetString(data);
            };
        };
    }

    void ReceberVideo(VideoStreamTrack videoTrack)
    {
        videoTexture = new RenderTexture(1920, 1080, 0);
        videoTexture.Create();

        videoTrack.OnVideoReceived += texture =>
        {
            Graphics.Blit(texture, videoTexture);
        };

        rawImage.texture = videoTexture;
    }
    void ComecarLeitura()
    {
        byte[] buffer = new byte[65536];

        stream.BeginRead(buffer,0,buffer.Length,Ler,buffer);
    }

    void Ler(IAsyncResult result)
    {
        try
        {
            int tamanho = stream.EndRead(result);

            if (tamanho <= 0)
                return;

            byte[] buffer = (byte[])result.AsyncState;

            string json = Encoding.UTF8.GetString(buffer,0,tamanho);

            lock (messages)
                messages.Enqueue(json);

            ComecarLeitura();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    void Update()
    {
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
        SignalMessage msg =
            JsonUtility.FromJson<SignalMessage>(json);

        if (msg.type == "offer")
        {
            StartCoroutine(ReceberOffer(msg.sdp));
        }
        else if (msg.type == "ice")
        {
            AdicionarIce(msg);
        }
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

        yield return remote;

        if (remote.IsError)
        {
            Debug.LogError(remote.Error);
            yield break;
        }

        var op = peer.CreateAnswer();

        yield return op;

        if (op.IsError)
        {
            Debug.LogError(op.Error);
            yield break;
        }

        var answer = op.Desc;

        var local = peer.SetLocalDescription(
            ref answer
        );

        yield return local;

        Enviar(new SignalMessage
        {
            type = "answer",
            sdp = answer.sdp
        });
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
        peer?.Close();
        peer?.Dispose();

        stream?.Close();
        client?.Close();
    }
}
