using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OP_Rapidas_Compartilhamento : MonoBehaviour
{
    #region DP Pode desenhar
    [SerializeField] TMP_Dropdown tg_Pode_Desenho;
    public void Carregar_Perite_Desenho()
    {
        tg_Desenho_Preferido.SetValueWithoutNotify((int)Desenho_Na_Tela.permi_Desenho);
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
    [SerializeField] Seletor_Exclude_Program_Audio Seletor_De_Prog_Ignorar;
    public void Abrir_Seletor_Exclud_Prog_Audio()
    {
        Seletor_De_Prog_Ignorar.Abrir();
    }
}
