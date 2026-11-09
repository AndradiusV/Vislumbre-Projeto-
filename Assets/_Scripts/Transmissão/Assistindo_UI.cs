using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Assistindo_UI : MonoBehaviour
{
    public GameObject IG_Recolhido;
    public GameObject IG;
    public GameObject aba_Cores;
    public GameObject aba_OP_Simples;
    public GameObject aba_Lapis_Borracha_OP;
    [SerializeField] Image color_Selected_IMG;
    private void Update()
    {
        if ((Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1)))
        {
            if (IG.activeSelf)
            {
                if (Desenho_Na_Tela.brush_Atual == BrushType.nada && !aba_OP_Simples.activeSelf && !aba_OP_Simples.activeSelf
                    && !Menu_Inicial_UI.instance.Opções.activeSelf)
                {
                    Alterar_Info_Geral(false);
                }
                else
                {
                    Desenho_Na_Tela.instance.Para_Desenho();
                    aba_Lapis_Borracha_OP.SetActive(false);
                    Fechar_OP_CR();
                    Menu_Inicial_UI.instance.FecharOpções();
                }
            }
        }

    }
    public void Alterar_Info_Geral(bool ativo)
    {
        if (ativo)
        {
            color_Selected_IMG.color = Desenho_Na_Tela.brushColor;
            _is_Pined_IMG.sprite = TransparencyWindowController.is_Pined ? _pined_On : _pined_Off;
            TransparencyWindowController.instance.Atualizar_Pined_Mode();
        }
        IG.SetActive(ativo);
        IG_Recolhido.SetActive(!ativo);
        aba_Lapis_Borracha_OP.SetActive(false);
        Fechar_OP_CR();
    }
    public void Começar_Desenho()
    {
        Desenho_Na_Tela.instance.Iniciar_Desenho(BrushType.lapis);
        aba_Lapis_Borracha_OP.SetActive(true);
        Fechar_OP_CR();
    }
    public void Começar_A_Apagar()
    {
        Desenho_Na_Tela.instance.Iniciar_Desenho(BrushType.borracha);
        aba_Lapis_Borracha_OP.SetActive(true);
        Fechar_OP_CR();
    }
    public void Começar_ContaGotas()
    {
        Desenho_Na_Tela.instance.Iniciar_Desenho(BrushType.conta_Gotas);
        aba_Lapis_Borracha_OP.SetActive(false);
        Fechar_OP_CR();
    }
    [SerializeField] Selecionador_De_Cor sdc;
    public void Abrir_Seleção_Cores()
    {
        aba_OP_Simples.SetActive(false);
        aba_Cores.SetActive(true);
        aba_Lapis_Borracha_OP.SetActive(false);
        sdc.Carregar_Info();
    }
    #region Pined

    [SerializeField] Image _is_Pined_IMG;
    [SerializeField] Sprite _pined_On;
    [SerializeField] Sprite _pined_Off;
    public void Alterar_Pined(bool ativo)
    {
        TransparencyWindowController.is_Pined = ativo;
        _is_Pined_IMG.sprite = TransparencyWindowController.is_Pined ? _pined_On : _pined_Off;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
    #region Op
    [SerializeField] OP_Rapidas_Assistindo oP_Rapidas;
    public void Abrir_OP_Basica()
    {
        aba_OP_Simples.SetActive(true);
        aba_Cores.SetActive(false);
        aba_Lapis_Borracha_OP.SetActive(false);
        oP_Rapidas.Carregar_Desenho_Preferido();
    }
    public void Fechar_OP_CR()
    {
        aba_OP_Simples.SetActive(false);
        aba_Cores.SetActive(false);
    }
    public void Abrir_OP_Avançada()
    {
        Menu_Inicial_UI.instance.AbrirOpções();
    }
    #endregion
    public void Sair_Transmissão()
    {
        Alterar_Info_Geral(false);
        Sala_UI.instance.Abir_Espera();
    }
    private void OnEnable()
    {
        Alterar_Info_Geral(false);
    }
}
