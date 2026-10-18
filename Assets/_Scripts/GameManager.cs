using GameServer;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEditor;
using UnityEngine;
using uWindowCapture;

public class GameManager : MonoBehaviour
{
    public static bool Pause;
    private void Awake()
    {
        //Opcoes.SalvarConfiguracoes();
        UwcManager.instance.name = "uWindowCapture";
        Application.targetFrameRate = 60;
        Opcoes.CarregarConfiguracoes();
    }
    private void Start()
    {
        StartCoroutine(WebRTC.Update());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Avisos.instance.Salvar_Log();
        }
        //Avisos.instance.Mostrar_Aviso_Fixo(Input.mousePosition.ToString());
    }
}
