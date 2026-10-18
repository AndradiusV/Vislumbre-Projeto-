using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Geral_OP_UI : MonoBehaviour
{
    #region Geral
    [SerializeField] Slider VL_Geral_sl;
    public void Carregar_Volume_Geral(float valor)
    {
        VL_Geral_sl.SetValueWithoutNotify(valor);
    }
    public void Set_Volume_Geral(float valor)
    {
        AudioListener.volume = valor;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
    #region Pings
    [SerializeField] Slider VL_Ping_sl;
    public void Carregar_Volume_Ping(float valor)
    {
        VL_Ping_sl.SetValueWithoutNotify(valor);
    }
    public void Set_Volume_Ping(float valor)
    {
        SoundManager.volume = valor;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
    #region Avisos
    [SerializeField] Toggle avisos_tg;
    public void Carregar_Avisos(bool valor)
    {
        avisos_tg.SetIsOnWithoutNotify(valor);
        Avisos.avisos = valor;
    }
    public void TG_Avisos(bool valor)
    {
        Avisos.avisos = valor;
        Opcoes.SalvarConfiguracoes();
    }
    #endregion
}
