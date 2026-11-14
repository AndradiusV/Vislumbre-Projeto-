using GameServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class WebRTC_Video_S_R : MonoBehaviour
{
    internal static RenderTexture textura;
    internal static Texture textura_Recebida;
    internal static string encoder_Preferido;
    internal static int encoder_Preferido_ID;
    internal static List<string> encoders;
    internal static void Definir_Todos_Ecoders()
    {
        var codecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;

        //RTCRtpTransceiver.SetCodecPreferences(codecs);
        encoders.Clear();
        foreach (var item in codecs)
        {
            if(!encoders.Contains(item.mimeType))
            encoders.Add(item.mimeType);
        }
    }
    internal static void Escolher_Encoder(string encoder)
    {
        encoder_Preferido = encoder;
        for (int i = 0; i < encoders.Count; i++)
        {
            if (encoders[i] == encoder)
            {
                encoder_Preferido_ID = i;
                break;
            }
        }
        Atualizar_Transmissao();
    }
    internal static uint? FPS_Target = 30;
    internal static ulong? BitRate = 4_000_000;
    internal static double? scaleResolutionDownBy = 1.0;
    internal static void Atualizar_Transmissao()
    {
        if(instance != null)
            instance.intervalo = 1f / (int)FPS_Target;
        if (Server.Online)
        {
            foreach (var item in Server.clients)
            {
                if (item.Value.assistindo)
                {
                    var parameters = item.Value.webRTC.sender.GetParameters();

                    foreach (var encoding in parameters.encodings)
                    {
                        encoding.maxBitrate = BitRate;
                        encoding.maxFramerate = FPS_Target;
                        encoding.scaleResolutionDownBy = scaleResolutionDownBy;
                    }

                    var op = item.Value.webRTC.sender.SetParameters(parameters);

                    if (op.errorType != RTCErrorType.None)
                    {
                        Debug.LogError($"Erro ao configurar Sender: {op.message}");
                        continue;
                    }
                    Debug.Log("Sender configurado com sucesso!");
                }
            }
        }
        else if(Local_Client.instance != null && Local_Client.instance.webRTC.conec_Tela && Local_Client.instance.transmitindo)
        {
            //Atualiza o sender
            var videoSender = Local_Client.instance.webRTC.peer.GetSenders().First(s => s.Track is VideoStreamTrack);
            var parameters = videoSender.GetParameters();

            foreach (var encoding in parameters.encodings)
            {
                encoding.maxBitrate = BitRate; // 2 Mbps
                encoding.maxFramerate = FPS_Target;
                encoding.scaleResolutionDownBy = scaleResolutionDownBy;
            }

            var error = videoSender.SetParameters(parameters);

            print("atualizei");

            if (error.errorType != RTCErrorType.None)
                Debug.LogError($"Erro: {error}");
        }
    }
    public static WebRTC_Video_S_R instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }
    internal void Parar()
    {
        Parar_Compartilhamento();
        Parar_Recive();
    }
    public void Start_Transmissão()
    {
        CriarVideo();
        if (Server.Online)
        {
            Server.Cliente_Escolheu_Tela = true;
            Server.Iniciar_Transmissão();
        }
        else
        {
            Send_Client_Packet.Informar_Sore_Tela_Escolhida(true);
            Local_Client.instance.webRTC.Iniciar_Transmissão();
        }
        ThreadManager.ExecuteOnMainThread(() => Avisos.instance.Mostrar_Aviso("vou inciar a transmissão"));
    }
    public void Parar_Compartilhamento()
    {
        if (Server.Online)
        {
            if(Server.Cliente_Transmitindo == -1)
            {
                Mouses_Manager.instance.Limpar_Mouses();
                Audio_Trans_Manager.instance.Parar_Capture();
                Server.Cliente_Escolheu_Tela = false;
                foreach (var item in Server.clients)
                {
                    Server.Cliente_Parou_Assistir(item.Key);
                }
            }
        }
        else
        {
            Mouses_Manager.instance.Limpar_Mouses();
            Audio_Trans_Manager.instance.Parar_Capture();
            Local_Client.instance.webRTC.Parar_Transmissão();
        }
    }
    void CriarVideo()
    {
        intervalo = 1f / (int)FPS_Target;
        Texture origem = Captura_De_Tela.instance.window.texture;// Get a valid RendertextureFormat
        var gfxType = SystemInfo.graphicsDeviceType;
        var format = WebRTC.GetSupportedRenderTextureFormat(gfxType);

        // Cria a RenderTexture
        textura = new RenderTexture(origem.width, origem.height, 0, format);
        textura.filterMode = FilterMode.Point;

        textura.Create();
    }
    private float proximoFrame;
    private float intervalo;
    void Update()
    {
        if (textura != null && Captura_De_Tela.instance != null && Captura_De_Tela.instance.isValid)
            if (Time.time > proximoFrame)
            {
                proximoFrame = Time.time + intervalo;

                //Processo para combinar a textura da tela da janela com a textura de desenho
                if (Local_Client.instance.ID_Janela_Escolhida.texture_PID != 1234)
                {
                    if(Desenho_Na_Tela.instance._Cam_Visial_Essencial != null)
                    {
                        ieita.texture = Desenho_Na_Tela.instance._Cam_Visial_Essencial;
                        Combinar(Captura_De_Tela.instance.window.texture, Desenho_Na_Tela.instance._Cam_Visial_Essencial);

                    }
                }
                else //Apenas atualiza a textura para ser enviada
                    Graphics.Blit(Captura_De_Tela.instance.window.texture, textura);
            }
    }
    public RawImage ieita;
    RenderTexture resultado_Junção;
    public Material overlay_Material;
    void Combinar(Texture A, Texture B)
    {
        if(resultado_Junção == null)
        {
            var gfxType = SystemInfo.graphicsDeviceType;
            var format = WebRTC.GetSupportedRenderTextureFormat(gfxType);
            resultado_Junção = new RenderTexture(A.width, A.height, 0, format);
        }
        overlay_Material.SetTexture("_OverlayTex", B);

        Graphics.Blit(A, resultado_Junção, overlay_Material);

        Graphics.Blit(resultado_Junção, textura);
    }
    public RawImage img_Recebida;
    internal bool Atualizar_Recive()
    {
        if(Server.clients[Server.Cliente_Transmitindo].webRTC.textura_Do_Cliente != null)
        {
            textura_Recebida = Server.clients[Server.Cliente_Transmitindo].webRTC.textura_Do_Cliente;
            img_Recebida.texture = textura_Recebida;
            return true;
        }
        return false;
    }
    internal void Recive(Texture texture)
    {
        textura_Recebida = texture;
        img_Recebida.texture = texture;
        if (Server.Online)
            Server.Iniciar_Transmissão();
    }
    internal void Parar_Recive()
    {
        Audio_Trans_Manager.instance.Parar_Reciver();
    }
}
