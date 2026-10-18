using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Opcoes_UI : MonoBehaviour
{
    public static Opcoes_UI instance;
    private void Awake()
    {
        instance = this;
        Abrir_Geral();
    }
    public void Abrir()
    {
        Menu_Inicial_UI.instance.AbrirOpções();
        Abrir_Geral();
    }
    public GameObject aba_Geral;
    public GameObject aba_Trans;
    public GameObject aba_Preferencias;
    public void Abrir_Geral()
    {
        aba_Geral.SetActive(true);
        aba_Trans.SetActive(false);
        aba_Preferencias.SetActive(false);

        G_OP_UI.Carregar_Volume_Geral(AudioListener.volume);
        G_OP_UI.Carregar_Volume_Ping(SoundManager.volume);
        G_OP_UI.Carregar_Avisos(Avisos.avisos);
    }
    public void Abrir_Trans()
    {
        aba_Geral.SetActive(false);
        aba_Trans.SetActive(true);
        aba_Preferencias.SetActive(false);

        T_OP_UI.Carregar_Coding_Type();
        T_OP_UI.Carregar_FPS();
        T_OP_UI.Carregar_BitRate();
        T_OP_UI.Carregar_Resolução();
    }
    public void Abrir_Preferencias()
    {
        aba_Geral.SetActive(false);
        aba_Trans.SetActive(false);
        aba_Preferencias.SetActive(true);
        P_OP_UI.Carregar_Perite_Desenho();
    }

    #region Geral
    [SerializeField] Geral_OP_UI G_OP_UI;
    #endregion
    #region Trans
    [SerializeField] Transmissao_OP_UI T_OP_UI;
    #endregion
    #region Pref
    [SerializeField] Preferencias_OP_UI P_OP_UI;
    #endregion
}
