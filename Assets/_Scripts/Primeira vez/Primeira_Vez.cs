using GameServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Primeira_Vez : MonoBehaviour
{
    [SerializeField]
    GameObject ob_Primeira_Vez;
    [SerializeField]
    GameObject aba1;
    [SerializeField]
    GameObject aba2;
    [SerializeField]
    GameObject aba3;
    [SerializeField]
    GameObject aba4;
    public void Verificar_Primeira_Abertura()
    {
        if (Opcoes.config_Atual.primeira_Vez_entrando)
        {
            ob_Primeira_Vez.gameObject.SetActive(true);
            aba1.SetActive(true);
            aba2.SetActive(false);
            aba3.SetActive(false);
            aba4.SetActive(false);
            bt_Próximo2.interactable = false;
            bt_Próximo.interactable = false;
        }
        else
            ob_Primeira_Vez.gameObject.SetActive(false);
    }
    public void Abrir_Aba_2()
    {
        aba1.SetActive(false);
        aba2.SetActive(true);
        aba3.SetActive(false);
        aba4.SetActive(false);
        bt_Próximo2.interactable = false;
        bt_Próximo.interactable = false;
    }
    public void Abrir_Aba_3()
    {
        aba1.SetActive(false);
        aba2.SetActive(false);
        aba3.SetActive(true);
        aba4.SetActive(false);
        bt_Próximo2.interactable = false;
        bt_Próximo.interactable = false;
    }
    public void Abrir_Aba_4()
    {
        aba1.SetActive(false);
        aba2.SetActive(false);
        aba3.SetActive(false);
        aba4.SetActive(true);
        bt_Próximo2.interactable = false;
        bt_Próximo.interactable = false;
    }
    [SerializeField]
    Desenho_Config_Simples config_Simples;
    public void Finalizar()
    {
        Desenho_Na_Tela.brushColor = config_Simples.brushColor;
        Desenho_Na_Tela.desenho_tipo_Preferido = config_Simples.desenho_tipo_Preferido;
        GameManager.Primeira_Vez = false;
        Opcoes.SalvarConfiguracoes();
        Verificar_Primeira_Abertura();
    }
    public void Validar_AudioCatcher()
    {
        Audio_Trans_Manager.instance.Validar();
        bt_Próximo2.interactable = true;
    }
    public Button bt_Próximo;
    public Button bt_Próximo2;
    public TMP_InputField meu_ip_Area;
    public TextMeshProUGUI meu_ip_place_Holder;
    TimerToken _TK_definir_IP;
    public void Definir_IP(string ip)
    {
        if (_TK_definir_IP != null)
            _TK_definir_IP.Dispose();

        if (NetworkUtils.IS_IP_Valid(ip))
        {
            Menu_Inicial_UI.instance.Definir_Proprio_IP_TMP(ip);
            Menu_Inicial_UI.instance.Atualizar_Meu_IP();
            bt_Próximo.interactable = true;
        }
        else
        {
            bt_Próximo.interactable = false;
            meu_ip_Area.SetTextWithoutNotify("");

            if (_TK_definir_IP != null)
                _TK_definir_IP.Dispose();

            _TK_definir_IP = Timer.FeedBack_De_Texto("Introduza seu IPv4 do Radmin.", "IP Inválido", meu_ip_place_Holder, 1.5f, 2, Color.red);

        }
    }
    [SerializeField]
    GameObject selecionadorDeCor;
    public void Abrir_Selecionador_De_Cor()
    {
        selecionadorDeCor.SetActive(true);
    }
    public void Fechar_Selecionador_De_Cor()
    {
        selecionadorDeCor.SetActive(false);
    }
}
