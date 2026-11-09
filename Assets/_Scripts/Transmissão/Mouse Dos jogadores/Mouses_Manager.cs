using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mouses_Manager : MonoBehaviour
{
    internal static Mouse_Type meu_Mouse_type;
    internal static Mouses_Manager instance;
    private void Awake()
    {
        instance = this;
        //gameObject.SetActive(false);
    }
    [SerializeField] RawImage mouse_Pref;
    Dictionary<int,(RawImage,Mouse_Type)> mouses = new();
    internal void Adicionar_Mouse(int client_ID)
    {
        if (mouses == null)
            mouses = new();
        RawImage mouse = Instantiate(mouse_Pref, transform);
        mouse.gameObject.SetActive(true);
        if (mouses.ContainsKey(client_ID))
        {
            Remover_Mouse(client_ID);
            if (mouses.ContainsKey(client_ID))
                mouses[client_ID] = (mouse, Mouse_Type.nenhum);
            else
                mouses.Add(client_ID, (mouse, Mouse_Type.nenhum));
        }
        else
            mouses.Add(client_ID,(mouse,Mouse_Type.nenhum));
    }
    public void Remover_Mouse(int client_ID)
    {
        if (mouses == null || !mouses.ContainsKey(client_ID))
            return;

        if(mouses[client_ID].Item1 != null)
            Destroy(mouses[client_ID].Item1.gameObject);
        mouses.Remove(client_ID);
    }
    public void Limpar_Mouses()
    {
        foreach (var item in mouses)
        {
            Destroy(item.Value.Item1.gameObject);
        }
        mouses.Clear();
    }
    internal void Mover_Mouse(int client_ID, Mouse_Type tipo,Vector2 nova_Posicao,Color cor)
    {
        if (!mouses.ContainsKey(client_ID) || mouses[client_ID].Item1 == null)
            return;

        RawImage ri = mouses[client_ID].Item1;


        float x_dif = Mathf.Sign(nova_Posicao.x - ri.transform.position.x);

        ri.transform.position = nova_Posicao;
        if (tipo == Mouse_Type.nenhum)
                x_dif = 1;

        //if (Mathf.Abs(x_dif) >= .1f)
        //{
        //    ri.transform.localScale = new Vector3(x_dif,
        //        ri.transform.lossyScale.y,
        //        ri.transform.lossyScale.z);
        //}

        ri.color = cor;
        if (mouses[client_ID].Item2 != tipo)
        {
            switch (tipo)
            {
                case Mouse_Type.nenhum:
                    ri.texture = mouse_Normal;
                    break;
                case Mouse_Type.lapis:
                    ri.texture = lapis;
                    break;
                case Mouse_Type.lapis_Temporario:
                    ri.texture = lapis_Temporario;
                    break;
                case Mouse_Type.borracha:
                    ri.texture = borracha;
                    ri.color = Color.white;
                    break;
                case Mouse_Type.conta_Gotas:
                    ri.texture = conta_Gotas;
                    break;
                default:
                    break;
            }
            mouses[client_ID] = (ri, tipo);
        }
    }

    #region Texturas
    [SerializeField] Texture mouse_Normal;
    [SerializeField] Texture lapis;
    [SerializeField] Texture lapis_Temporario;
    [SerializeField] Texture borracha;
    [SerializeField] Texture conta_Gotas;
    #endregion
}
