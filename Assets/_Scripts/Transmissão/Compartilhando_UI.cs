using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;

public class Compartilhando_UI : MonoBehaviour
{
    public GameObject IG_Recolhido;
    public GameObject IG;
    public GameObject aba_Cores;
    public GameObject aba_Lapis_Borracha_OP;
    [SerializeField] Captura_De_Tela cap_Pref;
    internal Captura_De_Tela cap_D_T;
    [SerializeField] Image color_Selected_IMG;
    public static Compartilhando_UI instance;
    private void Update()
    {
        if ((Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1)))
        {
            TransparencyWindowController.forceClickable = false;
            if (IG.activeSelf)
            {
                if (Desenho_Na_Tela.brush_Atual == BrushType.nada && !aba_OP_Simples.gameObject.activeSelf && !aba_Cores.activeSelf
                    && !Menu_Inicial_UI.instance.Opções.activeSelf)
                {
                    Alterar_Info_Geral(false);
                }
                else
                {
                    Desenho_Na_Tela.instance.Para_Desenho();
                    TransparencyWindowController.forceClickable = false;
                    aba_Lapis_Borracha_OP.SetActive(false);
                    Fechar_OP_CR();
                    Menu_Inicial_UI.instance.FecharOpções();
                }
            }
        }

    }
    #region Captura Area
    internal void Iniciar_Captura(Complet_Screen_ID id)
    {
        if (cap_D_T != null)
        {
            DestroyImmediate(cap_D_T.gameObject);
            cap_D_T = null;
        }

        cap_D_T = Instantiate(cap_Pref, transform);
        cap_D_T.transform.SetAsFirstSibling();

        if (id.window_ID == -2)
        {
            cap_D_T.type = WindowTextureType.Desktop;
        }
        else
        {
            cap_D_T.type = WindowTextureType.Window;
            cap_D_T.window = UwcManager.windows[id.window_ID];
        }
    }
    internal void Parar_De_Compartilhar()
    {
        if (cap_D_T != null)
        {
            DestroyImmediate(cap_D_T.gameObject);
            cap_D_T = null;
        }
        WebRTC_Video_S_R.instance.Parar_Compartilhamento();
    }
    #endregion
    public void Alterar_Info_Geral(bool ativo)
    {
        TransparencyWindowController.forceClickable = false;
        if (ativo)
        {
            color_Selected_IMG.color = Desenho_Na_Tela.brushColor;
        }
        IG.SetActive(ativo);
        IG_Recolhido.SetActive(!ativo);
        aba_Lapis_Borracha_OP.SetActive(false);
        Fechar_OP_CR();
    }
    public void Começar_Desenho()
    {
        TransparencyWindowController.forceClickable = true;
        Desenho_Na_Tela.instance.Iniciar_Desenho(BrushType.lapis);
        aba_Lapis_Borracha_OP.SetActive(true);
        Fechar_OP_CR();
        TransparencyWindowController.instance.RestaurarFocoDaJanela();
    }
    public void Começar_A_Apagar()
    {
        TransparencyWindowController.forceClickable = true;
        Desenho_Na_Tela.instance.Iniciar_Desenho(BrushType.borracha);
        aba_Lapis_Borracha_OP.SetActive(true);
        Fechar_OP_CR();
        TransparencyWindowController.instance.RestaurarFocoDaJanela();
    }
    public void Começar_ContaGotas()
    {
        TransparencyWindowController.forceClickable = true;
        Desenho_Na_Tela.instance.Iniciar_Desenho(BrushType.conta_Gotas);
        aba_Lapis_Borracha_OP.SetActive(false);
        Fechar_OP_CR();
        TransparencyWindowController.instance.RestaurarFocoDaJanela();
    }
    [SerializeField] Selecionador_De_Cor sdc;
    public void Abrir_Seleção_Cores()
    {
        TransparencyWindowController.forceClickable = false;
        aba_OP_Simples.gameObject.SetActive(false);
        aba_Cores.SetActive(true);
        aba_Lapis_Borracha_OP.SetActive(false);
        sdc.Carregar_Info();
    }
    #region opções rápidas
    [SerializeField] OP_Rapidas_Compartilhamento aba_OP_Simples;
    public void Abrir_Aba_OP_Simples()
    {
        TransparencyWindowController.forceClickable = false;
        aba_OP_Simples.gameObject.SetActive(true);
        aba_OP_Simples.Carregar_Perite_Desenho();
        aba_OP_Simples.Carregar_Desenho_Preferido();
        aba_Cores.SetActive(false);
        aba_Lapis_Borracha_OP.SetActive(false);
    }
    #endregion
    public void Fechar_OP_CR()
    {
        aba_OP_Simples.gameObject.SetActive(false);
        aba_Cores.SetActive(false);
    }
    public void Sair_Transmissão()
    {
        TransparencyWindowController.forceClickable = false;
        aba_Lapis_Borracha_OP.SetActive(false);
        Alterar_Info_Geral(false);
        Desenho_Na_Tela.instance.Para_Desenho();
        #region Desativa toda forma de captura
        Parar_De_Compartilhar();
        #endregion
        Sala_UI.instance.Abir_Espera();
    }
    private void OnEnable()
    {
        Alterar_Info_Geral(false);
    }
}
