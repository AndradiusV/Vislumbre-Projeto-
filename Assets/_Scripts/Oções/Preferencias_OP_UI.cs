using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Preferencias_OP_UI : MonoBehaviour
{
    #region TG Pode desenhar
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
    #region Seletor de Audio Excluder
    [SerializeField] Seletor_Exclude_Program_Audio Seletor_De_Prog_Ignorar;
    public void Abrir_Seletor()
    {
        Seletor_De_Prog_Ignorar.Abrir();
    }
    #endregion
}
