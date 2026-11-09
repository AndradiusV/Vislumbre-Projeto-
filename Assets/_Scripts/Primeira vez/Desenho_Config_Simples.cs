using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Desenho_Config_Simples : MonoBehaviour
{
    public BrushType brush_Atual;
    public Permissão_De_Desenho desenho_tipo_Preferido;
    public Permissão_De_Desenho desenho_Type_Atual;
    public float fade_time = 0.5f;
    [Header("Canvas")]
    public RawImage tela_Fixa_Streamer;
    public RawImage tela_Temporária_Streamer;

    public int width = 640;
    public int height = 480;

    [Header("Brush")]
    public Color brushColor = Color.black;

    [Range(1f, 200f)]
    public float brushSize = 20f;

    private RenderTexture _rt_Fixa;
    private RenderTexture _rt_Temporaria;

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
        if (width <= 0) width = 640;
        if (height <= 0) height = 480;
        Opcoes.SalvarConfiguracoes();

        desenho_tipo_Preferido = Permissão_De_Desenho.fixo;

        _rt_Fixa = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

        _rt_Fixa.useMipMap = false;
        _rt_Fixa.autoGenerateMips = false;
        _rt_Fixa.filterMode = FilterMode.Point;
        _rt_Fixa.wrapMode = TextureWrapMode.Clamp;
        _rt_Fixa.Create();

        // --------------------------------------------------
        // RenderTexture
        // --------------------------------------------------

        #region Fixa
        _rt_Fixa = new RenderTexture(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32
        );

        _rt_Fixa.enableRandomWrite = true;
        _rt_Fixa.filterMode = FilterMode.Bilinear;
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
        _rt_Temporaria.filterMode = FilterMode.Bilinear;
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

    public void Iniciar_Desenho(int b_tipo)
    {
        drawing = false;
        erasing = false;
        brush_Atual = (BrushType)b_tipo;
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
    public void Logica_Basica()
    {

        if (tela_Fixa_Streamer == null || _brush_Material == null)
            return;

        desenho_Type_Atual = desenho_tipo_Preferido;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (desenho_Type_Atual == Permissão_De_Desenho.fixo)
                desenho_Type_Atual = Permissão_De_Desenho.temporario;
            else
                desenho_Type_Atual = Permissão_De_Desenho.fixo;
        }

        Vector2 position = Vector2.zero;

        if (GetCanvasPosition(FunçõesPrecalculadas.mousePositionOnWorld, out position))
        {
            switch (brush_Atual)
            {
                case BrushType.nada:
                    break;
                case BrushType.lapis: 
                    Fluxo_Lapis(position);
                    break;
                case BrushType.borracha:
                    fluxo_Borracha(position);
                    break;
                case BrushType.conta_Gotas:
                    Conta_Gotas(position);
                    break;
                default:
                    break;
            }
        }
    }
    #region Conta Gotas
    [SerializeField] Conta_Gotas _conta_Gotas;
    [SerializeField] Selecionador_De_Cor_Primeira_Vez seletor_De_Cor;
    public void Conta_Gotas(Vector2 position)
    {
        if (Input.GetKey(KeyCode.Mouse0))
            _conta_Gotas.ReadPixel(_rt_Fixa, (int)position.x, (int)position.y, cor =>
            {
                seletor_De_Cor.Carregar_Cor(cor);
            });
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
            DrawBrush(position);
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
            DrawBrush(end);
            return;
        }


        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;

            Vector2 position =
                Vector2.Lerp(start, end, t);

            DrawBrush(position);
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
    #endregion
    #region Borracha
    #region Local

    public bool fluxo_Borracha(Vector2 position)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !drawing)
        {
            erasing = true;
            _lastMousePosition = position;

            EraseBrush(position);
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
            EraseBrush(end);
            return;
        }


        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;

            Vector2 position =
                Vector2.Lerp(start, end, t);

            EraseBrush(position);
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
        //Caso seja o host e esteja transmitindo, Retira o alpha da opção de desenho temporário
        Fade_Temporario();
        Logica_Basica();
    }

    bool GetCanvasPosition(Vector2 screenPosition, out Vector2 canvasPosition)
    {
        Vector2 localPosition = Vector2.zero;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tela_Fixa_Streamer.rectTransform,
            screenPosition,
            null,
            out localPosition))
        {
            canvasPosition = Vector2.zero;
            return false;
        }
        Rect r = tela_Fixa_Streamer.rectTransform.rect;

        if (Mathf.Abs(localPosition.x) > Mathf.Abs(r.xMax) || Mathf.Abs(localPosition.y) > Mathf.Abs(r.yMax))
        {
            canvasPosition = Vector2.zero;
            return false;
        }

        // 0..1
        float normalizedX = Mathf.InverseLerp(r.xMin, r.xMax, localPosition.x);

        float normalizedY = 0;

        normalizedY = Mathf.InverseLerp(r.yMin, r.yMax, localPosition.y);


        // 0..canvas resolution
        canvasPosition = new Vector2(normalizedX * width, normalizedY * height);

        return true;
    }
}
