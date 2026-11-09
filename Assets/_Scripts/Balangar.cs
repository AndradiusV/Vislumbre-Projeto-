using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balangar : MonoBehaviour
{
    [SerializeField] Transform st_Pos;
    [SerializeField] Transform balangar_T;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float _altura_Da_Balangada;
    public float _speed_Da_Balangada;
    // Update is called once per frame
    void Update()
    {
        balangar_T.position = st_Pos.position + new Vector3(0, -Mathf.Abs(Mathf.Sin(Time.time * _speed_Da_Balangada)) * _altura_Da_Balangada, 0);
    }
}
