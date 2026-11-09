using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VolatiusDataBase.FastDebug;

public class CA_Vertical : CA_Content
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
            bound.min = RefRect.position - new Vector3(RefRect.rect.width / 2, 0, 0);
            bound.max = RefRect.position + new Vector3(RefRect.rect.width / 2, RefRect.rect.height, 0);
        }
        else if (at == AlingType.B)
        {
            bound.min = RefRect.position - new Vector3(RefRect.rect.width / 2, RefRect.rect.height / 2, 0);
            bound.max = RefRect.position + new Vector3(RefRect.rect.width / 2, RefRect.rect.height / 2, 0);
        }
        else if (at == AlingType.C)
        {
            bound.min = RefRect.position - new Vector3(RefRect.rect.width / 2, RefRect.rect.height, 0);
            bound.max = RefRect.position + new Vector3(RefRect.rect.width / 2, 0, 0);
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
            myrect.pivot = new Vector2(0.5f, 0f);
            myrect.anchorMin = new Vector2(0, 0);
            myrect.anchorMax = new Vector2(1, 0);
            RefRect.pivot = new Vector2(.5f, 0);
        }
        else if (at == AlingType.B)
        {
            myrect.pivot = new Vector2(0.5f, 0.5f);
            myrect.anchorMin = new Vector2(0, 0.5f);
            myrect.anchorMax = new Vector2(1, 0.5f);
            RefRect.pivot = new Vector2(.5f, .5f);
        }
        else if (at == AlingType.C)
        {
            myrect.pivot = new Vector2(0.5f, 1f);
            myrect.anchorMin = new Vector2(0, 1);
            myrect.anchorMax = new Vector2(1, 1);
            RefRect.pivot = new Vector2(.5f, 1);
        }
        y_size = (activeChildren * RefRect.rect.height * verticalAmount)
            + (Mathf.Clamp(activeChildren - 1, 0, activeChildren) * RefRect.rect.height * spaceAmount)
            + (RefRect.rect.height * BorderAmoun * 2);
        myrect.sizeDelta = new Vector2(0, y_size);
    }

    public override void DefinirFilhos()
    {
        float _relaçãoy = (verticalAmount * RefRect.rect.height) / myrect.rect.height;
        float _relaçãox = 0;
        if (!quadriangular)
            _relaçãox = (horizontalAmount * RefRect.rect.width) / myrect.rect.width;
        else
        {
            if (travarDentro)
                verticalAmount = Mathf.Clamp(verticalAmount, 0, RefRect.rect.width/ RefRect.rect.height);
            _relaçãox = (verticalAmount * RefRect.rect.height) / myrect.rect.width;
            horizontalAmount = _relaçãox;
        }
        float B_y = 0;
        if (at == AlingType.B)
        {
            B_y = activeChildren * verticalAmount * RefRect.rect.height / 2 + (RefRect.rect.height * BorderAmoun)
                + Mathf.Clamp(activeChildren - 1, 0, activeChildren) * spaceAmount * RefRect.rect.height / 2;
            if (FunçõesPrecalculadas.Canvas_Ajust_Size != Vector2.zero)
            {
                B_y *= FunçõesPrecalculadas.Canvas_Ajust_Size.y;
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
                        rc.pivot = new Vector2(.5f, 0);
                    else if (cpa == ChildPivoAling.B)
                        rc.pivot = new Vector2(.5f, .5f);
                    else if (cpa == ChildPivoAling.C)
                        rc.pivot = new Vector2(.5f, 1f);
                    #endregion

                    #region position

                    float y = (i * (spaceAmount + verticalAmount)) * RefRect.rect.height + (RefRect.rect.height * BorderAmoun);

                    if (FunçõesPrecalculadas.Canvas_Ajust_Size != Vector2.zero)
                    {
                        y *= FunçõesPrecalculadas.Canvas_Ajust_Size.y;
                    }
                    if (at == AlingType.A)
                        rc.position = new Vector2(0, y) + (Vector2)myrect.position;
                    else if (at == AlingType.B)
                        rc.position = new Vector2(0, y - B_y) + (Vector2)myrect.position +
                            (cpa == ChildPivoAling.B ? new Vector2(0, (horizontalAmount) / 2 * RefRect.rect.height) : Vector2.zero);
                    else if (at == AlingType.C)
                        rc.position = new Vector2(0, -y) + (Vector2)myrect.position;
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
                    filhos[i].rc.pivot = new Vector2(.5f, 0);
                else if (cpa == ChildPivoAling.B)
                    filhos[i].rc.pivot = new Vector2(.5f, .5f);
                else if (cpa == ChildPivoAling.C)
                    filhos[i].rc.pivot = new Vector2(.5f, 1f);
                #endregion

                #region position

                float y = (i * (spaceAmount + verticalAmount)) * RefRect.rect.height + (RefRect.rect.height * BorderAmoun);

                if (FunçõesPrecalculadas.Canvas_Ajust_Size != Vector2.zero)
                {
                    y *= FunçõesPrecalculadas.Canvas_Ajust_Size.y;
                }
                if (at == AlingType.A)
                    filhos[i].rc.position = new Vector2(0, y) + (Vector2)myrect.position;
                else if (at == AlingType.B)
                    filhos[i].rc.position = new Vector2(0, y - B_y) + (Vector2)myrect.position +
                        (cpa == ChildPivoAling.B ? new Vector2(0, (verticalAmount) / 2 * RefRect.rect.height) : Vector2.zero);
                else if (at == AlingType.C)
                    filhos[i].rc.position = new Vector2(0, -y) + (Vector2)myrect.position;
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
            float quant0 = spaceAmount / (spaceAmount + verticalAmount);
            tempValor -= tempValor * quant0;
            scroolPos = Mathf.Clamp(scroolPos, tempValor, 1 - tempValor);
        }
        float real_Pos = RefRect.position.y;
        if (at == AlingType.A)
        {
            //real_Pos -= RefRect.rect.height / 2;
            myrect.position = new Vector2(RefRect.position.x, real_Pos - scroolPos * (y_size - RefRect.rect.height));
        }
        else if (at == AlingType.B)
        {
            real_Pos += (y_size - RefRect.rect.height) / 2;
            myrect.position = new Vector2(RefRect.position.x, real_Pos - scroolPos * (y_size - RefRect.rect.height));
        }
        else if (at == AlingType.C)
        {
            //real_Pos += RefRect.rect.height / 2;
            myrect.position = new Vector2(RefRect.position.x, real_Pos + scroolPos * (y_size - RefRect.rect.height));
        }

    }
    public override void FitFilhos()
    {
        float singleSize = verticalAmount * RefRect.rect.height;
        float singleSize_half = verticalAmount * RefRect.rect.height / 2;
        float half_ref = RefRect.rect.height;
        if (at == AlingType.B)
            half_ref = RefRect.rect.height /2;
        if (ft == fitType.nenhum)
        {
            float dif = 0;
            if (UseNormalChild)
            {
                for (int i = 0; i < activeChildren; i++)
                {
                    RectTransform rc = transform.GetChild(i).GetComponent<RectTransform>();
                    dif = rc.position.y;
                    if (cpa == ChildPivoAling.A)
                    {
                        if (dif < RefRect.position.y)
                            dif += singleSize;
                    }
                    else if (cpa == ChildPivoAling.B)
                        if (dif < RefRect.position.y)
                            dif += singleSize_half;
                        else
                            dif -= singleSize_half;
                    else if (cpa == ChildPivoAling.C)
                        if (dif > RefRect.position.y)
                            dif -= singleSize;
                    //FastDebug.Point(new Vector2(dif, rc.position.y), 20f, Color.magenta);
                    dif = RefRect.position.y - dif;
                    rc.gameObject.SetActive(Mathf.Abs(dif) <= half_ref);
                }
            }
            else
            {
                for (int i = 0; i < activeChildren; i++)
                {
                    dif = filhos[i].rc.position.y;
                    if (cpa == ChildPivoAling.A)
                    {
                        if (dif < RefRect.position.y)
                            dif += singleSize;
                    }
                    else if (cpa == ChildPivoAling.B)
                        if (dif < RefRect.position.y)
                            dif += singleSize_half;
                        else
                            dif -= singleSize_half;
                    else if (cpa == ChildPivoAling.C)
                        if (dif > RefRect.position.y)
                            dif -= singleSize;
                    dif = RefRect.position.y - dif;
                    filhos[i].Fit(Mathf.Abs(dif) <= half_ref);
                }
            }
        }
        else if (ft == fitType.somenteDentro)
        {
        }
        else if (ft == fitType.fadeParaOsDefora)
        {

        }
    }
    #endregion
    #endregion

    public override void FixarFilhos()
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
        float _relaçãoy_space = (spaceAmount * RefRect.rect.height) / myrect.rect.height;
        float _relaçãoy_Borda = (BorderAmoun * RefRect.rect.height) / myrect.rect.height;
        float B_x = 0;
        if (at == AlingType.B)
        {
            B_x = activeChildren * horizontalAmount * RefRect.rect.width / 2 + (RefRect.rect.width * BorderAmoun)
                + Mathf.Clamp(activeChildren - 1, 0, activeChildren) * spaceAmount * RefRect.rect.width / 2;
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
                if (at == AlingType.A)
                {
                    rc.anchorMin = new Vector2(.5f - (_relaçãox / 2), (_relaçãoy + _relaçãoy_space) * i + _relaçãoy_Borda);
                    rc.anchorMax = new Vector2(.5f + (_relaçãox / 2), _relaçãoy * (i + 1) + (_relaçãoy_space * i) + _relaçãoy_Borda);
                }
                else if (at == AlingType.B)
                {

                }
                else if (at == AlingType.C)
                {
                    rc.anchorMin = new Vector2(.5f - (_relaçãox / 2), 1 - (_relaçãoy * (i + 1) + (_relaçãoy_space * i)) - _relaçãoy_Borda);
                    rc.anchorMax = new Vector2(.5f + (_relaçãox / 2), 1 - ((_relaçãoy + _relaçãoy_space) * i) - _relaçãoy_Borda);
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
        if (RefRect == null)
            return;
        if (bound != null)
            FastDebug.DesenharBoundMinMax(bound, Color.green);
        #region Próprio tamanho
        x_size = myrect.rect.width / 2f;
        Debug.DrawLine(myrect.position - Vector3.right * x_size, myrect.position + Vector3.right * x_size, Color.green);
        Vector3 UpPos = myrect.transform.position;
        Vector3 DownPos = myrect.transform.position;
        if (at == AlingType.A)
        {
            DownPos += new Vector3(0, myrect.rect.height, 0);
        }
        else if (at == AlingType.B)
        {
            UpPos -= new Vector3(0, myrect.rect.height / 2, 0);
            DownPos += new Vector3(0, myrect.rect.height / 2, 0);
        }
        else
        {
            UpPos -= new Vector3(0, myrect.rect.height / 2, 0);
        }
        Debug.DrawRay(UpPos, new Vector3(0, RefRect.rect.height * BorderAmoun, 0), Color.red);
        Debug.DrawRay(DownPos, new Vector3(0, -RefRect.rect.height * BorderAmoun, 0), Color.red);
        #endregion
        #region Centro dos filhos
        float B_x = 0;
        if (at == AlingType.B)
        {
            B_x = activeChildren * verticalAmount * RefRect.rect.height / 2 + (RefRect.rect.height * BorderAmoun)
                + Mathf.Clamp(activeChildren - 1, 0, activeChildren) * spaceAmount * RefRect.rect.height / 2;
        }
        for (int i = 0; i < activeChildren; i++)
        {
            #region position

            float y = (i * (spaceAmount + verticalAmount)) * RefRect.rect.height + (RefRect.rect.height * BorderAmoun);
            Vector2 ppp = Vector2.zero;
            if (at == AlingType.A)
                ppp = new Vector2(0, y) + (Vector2)myrect.position;
            else if (at == AlingType.B)
                ppp = new Vector2(0, y - B_x) + (Vector2)myrect.position;
            else if (at == AlingType.C)
                ppp = new Vector2(0, -y) + (Vector2)myrect.position;
            #endregion
            //Debug.DrawRay(ppp-Vector2.up*y_size*((float)(i)/activeChildren), Vector3.right * verticalAmount * RefRect.rect.height, Color.green);
            //Debug.DrawRay(ppp-Vector2.up*y_size*((float)(i+1)/(activeChildren +1))+ Vector2.right * verticalAmount * RefRect.rect.height,
            //    Vector3.right * spaceAmount * RefRect.rect.height, Color.magenta);
            Debug.DrawRay(ppp, Vector3.up * verticalAmount * RefRect.rect.height, Color.green);
            Debug.DrawRay(ppp + Vector2.up * verticalAmount * RefRect.rect.height,
                Vector3.up * spaceAmount * RefRect.rect.height, Color.magenta);

            if (at == AlingType.B)
                ppp += (cpa == ChildPivoAling.B ? new Vector2(0, (verticalAmount) / 2 * RefRect.rect.height) : Vector2.zero);

            FastDebug.Point(ppp, 5f, Color.magenta);

        }
        #endregion
    }
}
