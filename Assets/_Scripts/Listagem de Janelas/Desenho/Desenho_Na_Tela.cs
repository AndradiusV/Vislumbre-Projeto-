using GameServer;
using System;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using uWindowCapture;
public class Desenho_Na_Tela : MonoBehaviour
{
    public static Permissão_De_Desenho permi_Desenho
    {
        get { return _permi_Desenho; }
        set
        {
            if (value != _permi_Desenho && Local_Client.instance != null && Local_Client.instance.transmitindo)
                instance?.Clear_Textures_ALL();
            _permi_Desenho = value;
            if (Local_Client.instance != null && Local_Client.instance.transmitindo)
            {
                Server.Client_Permi_Desenho_Type = _permi_Desenho;
                if (!Server.Online)
                {
                    Send_Client_Packet.Informar_Permi_Desenho((int)_permi_Desenho);
                }
            }
        }
    }
    private static Permissão_De_Desenho _permi_Desenho = Permissão_De_Desenho.todos;
    public static Desenho_Na_Tela instance;
    public static BrushType brush_Atual;
    public static Permissão_De_Desenho desenho_tipo_Preferido;
    public Permissão_De_Desenho desenho_Type_Atual;
    public static float fade_time = 0.5f;
    [Header("Canvas")]
    public RawImage tela_Fixa_Streamer;
    public RawImage tela_Temporária_Streamer;
    public RawImage tela_Viewer;

    public static int width = 640;
    public static int height = 480;

    [Header("Brush")]
    public static Color brushColor = Color.black;

    [Range(1f, 200f)]
    public static float brushSize = 20f;

    private RenderTexture _rt_Fixa;
    private RenderTexture _rt_Temporaria;
    [SerializeField]
    private Camera _cam_Essencial;
    internal RenderTexture _Cam_Visial_Essencial;

    private Material _brush_Material;
    private Material _erase_Material;
    private Mesh _brush_Mesh;

    private CommandBuffer _brush_commandBuffer;
    private CommandBuffer _erase_commandBuffer;
    private CommandBuffer _overlay_commandBuffer;

    private Vector2 _lastMousePosition;
    private bool drawing;
    private bool erasing;
    [SerializeField]
    private Shader _brush_Shader;
    [SerializeField]
    private Shader _erase_Shader;
    [SerializeField]
    private ComputeShader _Fade_Shader;
    [SerializeField]
    private ComputeShader _Clear_Shader;

    void Awake()
    {
        instance = this;
        if (width <= 0) width = 640;
        if (height <= 0) height = 480;
        Opcoes.SalvarConfiguracoes();

        // --------------------------------------------------
        // RenderTexture
        // --------------------------------------------------

        #region Fixa
        _rt_Fixa = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

        _rt_Fixa.enableRandomWrite = true;
        _rt_Fixa.filterMode = FilterMode.Point;
        _rt_Fixa.wrapMode = TextureWrapMode.Clamp;
        _rt_Fixa.useMipMap = false;
        _rt_Fixa.autoGenerateMips = false;

        _rt_Fixa.Create();

        tela_Fixa_Streamer.texture = _rt_Fixa;
        #endregion

        #region Temporaria
        _rt_Temporaria = new RenderTexture(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32
        );

        _rt_Temporaria.enableRandomWrite = true;
        _rt_Temporaria.filterMode = FilterMode.Point;
        _rt_Temporaria.wrapMode = TextureWrapMode.Clamp;
        _rt_Temporaria.useMipMap = false;
        _rt_Temporaria.autoGenerateMips = false;

        _rt_Temporaria.Create();

        tela_Temporária_Streamer.texture = _rt_Temporaria;
        #endregion


        // --------------------------------------------------
        // Material
        // --------------------------------------------------
        _brush_Material = new Material(_brush_Shader);
        _erase_Material = new Material(_erase_Shader);

        // --------------------------------------------------
        // Mesh do brush
        // --------------------------------------------------

        _brush_Mesh = new Mesh();

        _brush_Mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0)
        };

        _brush_Mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        _brush_Mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        _brush_Mesh.UploadMeshData(true);


        // --------------------------------------------------
        // CommandBuffer
        // --------------------------------------------------

        _brush_commandBuffer = new CommandBuffer();
        _brush_commandBuffer.name = "Paint Brush";

        _erase_commandBuffer = new CommandBuffer();
        _erase_commandBuffer.name = "Erase Brush";

        // --------------------------------------------------
        // Inicializa o canvas
        // --------------------------------------------------

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = _rt_Fixa;

        GL.Clear(true, true, Color.clear);

        RenderTexture.active = previous;
    }

    public static void Iniciar_Assistir() { width = Server.Desenho_width; height = Server.Desenho_height; }
    public static void Iniciar_Transmitir() {
        if (Local_Client.instance != null && instance != null)
        {
            if(Local_Client.instance.transmitindo)
                instance?.Clear_Textures_ALL();
        }
        width = Opcoes.config_Atual.D_width; height = Opcoes.config_Atual.D_height;

    }
    public static void Escolheu_Janela(bool especifica)
    {
        if (especifica)
        {
            instance?._cam_Essencial.gameObject.SetActive(true);
            int width = Captura_De_Tela.instance.window.texture.width;
            int height = Captura_De_Tela.instance.window.texture.height;
            var gfxType = SystemInfo.graphicsDeviceType;
            var format = WebRTC.GetSupportedRenderTextureFormat(gfxType);

            // Cria a RenderTexture
            instance._Cam_Visial_Essencial = new RenderTexture(width, height, 0, format);
            instance._cam_Essencial.targetTexture = instance._Cam_Visial_Essencial;
        }
        else
        {
            instance?._cam_Essencial.gameObject.SetActive(false);
        }
    }
    public static void Parar_Transmitir()
    {
        instance?._cam_Essencial.gameObject.SetActive(false);
    }

    public void Iniciar_Desenho(BrushType b_tipo)
    {
        drawing = false;
        erasing = false;
        brush_Atual = b_tipo;
    }
    public void Definir_Size(float size)
    {
        brushSize = size;
    }
    public void Definir_Cor(Color cor)
    {
        brushColor = cor;
    }
    public void Para_Desenho()
    {
        brush_Atual = BrushType.nada;
    }
    public void Escolher_Tipo_Desenho_Preferido(int tipo)
    {
        if (tipo == 0)
            desenho_tipo_Preferido = Permissão_De_Desenho.fixo;
        else
            desenho_tipo_Preferido = Permissão_De_Desenho.temporario;
        Opcoes.SalvarConfiguracoes();
    }
    void Definir_M_Type_Atual()
    {
        if (brush_Atual == BrushType.nada)
            Mouses_Manager.meu_Mouse_type = Mouse_Type.nenhum;
        else if (brush_Atual == BrushType.lapis)
        {
            if (desenho_Type_Atual == Permissão_De_Desenho.fixo)
                Mouses_Manager.meu_Mouse_type = Mouse_Type.lapis;
            else
                Mouses_Manager.meu_Mouse_type = Mouse_Type.lapis_Temporario;
        }
        else if (brush_Atual == BrushType.borracha)
            Mouses_Manager.meu_Mouse_type = Mouse_Type.borracha;
        else if (brush_Atual == BrushType.conta_Gotas)
            Mouses_Manager.meu_Mouse_type = Mouse_Type.conta_Gotas;
    }
    public void Logica_Basica()
    {
        Definir_M_Type_Atual();
        if (tela_Fixa_Streamer == null || _brush_Material == null
            || FunçõesPrecalculadas.ismouseOverUI || FunçõesPrecalculadas.isOverArea)
            return;

        desenho_Type_Atual = desenho_tipo_Preferido;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (desenho_Type_Atual == Permissão_De_Desenho.fixo)
                desenho_Type_Atual = Permissão_De_Desenho.temporario;
            else
                desenho_Type_Atual = Permissão_De_Desenho.fixo;
        }

        Definir_M_Type_Atual();

        desenho_Type_Atual = Filtro_Permi(desenho_Type_Atual);

        bool Usou = false;
        Vector2 position = Vector2.zero;

        if (GetCanvasPosition(FunçõesPrecalculadas.mousePositionOnWorld, out position))
        {
            switch (brush_Atual)
            {
                case BrushType.nada:
                    break;
                case BrushType.lapis:
                    Usou = Fluxo_Lapis(position);
                    break;
                case BrushType.borracha:
                    Usou = fluxo_Borracha(position);
                    break;
                case BrushType.conta_Gotas:
                    Conta_Gotas(position);
                    break;
                default:
                    break;
            }
        }

        if (Local_Client.instance.assistindo)
            if (!Usou)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, BrushType.nada,
                    Permissão_De_Desenho.nenhuma, position, brushColor, brushSize);
    }
    #region Conta Gotas
    [SerializeField] Conta_Gotas _conta_Gotas;
    [SerializeField] Selecionador_De_Cor seletor_De_Cor_Transmissão;
    [SerializeField] Selecionador_De_Cor seletor_De_Cor_Assistindo;
    public void Conta_Gotas(Vector2 position)
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            position /= new Vector2(width, height);
            position.y = 1 - position.y;
            if (Local_Client.instance.transmitindo)
            {
                position.x *= WebRTC_Video_S_R.textura.width;
                position.y *= WebRTC_Video_S_R.textura.height;
                _conta_Gotas.ReadPixel(WebRTC_Video_S_R.textura, (int)position.x, (int)position.y, cor =>
                {
                    seletor_De_Cor_Assistindo.Carregar_Cor(cor);
                    seletor_De_Cor_Transmissão.Carregar_Cor(cor);
                });
            }
            else
            {
                position.x *= WebRTC_Video_S_R.instance.img_Recebida.texture.width;
                position.y *= WebRTC_Video_S_R.instance.img_Recebida.texture.height;
                _conta_Gotas.ReadPixel(WebRTC_Video_S_R.instance.img_Recebida.texture, (int)position.x, (int)position.y, cor =>
                {
                    seletor_De_Cor_Assistindo.Carregar_Cor(cor);
                    seletor_De_Cor_Transmissão.Carregar_Cor(cor);
                });
            }
        }
    }
    #endregion
    #region Lapis
    #region Local
    public bool Fluxo_Lapis(Vector2 position)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !erasing)
        {
            drawing = true;
            _lastMousePosition = position;
            if (Local_Client.instance.transmitindo)
                DrawBrush(position);
            else if (Local_Client.instance.assistindo)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, brush_Atual, desenho_Type_Atual, position, brushColor, brushSize);
            return true;
        }
        if (Input.GetKey(KeyCode.Mouse0) && drawing)
        {
            DrawLine(_lastMousePosition, position);

            _lastMousePosition = position;
            return true;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            drawing = false;
        }
        return false;
    }
    void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);

        // Espaçamento dos pontos do pincel.
        // Quanto menor, mais contínuo fica o traço.
        float spacing = Mathf.Max(1f, brushSize * 0.75f);

        int steps = Mathf.CeilToInt(distance / spacing);

        if (steps <= 0)
        {
            if (Local_Client.instance.transmitindo)
                DrawBrush(end);
            else if (Local_Client.instance.assistindo)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, brush_Atual, desenho_Type_Atual, end, brushColor, brushSize);
            return;
        }


        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;

            Vector2 position =
                Vector2.Lerp(start, end, t);

            if (Local_Client.instance.transmitindo)
                DrawBrush(position);
            else if (Local_Client.instance.assistindo)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, brush_Atual, desenho_Type_Atual, position, brushColor, brushSize);
        }
    }

    void DrawBrush(Vector2 position)
    {
        _brush_Material.SetColor("_Color", brushColor);
        _brush_Material.SetFloat("_BrushSize", brushSize);

        _brush_Material.SetVector("_BrushPosition", new Vector4(position.x, position.y, 0, 0));

        _brush_Material.SetVector("_Resolution", new Vector4(width, height, 0, 0));

        _brush_commandBuffer.Clear();

        if (desenho_Type_Atual == Permissão_De_Desenho.fixo)
            _brush_commandBuffer.SetRenderTarget(_rt_Fixa);
        else
            _brush_commandBuffer.SetRenderTarget(_rt_Temporaria);

        _brush_commandBuffer.DrawMesh(_brush_Mesh, Matrix4x4.identity, _brush_Material);

        Graphics.ExecuteCommandBuffer(_brush_commandBuffer);
    }
    #endregion
    #region Clientes
    public void Fluxo_Lapis_Recebido(Permissão_De_Desenho tipo_Desenho, Vector2 pos, Color cor, float size)
    {
        DrawBrush(tipo_Desenho, pos, cor, size);
    }
    void DrawBrush(Permissão_De_Desenho tipo_Desenho, Vector2 position, Color cor, float size)
    {
        _brush_Material.SetColor("_Color", cor);
        _brush_Material.SetFloat("_BrushSize", size);

        _brush_Material.SetVector("_BrushPosition", new Vector4(position.x, position.y, 0, 0));

        _brush_Material.SetVector("_Resolution", new Vector4(width, height, 0, 0));


        _brush_commandBuffer.Clear();

        if (tipo_Desenho == Permissão_De_Desenho.temporario)
            _brush_commandBuffer.SetRenderTarget(_rt_Temporaria);
        else
            _brush_commandBuffer.SetRenderTarget(_rt_Fixa);

        _brush_commandBuffer.DrawMesh(_brush_Mesh, Matrix4x4.identity, _brush_Material);

        Graphics.ExecuteCommandBuffer(_brush_commandBuffer);
    }
    #endregion
    #endregion
    #region Borracha
    #region Local

    public bool fluxo_Borracha(Vector2 position)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !drawing)
        {
            erasing = true;
            _lastMousePosition = position;

            if (Local_Client.instance.transmitindo)
                EraseBrush(position);
            else if (Local_Client.instance.assistindo)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, brush_Atual, desenho_Type_Atual, position, brushColor, brushSize);
            return true;
        }


        if (Input.GetKey(KeyCode.Mouse0) && erasing)
        {
            EraseLine(_lastMousePosition, position);

            _lastMousePosition = position;
            return true;
        }


        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            erasing = false;
        }
        return false;
    }
    void EraseLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);

        // Espaçamento dos pontos do pincel.
        // Quanto menor, mais contínuo fica o traço.
        float spacing = Mathf.Max(1f, brushSize * 0.25f);

        int steps = Mathf.CeilToInt(distance / spacing);

        if (steps <= 0)
        {
            if (Local_Client.instance.transmitindo)
                EraseBrush(end);
            else if (Local_Client.instance.assistindo)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, brush_Atual, desenho_Type_Atual, end, brushColor, brushSize);
            return;
        }


        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;

            Vector2 position =
                Vector2.Lerp(start, end, t);

            if (Local_Client.instance.transmitindo)
                EraseBrush(position);
            else if (Local_Client.instance.assistindo)
                Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, brush_Atual, desenho_Type_Atual, position, brushColor, brushSize);
        }
    }
    void EraseBrush(Vector2 position)
    {
        _erase_Material.SetFloat("_BrushSize", brushSize);

        _erase_Material.SetVector("_BrushPosition", new Vector4(position.x, position.y, 0, 0));

        _erase_Material.SetVector("_Resolution", new Vector4(width, height, 0, 0));


        _erase_commandBuffer.Clear();
        _erase_commandBuffer.SetRenderTarget(_rt_Fixa);

        _erase_commandBuffer.DrawMesh(_brush_Mesh, Matrix4x4.identity, _erase_Material);

        Graphics.ExecuteCommandBuffer(_erase_commandBuffer);
    }
    #endregion

    #region Clientes
    public void Fluxo_Erase_Recebido(Permissão_De_Desenho tipo_Desenho, Vector2 pos, float size)
    {
        EraseBrush(tipo_Desenho, pos, size);
    }
    void EraseBrush(Permissão_De_Desenho tipo_Desenho, Vector2 position, float size)
    {
        _erase_Material.SetFloat("_BrushSize", size);

        _erase_Material.SetVector("_BrushPosition", new Vector4(position.x, position.y, 0, 0));

        _erase_Material.SetVector("_Resolution", new Vector4(width, height, 0, 0));


        _erase_commandBuffer.Clear();
        _erase_commandBuffer.SetRenderTarget(_rt_Fixa);

        _erase_commandBuffer.DrawMesh(_brush_Mesh, Matrix4x4.identity, _erase_Material);

        Graphics.ExecuteCommandBuffer(_erase_commandBuffer);
    }
    #endregion
    #endregion

    #region Fade/Clear
    public void Fade_Temporario()
    {
        int kernel = _Fade_Shader.FindKernel("CSMain");

        _Fade_Shader.SetFloats("reduce_Speed", Time.deltaTime / fade_time);

        _Fade_Shader.SetTexture(kernel, "Result", _rt_Temporaria);

        int gruposX = Mathf.CeilToInt(_rt_Temporaria.width / 8f);
        int gruposY = Mathf.CeilToInt(_rt_Temporaria.height / 8f);

        _Fade_Shader.Dispatch(kernel, gruposX, gruposY, 1);
    }
    public void Clear_Textures_ALL()
    {
        if (_rt_Fixa != null)
            Clear_Textures(_rt_Fixa);
        if (_rt_Temporaria != null)
            Clear_Textures(_rt_Temporaria);
    }
    public void Clear_Textures(Texture textura)
    {
        int kernel = _Clear_Shader.FindKernel("CSMain");

        _Clear_Shader.SetTexture(kernel, "Result", textura);

        int gruposX = Mathf.CeilToInt(textura.width / 8f);
        int gruposY = Mathf.CeilToInt(textura.height / 8f);

        _Clear_Shader.Dispatch(kernel, gruposX, gruposY, 1);
    }
    #endregion
    private void Update()
    {
        if (!Local_Client.instance.transmitindo && !Local_Client.instance.assistindo)
        {
            erasing = false;
            drawing = false;
            return;
        }
        if (Local_Client.instance.transmitindo)
        {
            //Caso seja o host e esteja transmitindo, Retira o alpha da opção de desenho temporário
            Fade_Temporario();
            if (permi_Desenho != Permissão_De_Desenho.nenhuma && Server.Client_Permi_Desenho_Type != Permissão_De_Desenho.nenhuma)
                Logica_Basica();
            else
            {
                erasing = false;
                drawing = false;
            }
        }
        else
        {
            if (Server.Client_Permi_Desenho_Type != Permissão_De_Desenho.nenhuma)
                Logica_Basica();
            else
            {
                Definir_M_Type_Atual();

                if (Local_Client.instance.assistindo)
                    if (GetCanvasPosition(Input.mousePosition, out Vector2 position))
                        Local_Client.instance.Envio_Mouse(Mouses_Manager.meu_Mouse_type, BrushType.nada,
                            Permissão_De_Desenho.nenhuma, position, brushColor, brushSize);
                erasing = false;
                drawing = false;
            }
        }
    }

    bool GetCanvasPosition(Vector2 screenPosition, out Vector2 canvasPosition)
    {
        Vector2 localPosition = Vector2.zero;

        if (Local_Client.instance.transmitindo)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tela_Fixa_Streamer.rectTransform,
                screenPosition,
                null,
                out localPosition))
            {
                canvasPosition = Vector2.zero;
                return false;
            }
        }
        else if (Local_Client.instance.assistindo)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(tela_Viewer.rectTransform, screenPosition, null, out localPosition))
            {
                canvasPosition = Vector2.zero;
                return false;
            }
        }

        Rect r = tela_Fixa_Streamer.rectTransform.rect;


        // 0..1
        float normalizedX = Mathf.InverseLerp(r.xMin, r.xMax, localPosition.x);

        float normalizedY = 0;

        if (Local_Client.instance.transmitindo)
        {
            normalizedY = Mathf.InverseLerp(r.yMin, r.yMax, localPosition.y);
        }
        else if (Local_Client.instance.assistindo)
        {
            normalizedY = Mathf.InverseLerp(r.yMax, r.yMin, localPosition.y);
        }


        // 0..canvas resolution
        //canvasPosition = new Vector2(normalizedX, normalizedY);
        canvasPosition = new Vector2(normalizedX * width, normalizedY * height);

        return true;
    }
    public void Desativar()
    {
        if (_brush_commandBuffer != null)
        {
            _brush_commandBuffer.Release();
            _brush_commandBuffer = null;
        }

        if (_brush_Material != null)
        {
            Destroy(_brush_Material);
            _brush_Material = null;
        }

        if (_rt_Fixa != null)
        {
            _rt_Fixa.Release();
            Destroy(_rt_Fixa);
            _rt_Fixa = null;
        }
    }
    [SerializeField] RectTransform cv;
    internal void Receber_Mouse(int client_ID,Mouse_Type m_tipo,BrushType tipo, Permissão_De_Desenho tipo_Desenho, Vector2 pos, Color cor, float size)
    {
        Rect r = tela_Fixa_Streamer.rectTransform.rect;
        Vector2 pos_Convertida = pos;
        pos_Convertida.x /= _rt_Fixa.width;
        pos_Convertida.y /= _rt_Fixa.height;
        pos_Convertida -= new Vector2(0.5f, 0.5f);
        pos_Convertida *= FunçõesPrecalculadas.To_Camera_Size;
        //pos_Convertida.x *= cv.rect.width;
        //pos_Convertida.y *= cv.rect.height;

        Mouses_Manager.instance.Mover_Mouse(client_ID, m_tipo, pos_Convertida, cor);
        if (permi_Desenho == Permissão_De_Desenho.nenhuma || Server.Client_Permi_Desenho_Type == Permissão_De_Desenho.nenhuma)
            return;

        tipo_Desenho = Filtro_Permi(tipo_Desenho);

        switch (tipo)
        {
            case BrushType.nada:
                break;
            case BrushType.lapis:
                Fluxo_Lapis_Recebido(tipo_Desenho, pos, cor, size);
                break;
            case BrushType.borracha:
                Fluxo_Erase_Recebido(tipo_Desenho, pos, size);
                break;
            default:
                break;
        }
    }

    Permissão_De_Desenho Filtro_Permi(Permissão_De_Desenho tipo)
    {
        if (permi_Desenho == Permissão_De_Desenho.todos)
            return tipo;
        else if(permi_Desenho == Permissão_De_Desenho.fixo)
            return Permissão_De_Desenho.fixo;
        else if(permi_Desenho == Permissão_De_Desenho.temporario)
            return Permissão_De_Desenho.temporario;
        return Permissão_De_Desenho.nenhuma;
    }
}
