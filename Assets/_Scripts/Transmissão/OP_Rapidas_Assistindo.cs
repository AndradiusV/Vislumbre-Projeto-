using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OP_Rapidas_Assistindo : MonoBehaviour
{
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
}
