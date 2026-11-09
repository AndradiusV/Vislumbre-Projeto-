using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Exclude_Program_Audio_Item_Primeira_Vez : MonoBehaviour
{
    public string nome_Program;
    [SerializeField] TextMeshProUGUI _nome_Program_TXT;
    public Texture2D icon_Program;
    [SerializeField] Image _icon;
    [SerializeField] Seletor_Exclude_Program_Audio_Primeira_Vez instance;
    public void Selecionar()
    {
        Audio_Trans_Manager.prog_Audi_Excluido = nome_Program;
        Opcoes.SalvarConfiguracoes();
        instance.Atualizar_Escolhido(nome_Program, _icon.sprite);
    }
}
