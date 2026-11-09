using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Preferencias_OP_UI : MonoBehaviour
{
    #region TMP Pode desenhar
    [SerializeField] TMP_Dropdown tg_Pode_Desenho;
    public void Carregar_Perite_Desenho()
    {
        tg_Pode_Desenho.SetValueWithoutNotify((int)Desenho_Na_Tela.permi_Desenho);
    }
    public void Alterar_Permi_Desenho(int valor)
    {
        Desenho_Na_Tela.permi_Desenho = (Permissão_De_Desenho)valor;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
    #region TG Pode desenhar
    [SerializeField] TMP_Dropdown tg_Desenho_Preferido;
    public void Carregar_Desenho_Preferido()
    {
        if (Desenho_Na_Tela.desenho_tipo_Preferido == Permissão_De_Desenho.fixo)
            tg_Desenho_Preferido.SetValueWithoutNotify(0);
        else if (Desenho_Na_Tela.desenho_tipo_Preferido == Permissão_De_Desenho.temporario)
            tg_Desenho_Preferido.SetValueWithoutNotify(1);
    }
    #endregion

    #region Pined
    [SerializeField] Image _is_Pined_IMG;
    [SerializeField] Sprite _pined_On;
    [SerializeField] Sprite _pined_Off;
    public void Carregar_Pined()
    {
        _is_Pined_IMG.sprite = TransparencyWindowController.is_Pined ? _pined_On : _pined_Off;
    }
    public void Alterar_Pined(bool ativo)
    {
        TransparencyWindowController.is_Pined = ativo;
        _is_Pined_IMG.sprite = TransparencyWindowController.is_Pined ? _pined_On : _pined_Off;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
    #region TG Descrições
    [SerializeField] Toggle tg_Descrições;
    public void Carregar_Descrições()
    {
        tg_Descrições.SetIsOnWithoutNotify(Regiões_ADM.descrições_Discretas);
    }
    public void Alterar_Descrições(bool valor)
    {
        Regiões_ADM.descrições_Discretas = valor;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
}
