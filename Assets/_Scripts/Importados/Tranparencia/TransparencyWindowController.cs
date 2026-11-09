using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

public class TransparencyWindowController : MonoBehaviour
{

#if UNITY_STANDALONE_WIN

    // Importações da API do Windows (user32.dll e dwmapi.dll)
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    // Importação correta e segura para 32 e 64 bits
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    // Para ler teclas mesmo quando a janela estiver sem foco (já que o clique vai vazar)
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")]
    private static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);


    // Constantes do Windows
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;


    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;

    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0001;
    //private const uint WS_EX_TOPMOST = 0x00000008;

    const int LWA_COLOKEY = 0x00000001;
    private const int VK_A = 0x41; // Tecla A

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private IntPtr hWnd;
    private uint originalStyle;
    private uint originalExStyle;
    // Variáveis para armazenar o estado original
    private IntPtr windowHandle;
    private RECT originalRect;

    // Variáveis da câmera
    private Camera mainCamera;
    private Color originalCameraColor;
    private CameraClearFlags originalClearFlags;
    internal static bool is_Pined
    {
        get { return _is_Pined; }
        set
        {
            if (value != _is_Pined)
            {
                _is_Pined = value;
                instance?.Atualizar_Pined_Mode();
            }
        }
    }
    private static bool _is_Pined = false;
    private bool isTransparent = false;
    private bool isCurrentlyClickable = true;

    public static bool forceClickable = false;
    public static TransparencyWindowController instance;
    void Start()
    {
        instance = this;
        Set();
    }
    void Set()
    {
        hWnd = GetActiveWindow();

        // Salva os valores originais para podermos restaurar depois
        originalStyle = GetWindowLong(hWnd, GWL_STYLE);
        originalExStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

        mainCamera = Camera.main;
        // Pega a referência da janela ativa da Unity
        windowHandle = GetActiveWindow();
        Application.runInBackground = true;
    }
    void Update()
    {

#if UNITY_EDITOR
        return;
#endif
        // 1. Verifica a sua condição (Ex: O mouse está dentro de uma área específica?)
        bool shouldBeClickable = FunçõesPrecalculadas.ismouseOverUI ||
            FunçõesPrecalculadas.isOverArea || forceClickable;

        if (hWnd == new IntPtr(0))
            Set();

        // 2. Só faz a chamada na API se o estado precisar mudar (evita lag)
        if (shouldBeClickable != isCurrentlyClickable)
        {
            SetClickable(shouldBeClickable);
            isCurrentlyClickable = shouldBeClickable;
            //Avisos.instance.Mostrar_Aviso_Fixo($"Pode clicar: {isCurrentlyClickable}");
        }

        //// Testes para as funções (Você pode mudar as teclas se quiser)
        //if (Input.GetKeyDown(KeyCode.T) && !isTransparent)
        //{
        //    EnableTransparentFullscreen();
        //}
        //else if (Input.GetKeyDown(KeyCode.R) && isTransparent)
        //{
        //    RestoreWindow();
        //}
        //if (IsKeyDown(VK_A))
        //{
        //    RestaurarFocoDaJanela();
        //}
        //Avisos.instance.Mostrar_Aviso_Fixo(isCurrentlyClickable + ":"+Input.mousePosition.ToString());
    }
    /// <summary>
    /// Função para deixar transparente, tela cheia e sempre no topo.
    /// </summary>
    public void EnableTransparentFullscreen()
    {

#if UNITY_EDITOR
        return;
#endif
        if (isTransparent) return;

        // 1. Salva as configurações originais da câmera
        originalCameraColor = mainCamera.backgroundColor;
        originalClearFlags = mainCamera.clearFlags;

        // 2. Prepara a câmera para renderizar o fundo transparente (Alpha 0)
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0, 0, 0, 0);

        // 3. Salva o tamanho, posição e estilos originais da janela
        GetWindowRect(windowHandle, out originalRect);
        originalStyle = GetWindowLong(windowHandle, GWL_STYLE);
        originalExStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);

        // 4. Aplica a transparência no Windows (DWM)
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(windowHandle, ref margins);

        // 5. Remove as bordas da janela e ativa a camada transparente
        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowLong(windowHandle, GWL_EXSTYLE, originalExStyle | WS_EX_LAYERED);

        // 6. Coloca em tela cheia e força a ficar sempre no topo (Topmost)
        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;
        SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, screenWidth, screenHeight, SWP_FRAMECHANGED);

        //SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLOKEY);
        isTransparent = true;
    }
    /// <summary>
    /// Função para restaurar a janela ao normal.
    /// </summary>
    public void RestoreWindow()
    {
#if UNITY_EDITOR
        return;
#endif
        if (!isTransparent || Local_Client.instance == null) return;

            // 1. Restaura a câmera
            mainCamera.clearFlags = originalClearFlags;
            mainCamera.backgroundColor = originalCameraColor;

            // 2. Remove o efeito de transparência do DWM
            MARGINS margins = new MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0 };
            DwmExtendFrameIntoClientArea(windowHandle, ref margins);

            // 3. Restaura os estilos originais da janela (Bordas, etc)
            SetWindowLong(windowHandle, GWL_STYLE, originalStyle);
            SetWindowLong(windowHandle, GWL_EXSTYLE, originalExStyle);

            // 4. Restaura a posição, tamanho original e tira o status de sempre no topo
            int width = originalRect.Right - originalRect.Left;
            int height = originalRect.Bottom - originalRect.Top;
            SetWindowPos(windowHandle, is_Pined && Local_Client.instance.assistindo ? HWND_TOPMOST : HWND_NOTOPMOST, originalRect.Left, originalRect.Top, width, height, 0);

            isTransparent = false;
    }
    public void Atualizar_Pined_Mode()
    {
        if (isTransparent) return;


        GetWindowRect(windowHandle, out originalRect);


        int width = originalRect.Right - originalRect.Left;
        int height = originalRect.Bottom - originalRect.Top;
        SetWindowPos(windowHandle, is_Pined && Local_Client.instance.assistindo ? HWND_TOPMOST : HWND_NOTOPMOST, originalRect.Left, originalRect.Top, width, height, 0);
    }
    public void Atualiar_Window_Pos()
    {
        if (isTransparent)
        {
            // 6. Coloca em tela cheia e força a ficar sempre no topo (Topmost)
            int screenWidth = Screen.currentResolution.width;
            int screenHeight = Screen.currentResolution.height;
            SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, screenWidth, screenHeight, 0);
        }
        else if(_is_Pined && Local_Client.instance.assistindo)
        {
            GetWindowRect(windowHandle, out originalRect);


            int width = originalRect.Right - originalRect.Left;
            int height = originalRect.Bottom - originalRect.Top;
            SetWindowPos(windowHandle, HWND_TOPMOST, originalRect.Left, originalRect.Top, width, height, 0);
        }
    }
    /// <summary>
    /// Define se a tela é clicável ou não.
    /// </summary>
    /// <param name="clickable"></param>
    private void SetClickable(bool clickable)
    {
        uint currentStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

        if (clickable || !isTransparent)
        {
            // Remove a flag de transparente (Permite clicar na sua janela)
            SetWindowLong(hWnd, GWL_EXSTYLE, currentStyle & ~WS_EX_TRANSPARENT);
        }
        else
        {
            // Adiciona a flag de transparente (O clique atravessa a janela)
            SetWindowLong(hWnd, GWL_EXSTYLE, currentStyle | WS_EX_TRANSPARENT);
        }
    }
    // Função auxiliar para deixar o código mais limpo
    private bool IsKeyDown(int virtualKeyCode)
    {
        return (GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;
    }
    public void RestaurarFocoDaJanela()
    {
        if (hWnd == IntPtr.Zero) return;

        // Pega a janela que está com o foco atual no Windows
        IntPtr janelaComFoco = GetForegroundWindow();

        // Se a nossa janela já tem o foco, não precisamos fazer nada
        if (janelaComFoco == hWnd) return;

        // Descobre a "thread" da janela com foco e a thread da nossa janela
        uint threadJanelaComFoco = GetWindowThreadProcessId(janelaComFoco, IntPtr.Zero);
        uint minhaThread = GetWindowThreadProcessId(hWnd, IntPtr.Zero);

        if (threadJanelaComFoco != minhaThread)
        {
            // 1. "Engana" o Windows anexando nossa thread à do programa ativo
            AttachThreadInput(threadJanelaComFoco, minhaThread, true);

            // 2. Traz a janela para frente e força o foco nela
            SetForegroundWindow(hWnd);
            SetFocus(hWnd);

            // 3. Desfaz a conexão (SUPER IMPORTANTE para não bugar o Windows!)
            AttachThreadInput(threadJanelaComFoco, minhaThread, false);
        }
        else
        {
            // Fallback caso estejam na mesma thread
            SetForegroundWindow(hWnd);
            SetFocus(hWnd);
        }
    }
#endif
    }
