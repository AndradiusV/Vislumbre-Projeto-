using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolatiusDataBase.FastDebug;

public class RegiãoClicavel : MonoBehaviour
{
    //Pode mudar quando foi intrudizido o sistema de tradução
    public string titulo;
    public string descrição;

    public float mod_Speed_Aparecer = 1;

    public Bounds bound;
    [SerializeField] RectTransform rectTransform;
    public void Start()
    {
        Atualizar();
        FunçõesPrecalculadas.Adicionar_Area(this);
    }
    public bool Ativado() {  return rectTransform.gameObject.activeInHierarchy; }
    public void Atualizar()
    {
        Vector2 size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        size *= FunçõesPrecalculadas.Canvas_Ajust_Size;
        Vector2 Center = (Vector2)rectTransform.position + (size * new Vector2(.5f - rectTransform.pivot.x, .5f - rectTransform.pivot.y));
        bound = new(Center, size);
    }
    private void OnDrawGizmos()
    {
        FastDebug.DesenharBoundMinMax(bound, Color.green);
    }
}
