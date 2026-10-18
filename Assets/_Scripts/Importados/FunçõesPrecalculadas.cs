using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FunçõesPrecalculadas : MonoBehaviour
{
    public static FunçõesPrecalculadas instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }
    public static Vector2 mousePositionOnWorld { get { return _mousePositionOnWorld; } private set { _mousePositionOnWorld = value; } }
    static Vector2 _mousePositionOnWorld;
    public static Vector2 cameraCenterPositionOnWorld { get { return _CameraCenterPositionOnWorld; } private set { _CameraCenterPositionOnWorld = value; } }
    static Vector2 _CameraCenterPositionOnWorld;
    public static Vector2 mousePorcentagemInCamera { get { return _mousePorcentagemInCamera; } private set { _mousePorcentagemInCamera = value; } }
    static Vector2 _mousePorcentagemInCamera;
    public static bool ismouseOverUI { get { return _mouseOverUI; } private set { _mouseOverUI = value; } }
    static bool _mouseOverUI;
    public static bool isOverArea { get { return _isOverArea; } private set { _isOverArea = value; } }
    static bool _isOverArea;
    public static List<RegiãoClicavel> regiões =new();


    private int lastWidth;
    private int lastHeight;
    //Aqui ele irá calcular todas as coisas que poderão ser pedidas por mais de 1 objeto ou script
    void Update()
    {
        try
        {

            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;

                OnResize(lastWidth, lastHeight);
            }

            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            _mousePositionOnWorld = r.origin + r.direction *
                Mathf.Abs(r.origin.z) / r.direction.z;

            _CameraCenterPositionOnWorld = Camera.main.transform.position + Camera.main.transform.forward *
                Mathf.Abs(Camera.main.transform.position.z) / Camera.main.transform.forward.z;


            _mousePorcentagemInCamera.x = Mathf.Clamp01(Input.mousePosition.x / Camera.main.pixelWidth);
            _mousePorcentagemInCamera.y = Mathf.Clamp01(Input.mousePosition.y / Camera.main.pixelHeight);
            _mouseOverUI = isMouseOverUI();
            _isOverArea = isMouseOverArea();
            //if (Input.GetKeyDown(KeyCode.Equals))
            //    PlayerDebuger.instance.LogsList();
        }
        catch (System.Exception ex)
        {
            Avisos.instance.Mostrar_Aviso(ex.ToString(), 10);
        }
    }
    bool isMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    bool isMouseOverArea()
    {
        bool ret = false;
        for (int i = 0; i < regiões.Count; i++)
        {
            if (regiões[i] == null)
            {
                regiões.RemoveAt(i);
                i--;
                continue;
            }
            if (regiões[i].Ativado())
                ret |= regiões[i].bound.Contains((Vector2)Input.mousePosition);
        }
        return ret;
    }
    public static void Atualizar_Areas()
    {
        Timer.AdicionarToken(0.01f, () => { },() => {
            for (int i = 0; i < regiões.Count; i++)
            {
                if (regiões[i] == null)
                {
                    regiões.RemoveAt(i);
                    i--;
                    continue;
                }
                regiões[i].Atualizar();
            }
        },false);
        Timer.AdicionarToken(0.02f, () => { },() => {
            for (int i = 0; i < regiões.Count; i++)
            {
                if (regiões[i] == null)
                {
                    regiões.RemoveAt(i);
                    i--;
                    continue;
                }
                regiões[i].Atualizar();
            }
        },false);
    }
    public static void Adicionar_Area(RegiãoClicavel r)
    {
        if(!regiões.Contains(r))
            regiões.Add(r);
    }

    void OnResize(int width, int height)
    {
        Atualizar_Areas();
    }
}
