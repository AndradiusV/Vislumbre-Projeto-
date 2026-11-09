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

        _To_Camera_Size.y = Camera.main.orthographicSize * 2f;
        _To_Camera_Size.x = _To_Camera_Size.y * Camera.main.aspect;
        Canvas_Ajust_Size = new Vector2(_To_Camera_Size.x/canvas_Geral.pixelRect.width, _To_Camera_Size.y / canvas_Geral.pixelRect.height);
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
    public static Vector2 Canvas_Ajust_Size { get { return _Canvas_Ajust_size;}  private set { _Canvas_Ajust_size = value; } }
    static Vector2 _Canvas_Ajust_size;
    public static Vector2 To_Camera_Size { get { return _To_Camera_Size; }  private set { _To_Camera_Size = value; } }
    static Vector2 _To_Camera_Size;


    public Canvas canvas_Geral;
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

                _To_Camera_Size.y = Camera.main.orthographicSize * 2f;
                _To_Camera_Size.x = _To_Camera_Size.y * Camera.main.aspect;
                Canvas_Ajust_Size = new Vector2(_To_Camera_Size.x / canvas_Geral.pixelRect.width, _To_Camera_Size.y / canvas_Geral.pixelRect.height);
                OnResize(lastWidth, lastHeight);
            }
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            _mousePositionOnWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            _CameraCenterPositionOnWorld = Camera.main.transform.position + Camera.main.transform.forward *
                Mathf.Abs(Camera.main.transform.position.z) / Camera.main.transform.forward.z;


            _mousePorcentagemInCamera.x = Mathf.Clamp01(Input.mousePosition.x / Camera.main.pixelWidth);
            _mousePorcentagemInCamera.y = Mathf.Clamp01(Input.mousePosition.y / Camera.main.pixelHeight);
            _mouseOverUI = isMouseOverUI();
            _isOverArea = isMouseOverArea(mousePositionOnWorld);
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
    public GameObject GetHoveredUIObject()
    {
        // 2. Set up the Pointer Event Data at the current mouse position
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // 3. Create a list to store all raycast hits
        List<RaycastResult> results = new List<RaycastResult>();

        // 4. Raycast using the current EventSystem
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // 5. The first item in the list is always the topmost UI element
        if (results.Count > 0)
        {
            return results[0].gameObject;
        }

        return null;
    }
    bool isMouseOverArea(Vector2 pos)
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
            {
                //Caso com o mouse sobre a região a entrega para o ADM de regiões
                if (regiões[i].bound.Contains(pos))
                {
                    if(regiões[i].titulo == null || regiões[i].titulo == "")
                    {
                        ret = true;
                        continue;
                    }
                    //Aqui o ADM deve ter um código para ver quanto tempo ele ficou com o mouse sobre essa região,
                    //se ficar mais de 1 segundo, ele deve mostrar a explicação da região
                    //Antes precisa verificar se realmente está com o mouse sobre a região.
                    if (ismouseOverUI)
                    {
                        GameObject ob = GetHoveredUIObject();
                        if (ob != null && ob.TryGetComponent(out RegiãoClicavel r) && r == regiões[i])
                            Regiões_ADM.Mouse_Over_Area(regiões[i]);
                        else if(ob != null)
                        {
                            RegiãoClicavel r2 = ob.GetComponentInParent<RegiãoClicavel>(false);
                            if(r2 != null && r2 == regiões[i])
                                Regiões_ADM.Mouse_Over_Area(regiões[i]);
                        }
                    }
                    return true;
                }
            }
        }
        Regiões_ADM.Mouse_Over_Area(null);
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
