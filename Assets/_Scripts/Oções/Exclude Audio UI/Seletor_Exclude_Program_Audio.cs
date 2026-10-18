using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Seletor_Exclude_Program_Audio : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _programa_Escolhido;
    [SerializeField] Image _programa_Icon;
    public static Seletor_Exclude_Program_Audio instance;
    private void Awake()
    {
        instance = this;
    }
    public void Abrir()
    {
        gameObject.SetActive(true);

        _programa_Escolhido.text = Opcoes.config_Atual.prog_Audi_Excluido;
    }
    public void Atualizar_Escolhido(string program_Nome,Sprite sp)
    {
        _programa_Escolhido.text = program_Nome;
        _programa_Icon.sprite = sp;
        if (Local_Client.instance.transmitindo)
        {
            Audio_Trans_Manager.instance.Parar_Capture();
            Local_Client.instance.Iniciar_Audio_Capture();
        }
    }
    public void Limpar()
    {
        Audio_Trans_Manager.prog_Audi_Excluido = "";
        Opcoes.SalvarConfiguracoes();
        Atualizar_Escolhido("", null);
    }
    public void Auto()
    {

    }
    public void Fechar()
    {
        gameObject.SetActive(false);
    }
}
