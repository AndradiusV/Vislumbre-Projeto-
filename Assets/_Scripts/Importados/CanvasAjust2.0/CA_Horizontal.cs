using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VolatiusDataBase.FastDebug;

/// <summary>
/// Canvas Ajust Horizontal
/// </summary>
public class CA_Horizontal : CA_Content
{
    #region proporções
    public bool LimitScrool;

    public bool quadriangular;
    public bool travarDentro;
    public bool porcentagem;

    public bool fullHorizontal;
    public float horizontalAmount;
    public float horizontalSize;

    public bool fullVertical;
    public float verticalAmount;
    public float verticalSize;

    public float spaceAmount;
    public float spaceSize;

    public float BorderAmoun;
    public float BorderSize;
    #endregion

    #region Interação
    public override void DefinirBound()
    {
        base.DefinirBound();

        if (at == AlingType.A)
        {
            bound.min = RefRect.position - new Vector3(0, RefRect.rect.height / 2, 0);
            bound.max = RefRect.position + new Vector3(RefRect.rect.width, RefRect.rect.height / 2, 0);
        }
        else if (at == AlingType.B)
        {
            bound.min = RefRect.position - new Vector3(RefRect.rect.width / 2, RefRect.rect.height / 2, 0);
            bound.max = RefRect.position + new Vector3(RefRect.rect.width / 2, RefRect.rect.height / 2, 0);
        }
        else if (at == AlingType.C)
        {
            bound.min = RefRect.position - new Vector3(RefRect.rect.width, RefRect.rect.height, 0);
            bound.max = RefRect.position + new Vector3(0, RefRect.rect.height / 2, 0);
        }
    }
    #endregion

    #region Manegamento De filhos
    //public bool ignorarDesativados;
    #endregion
    #region Ajustes de tamanhos
    public override void DefinirProprioTamanho()
    {
        //escolhe o tamanho pela quantidade de filhos. Escolhe o estilo, então dá seu pivo e seus anchors
        if (at == AlingType.A)
        {
            myrect.pivot = new Vector2(0f, 0.5f);
            myrect.anchorMin = new Vector2(0, 0);
            myrect.anchorMax = new Vector2(0, 1);
            RefRect.pivot = new Vector2(0, .5f);
        }
        else if (at == AlingType.B)
        {
            myrect.pivot = new Vector2(0.5f, 0.5f);
            myrect.anchorMin = new Vector2(0.5f, 0f);
            myrect.anchorMax = new Vector2(0.5f, 1);
            RefRect.pivot = new Vector2(.5f, .5f);
        }
        else if (at == AlingType.C)
        {
            myrect.pivot = new Vector2(1f, 0.5f);
            myrect.anchorMin = new Vector2(1, 0);
            myrect.anchorMax = new Vector2(1, 1);
            RefRect.pivot = new Vector2(1, .5f);
        }
        x_size = (activeChildren * RefRect.rect.width * horizontalAmount) 
            + (Mathf.Clamp(activeChildren - 1,0, activeChildren) * RefRect.rect.width * spaceAmount)
            + (RefRect.rect.width * BorderAmoun * 2);

        myrect.sizeDelta = new Vector2(x_size, 0);
    }

    public override void DefinirFilhos()
    {
        float _relaçãox = (horizontalAmount * RefRect.rect.width) / myrect.rect.width;
        float _relaçãoy = 0;
        if (!quadriangular)
            _relaçãoy = (verticalAmount * RefRect.rect.height) / myrect.rect.height;
        else
        {
            if (travarDentro)
                horizontalAmount = Mathf.Clamp(horizontalAmount, 0, RefRect.rect.height / RefRect.rect.width);
            _relaçãoy = (horizontalAmount * RefRect.rect.width) / myrect.rect.height;
            verticalAmount = _relaçãoy;
        }
        float B_x = 0;
        if (at == AlingType.B)
        {
            B_x = activeChildren * horizontalAmount * RefRect.rect.width / 2 + (RefRect.rect.width * BorderAmoun)
                + Mathf.Clamp(activeChildren - 1, 0, activeChildren) * spaceAmount * RefRect.rect.width / 2;

            if (FunçõesPrecalculadas.Canvas_Ajust_Size != Vector2.zero)
            {
                B_x *= FunçõesPrecalculadas.Canvas_Ajust_Size.x;
            }
        }
        if (UseNormalChild)
        {
            for (int i = 0; i < activeChildren; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out RectTransform rc))
                {
                    #region Anchor ajust
                    rc.anchorMin = new Vector2(0, 0);
                    rc.anchorMax = new Vector2(0, 0);
                    rc.sizeDelta = new Vector2(1, 1);
                    rc.anchorMin = new Vector2(0, 0);
                    rc.anchorMax = new Vector2(_relaçãox, _relaçãoy);
                    rc.sizeDelta = new Vector2(0, 0);
                    #endregion

                    #region pivo
                    if (cpa == ChildPivoAling.A)
                        rc.pivot = new Vector2(0, .5f);
                    else if (cpa == ChildPivoAling.B)
                        rc.pivot = new Vector2(.5f, .5f);
                    else if (cpa == ChildPivoAling.C)
                        rc.pivot = new Vector2(1f, .5f);
                    #endregion

                    #region position

                    float x = (i * (spaceAmount + horizontalAmount)) * RefRect.rect.width + (RefRect.rect.width * BorderAmoun);

                    if (FunçõesPrecalculadas.Canvas_Ajust_Size != Vector2.zero)
                    {
                        x *= FunçõesPrecalculadas.Canvas_Ajust_Size.x;
                    }
                    if (at == AlingType.A)
                        rc.position = new Vector2(x, 0) + (Vector2)myrect.position;
                    else if (at == AlingType.B)
                        rc.position = new Vector2(x - B_x, 0) + (Vector2)myrect.position +
                            (cpa == ChildPivoAling.B ? new Vector2((horizontalAmount) / 2 * RefRect.rect.width, 0) : Vector2.zero);
                    else if (at == AlingType.C)
                        rc.position = new Vector2(-x, 0) + (Vector2)myrect.position;
                    #endregion

                }
            }
        }
        else
        {
            for (int i = 0; i < activeChildren; i++)
            {
                #region Anchor ajust
                filhos[i].rc.anchorMin = new Vector2(0, 0);
                filhos[i].rc.anchorMax = new Vector2(0, 0);
                filhos[i].rc.sizeDelta = new Vector2(1, 1);
                filhos[i].rc.anchorMin = new Vector2(0, 0);
                filhos[i].rc.anchorMax = new Vector2(_relaçãox, _relaçãoy);
                filhos[i].rc.sizeDelta = new Vector2(0, 0);
                #endregion

                #region pivo
                if (cpa == ChildPivoAling.A)
                    filhos[i].rc.pivot = new Vector2(0, .5f);
                else if (cpa == ChildPivoAling.B)
                    filhos[i].rc.pivot = new Vector2(.5f, .5f);
                else if (cpa == ChildPivoAling.C)
                    filhos[i].rc.pivot = new Vector2(1f, .5f);
                #endregion

                #region position

                float x = (i * (spaceAmount + horizontalAmount)) * RefRect.rect.width + (RefRect.rect.width * BorderAmoun);

                if (FunçõesPrecalculadas.Canvas_Ajust_Size != Vector2.zero)
                {
                    x *= FunçõesPrecalculadas.Canvas_Ajust_Size.x;
                }
                if (at == AlingType.A)
                    filhos[i].rc.position = new Vector2(x, 0) + (Vector2)myrect.position;
                else if (at == AlingType.B)
                    filhos[i].rc.position = new Vector2(x - B_x, 0) + (Vector2)myrect.position +
                        (cpa == ChildPivoAling.B ? new Vector2((horizontalAmount) / 2 * RefRect.rect.width, 0) : Vector2.zero);
                else if (at == AlingType.C)
                    filhos[i].rc.position = new Vector2(-x, 0) + (Vector2)myrect.position;
                #endregion
            }
        }
    }
    #region Movement
    public override void DefinirPropriaPos()
    {
        if (Application.isPlaying && !scrool)
            return;
        if (LimitScrool)
        {
            float tempValor = (1f / activeChildren) / 2;
            float quant0 = spaceAmount / (spaceAmount + horizontalAmount);
            tempValor -= tempValor * quant0;
            scroolPos = Mathf.Clamp(scroolPos, tempValor, 1 - tempValor);
        }
        float real_Pos = RefRect.position.x;
        if (at == AlingType.A)
        {
            //real_Pos -= RefRect.rect.width / 2;
            myrect.position = new Vector2(real_Pos - scroolPos * (x_size - RefRect.rect.width), RefRect.position.y);
        }
        else if (at == AlingType.B)
        {
            real_Pos += (x_size - RefRect.rect.width) / 2;
            myrect.position = new Vector2(real_Pos - scroolPos * (x_size - RefRect.rect.width), RefRect.position.y);
        }
        else if (at == AlingType.C)
        {
            //real_Pos += RefRect.rect.width / 2;
            myrect.position = new Vector2(real_Pos + (1 - scroolPos) * (x_size - RefRect.rect.width), RefRect.position.y);
        }

    }
    public override void FitFilhos()
    {
        float singleSize = horizontalAmount * RefRect.rect.width;
        float singleSize_half = horizontalAmount * RefRect.rect.width / 2;
        float half_ref = RefRect.rect.width;
        if (at == AlingType.B)
            half_ref = RefRect.rect.width / 2;
        if (ft == fitType.nenhum)
        {
        }
        else if (ft == fitType.somenteDentro)
        {
            float dif = 0;
            if (UseNormalChild)
            {
                for (int i = 0; i < activeChildren; i++)
                {
                    RectTransform rc = transform.GetChild(i).GetComponent<RectTransform>();
                    dif = rc.position.x;
                    if (cpa == ChildPivoAling.A)
                    {
                        if (dif < RefRect.position.x){
                            dif += singleSize;
                            if (RefRect.position.x - dif >= 0)
                            {
                                rc.gameObject.SetActive(false);
                                continue;
                            }
                        }
                    }
                    else if (cpa == ChildPivoAling.B)
                        if (dif < RefRect.position.x)
                            dif += singleSize_half;
                        else
                            dif -= singleSize_half;
                    else if (cpa == ChildPivoAling.C)
                        if (dif > RefRect.position.x){
                            dif -= singleSize;
                            if (RefRect.position.x - dif <= 0)
                            {
                                rc.gameObject.SetActive(false);
                                continue;
                            }
                        }
                    //FastDebug.Point(new Vector2(dif, rc.position.y), 20f, Color.magenta);
                    dif = RefRect.position.x - dif;
                    rc.gameObject.SetActive(Mathf.Abs(dif) <= half_ref);
                }
            }
            else
            {
                for (int i = 0; i < activeChildren; i++)
                {
                    dif = filhos[i].rc.position.x;
                    if (cpa == ChildPivoAling.A)
                    {
                        if (dif < RefRect.position.x){
                            dif += singleSize;
                            if (RefRect.position.x - dif >= 0)
                            {
                                filhos[i].Fit(false);
                                continue;
                            }
                        }
                    }
                    else if (cpa == ChildPivoAling.B)
                        if (dif < RefRect.position.x)
                            dif += singleSize_half;
                        else
                            dif -= singleSize_half;
                    else if (cpa == ChildPivoAling.C)
                        if (dif > RefRect.position.x){
                            dif -= singleSize;
                            if (RefRect.position.x - dif <= 0)
                            {
                                filhos[i].Fit(false);
                                continue;
                            }
                        }
                    dif = RefRect.position.x - dif;
                    filhos[i].Fit(Mathf.Abs(dif) <= half_ref);
                }
            }
        }
        else if (ft == fitType.fadeParaOsDefora)
        {

        }
    }
    #endregion
    #endregion
    public override void FixarFilhos()
    {
        //float _relaçãox = (horizontalAmount * RefRect.rect.width) / myrect.rect.width;
        //float _relaçãox_space = (spaceAmount * RefRect.rect.width) / myrect.rect.width;
        //float _relaçãox_Border = ((BorderAmoun /2)* RefRect.rect.width) / myrect.rect.width;
        //float _relaçãoy = 0;
        if (!quadriangular)
            verticalAmount = (verticalAmount * RefRect.rect.height) / myrect.rect.height;
        else
        {
            if (travarDentro)
                horizontalAmount = Mathf.Clamp(horizontalAmount, 0, RefRect.rect.height / RefRect.rect.width);
            verticalAmount = (horizontalAmount * RefRect.rect.width) / myrect.rect.height;
        }
        ResetarTamanho();
        for (int i = 0; i < activeChildren; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out RectTransform rc))
            {
                #region Anchor ajust
                rc.anchorMin = new Vector2(0, 0);
                rc.anchorMax = new Vector2(0, 0);
                rc.sizeDelta = new Vector2(1, 1);
                if(cpa == ChildPivoAling.A)
                {
                    rc.anchorMin = new Vector2(((horizontalAmount + spaceAmount) * i) + BorderAmoun, .5f - (verticalAmount / 2));
                    rc.anchorMax = new Vector2(horizontalAmount * (i + 1) + (spaceAmount * i) + BorderAmoun, .5f + (verticalAmount / 2));
                }
                if (cpa == ChildPivoAling.B)
                {
                    rc.anchorMin = new Vector2(((horizontalAmount + spaceAmount) * i) + BorderAmoun - ((horizontalAmount/2)), .5f - (verticalAmount / 2));
                    rc.anchorMax = new Vector2(horizontalAmount * (i + 1) + (spaceAmount * i) + BorderAmoun - ((horizontalAmount / 2)), .5f + (verticalAmount / 2));
                }
                rc.sizeDelta = new Vector2(0, 0);
                rc.anchoredPosition = new Vector2(0, 0);
                #endregion

            }
        }


        base.FixarFilhos();
    }
    private void OnDrawGizmosSelected()
    {
        if (bound != null)
            FastDebug.DesenharBoundMinMax(bound, Color.green);
        #region Próprio tamanho
        y_size = myrect.rect.height / 2f;
        Debug.DrawLine(myrect.position - Vector3.up * y_size, myrect.position + Vector3.up * y_size, Color.green);
        Vector3 leftPos = myrect.transform.position;
        Vector3 RightPos = myrect.transform.position;
        if (at == AlingType.A)
        {
            RightPos += new Vector3(myrect.rect.width, 0, 0);
        }
        else if (at == AlingType.B)
        {
            leftPos -= new Vector3(myrect.rect.width / 2, 0, 0);
            RightPos += new Vector3(myrect.rect.width / 2, 0, 0);
        }
        else
        {
            leftPos -= new Vector3(myrect.rect.width, 0, 0);
        }
        Debug.DrawRay(leftPos,new Vector3(RefRect.rect.width * BorderAmoun,0,0),Color.red);
        Debug.DrawRay(RightPos,new Vector3(-RefRect.rect.width * BorderAmoun,0,0),Color.red);
        #endregion
        #region Centro dos filhos
        float B_x = 0;
        if (at == AlingType.B)
        {
            B_x = activeChildren * horizontalAmount * RefRect.rect.width / 2 + (RefRect.rect.width * BorderAmoun)
                + Mathf.Clamp(activeChildren - 1, 0, activeChildren) * spaceAmount * RefRect.rect.width / 2;
        }
        for (int i = 0; i < activeChildren; i++)
        {
            #region position

            float x = (i * (spaceAmount + horizontalAmount)) * RefRect.rect.width + (RefRect.rect.width * BorderAmoun);
            Vector2 ppp = Vector2.zero;
            if (at == AlingType.A)
                ppp = new Vector2(x, 0) + (Vector2)myrect.position;
            else if (at == AlingType.B)
                ppp = new Vector2(x - B_x, 0) + (Vector2)myrect.position;
            else if (at == AlingType.C)
                ppp = new Vector2(x, 0) + (Vector2)myrect.position;
            #endregion
            //Debug.DrawRay(ppp-Vector2.up*y_size*((float)(i)/activeChildren), Vector3.right * horizontalAmount * RefRect.rect.width, Color.green);
            //Debug.DrawRay(ppp-Vector2.up*y_size*((float)(i+1)/(activeChildren +1))+ Vector2.right * horizontalAmount * RefRect.rect.width,
            //    Vector3.right * spaceAmount * RefRect.rect.width, Color.magenta);
            Debug.DrawRay(ppp, Vector3.right * horizontalAmount * RefRect.rect.width, Color.green);
            Debug.DrawRay(ppp+ Vector2.right * horizontalAmount * RefRect.rect.width,
                Vector3.right * spaceAmount * RefRect.rect.width, Color.magenta);

            if (at == AlingType.B)
                ppp += (cpa == ChildPivoAling.B ? new Vector2((horizontalAmount) / 2 * RefRect.rect.width, 0) : Vector2.zero);

            FastDebug.Point(ppp, 5f, Color.magenta);

        }
        #endregion
    }
}
//88x188