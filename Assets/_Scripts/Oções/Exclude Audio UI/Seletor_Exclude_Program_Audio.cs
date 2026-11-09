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
    [SerializeField] List<Exclude_Program_Audio_Item> _programas_Itens_Predefinidos = new();
    [SerializeField] Sprite sr_Vazio;
    private void Awake()
    {
        instance = this;
    }
    public void Abrir()
    {
        gameObject.SetActive(true);

        //Verifica se tem algum programa excluido salvo nas opcoes, se tiver, atualiza o texto e o icone do programa escolhido
        foreach (var item in _programas_Itens_Predefinidos)
        {
            if(item.nome_Program == Opcoes.config_Atual.prog_Audi_Excluido)
            {
                _programa_Escolhido.text = item.nome_Program;
                _programa_Icon.sprite = item._icon.sprite;
                return;
            }
        }
        //Se não tiver nenhum programa excluido salvo nas opcoes, atualiza o texto e o icone do programa escolhido para vazio
        _programa_Escolhido.text = "Nenhum selecionado";
        _programa_Icon.sprite = sr_Vazio;
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
        Audio_Trans_Manager.prog_Audi_Excluido = "Nenhum selecionado";
        Opcoes.SalvarConfiguracoes();
        Atualizar_Escolhido("", sr_Vazio);
    }
    /// <summary>
    /// O objeto deste Auto é pegar todos os programas que estão rodando, e verificar qual deles é um dos que está na lista.
    /// Assim que encontrar, ele irá definir este como o programa para ter o  audio excluido.
    /// </summary>
    public void Auto()
    {
        throw new System.NotImplementedException("Ainda não implementado fds");
    }
    public void Fechar()
    {
        gameObject.SetActive(false);
    }
}
