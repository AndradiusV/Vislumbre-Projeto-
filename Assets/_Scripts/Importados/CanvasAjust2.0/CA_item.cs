using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_item : MonoBehaviour
{
    public RectTransform rc;
    CA_Content dono;
    public virtual void Set(CA_Content d)
    {
        rc = GetComponent<RectTransform>();
        dono = d;
    }
    internal void Fit(bool v)
    {
        gameObject.SetActive(v);
    }
}
