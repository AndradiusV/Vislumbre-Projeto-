using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolatiusDataBase.FastDebug;

public abstract class CA_Content : MonoBehaviour
{
    public CA_item item;
    public bool Definido;
    public bool UseNormalChild;
    #region Scrool
    public bool scrool;
    public float scroolPos;
    public float scroolSpeed = 20;
    public void Scrollar(float quant)
    {
        scroolPos += Mathf.Sign(quant) * Time.deltaTime * scroolSpeed;
        scroolPos = Mathf.Clamp01(scroolPos);
        PedirParaAjustar();
    }
    public void MoverScrool(float quant)
    {
        scroolPos += (quant / activeChildren);
        PedirParaAjustar();
    }
    #endregion
    public ChildPivoAling cpa;
    public AlingType at;
    public fitType ft;

    private void Start()
    {
        UseNormalChild = true;
        PedirParaAjustar();
    }

    #region Interação
    public Bounds bound;
    public bool MouseDentro(Vector2 comp)
    {
        return bound != null && bound.Contains(comp);
    }
    public virtual void DefinirBound()
    {
        if(bound == null)
            bound = new Bounds();
    }
    #endregion

    #region Grupo dos filhos e objetos de tamanho
    public List<CA_item> filhos = new List<CA_item>();
    public List<CA_item> filhos_Desativados = new List<CA_item>();

    public RectTransform RefRect;
    protected RectTransform myrect => GetComponent<RectTransform>();
    public int activeChildren
    {
        get
        {
            if (UseNormalChild)
                return transform.childCount;
            return filhos.Count - filhos_Desativados.Count; ;
        }
    }
    #endregion
    #region Manager de filhos
    /// <summary>
    /// Usada para popular A CA com os objetos desejados,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pref"></param>
    /// <param name="quantidade"></param>
    /// <param name="limpar"></param>
    /// <returns></returns>
    public virtual List<T> Popular<T>(T pref, int quantidade, bool limpar = true) where T : CA_item
    {
        UseNormalChild = false;
        List<T> ret = new List<T>();
        if (limpar)
            LimparFilhos();
        for (int i = 0; i < quantidade; i++)
        {
            ret.Add(Instantiate(pref, transform));
            filhos.Add(ret[i]);
            ret[i].Set(this);
        }

        Definido = false;
        PedirParaAjustar();
        return ret;
    }
    /// <summary>
    /// Usada para popular A CA com seu item predefinido,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="quantidade"></param>
    /// <param name="limpar"></param>
    /// <returns></returns>
    public virtual List<T> Popular<T>(int quantidade, bool limpar = true) where T : CA_item
    {
        UseNormalChild = false;
        List<T> ret = new List<T>();
        if (limpar)
            LimparFilhos();
        for (int i = 0; i < quantidade; i++)
        {
            ret.Add(Instantiate(item as T, transform));
            filhos.Add(ret[i]);
            ret[i].Set(this);
        }

        Definido = false;
        PedirParaAjustar();
        return ret;
    }
    public T Adicionar<T>(T pref, bool limpar = true) where T : MonoBehaviour
    {
        if (limpar)
            LimparFilhos();
        GameObject ob = Instantiate(pref.gameObject, transform);
        T item = ob.GetComponent<T>();
        PedirParaAjustar();
        return item;
    }
    public T Adicionar<T>( bool limpar = true) where T : CA_item
    {
        UseNormalChild = false;
        if (limpar)
            LimparFilhos();
        T ob = Instantiate(item as T, transform);
        filhos.Add(ob);
        PedirParaAjustar();
        return ob;
    }
    public void LimparFilhos()
    {
        if (!UseNormalChild)
        {
            transform.DetachChildren();
            for (int i = 0; i < filhos.Count;)
            {
                Destroy(filhos[i].gameObject);
                filhos.RemoveAt(i);
            }
        }
        else
        {
            List<Transform> childs = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                childs.Add(transform.GetChild(i));
            }
            transform.DetachChildren();
            for (int i = 0; i < childs.Count; i++)
            {
                Destroy(childs[i].gameObject);
            }
        }
        ResetarTamanho();
    }
    #endregion

    #region Fixate
    public virtual void FixarFilhos()
    {
        print("Fixado");
        gameObject.name += "(Fixed)";
        DestroyImmediate(this);
    }
    #endregion

    protected virtual void LateUpdate()
    {
        if (!Definido)
        {
            //Ajusta Os objetos e atualiza o tamanho do Content
            if (RefRect.rect.width == 0f || activeChildren == 0)
            {
                ResetarTamanho();
            }
            DefinirBound();
            DefinirPropriaPos();
            DefinirProprioTamanho();
            DefinirFilhos();
            scroolPos = 0f;
            Ajustar();
            Definido = true;
        }
        if (ajustar)
            Ajustar();
    }
    #region Ajustes de tamanhos
    protected float x_size;
    protected float y_size;
    public abstract void DefinirProprioTamanho();
    public abstract void DefinirPropriaPos();
    public abstract void DefinirFilhos();
    public abstract void FitFilhos();

    /// <summary>
    /// Irá fazer com o que o tamanho deste objeto "content" fica com o msm tamanho e formado de sua referencia
    /// </summary>
    public void ResetarTamanho()
    {
        ajustar = false;
        
        myrect.pivot= new Vector2(0.5f,0.5f);
        myrect.anchorMin= new Vector2(0,0);
        myrect.anchorMax= new Vector2(1,1);
        myrect.sizeDelta = new Vector2(0,0);

        //Ajustará os filhos também!
    }
    /// <summary>
    /// Caso verdadiro, ao fim do frame ele irá se ajustar
    /// </summary>
    protected bool ajustar;
    public virtual void PedirParaAjustar()
    {
        if (ajustar)
            return;
        ajustar = true;
    }
    public virtual void Ajustar()
    {
        if (!ajustar)
            return;
        //Ajusta Os objetos e atualiza o tamanho do Content
        if (RefRect.rect.width == 0f || activeChildren == 0)
        {
            ResetarTamanho();
        }
        DefinirBound();
        DefinirPropriaPos();
        DefinirProprioTamanho();
        DefinirFilhos();
        scroolPos = Mathf.Clamp01(scroolPos);
        FitFilhos();
        ajustar = false;
    }
    #endregion
}
