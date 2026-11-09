using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Avisos : MonoBehaviour
{
    public GameObject obj_Aviso;
    public static Avisos instance;
    private void Awake()
    {
        instance = this;
        _todos_Avisos = "";
        _feedBack_Duration_Base = feedBack_Duration;
    }
    #region Avisos
    TimerToken _token_Aviso;
    [SerializeField] TextMeshProUGUI _aviso;
    string _todos_Avisos;
    public static bool avisos = true;
    public void Mostrar_Aviso(string mensagem, float tempo = 10,bool obrigatório = false)
    {
        if (!avisos && !obrigatório)
            return;

        if (_token_Aviso != null)
            _token_Aviso.Dispose();

        obj_Aviso.SetActive(true);
        _todos_Avisos += "\n" + mensagem;
        _aviso.text = mensagem;
        Debug.Log(mensagem);
        _token_Aviso = Timer.AdicionarToken(tempo, null,
            Esconder_Aviso, false);
    }
    public void Adicionar_Ao_Log(string mensagem)
    {
        _todos_Avisos += "\n" + mensagem;
    }
    public void Mostrar_Aviso_Fixo(string mensagem)
    {
        obj_Aviso.SetActive(true);
        //todos_Avisos += "\n" + mensagem;
        _aviso.text = mensagem;
        Debug.Log(mensagem);
    }
    public void Salvar_Log()
    {
        CriarArquivo(_todos_Avisos, "logs");
    }
    public void Esconder_Aviso()
    {
        obj_Aviso.SetActive(false);
    }
    public void CriarArquivo(string texto, string nomeArquivo)
    {
        string diretorio = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        string caminho = Path.Combine(diretorio, nomeArquivo + ".txt");

        File.WriteAllText(caminho, texto);

        Debug.Log("Arquivo criado em: " + caminho);
    }
    #endregion
    #region feedBack
    TimerToken _token_FeedBack;
    public float feedBack_Duration;
    float _feedBack_Duration_Base;
    public float feedBack_Times_Chance_Color;
    public Color feedBack_Color_A;
    public Color feedBack_Color_B;
    [SerializeField] TextMeshProUGUI _feedBack_Text;
    [SerializeField] RectTransform _feedBack_RectTransform;
    public void Dar_Feed_Back(string texto)
    {
        if (_token_FeedBack != null)
            _token_FeedBack.Dispose();

        _feedBack_Text.text = texto;
        _feedBack_RectTransform.gameObject.SetActive(true);

        feedBack_Duration = _feedBack_Duration_Base + Mathf.Clamp((texto.Length / 36f), 0, 3f);

        _token_FeedBack = Timer.AdicionarToken(feedBack_Duration, () =>
        {

        }, t =>
        {
            float c_t = t / feedBack_Duration;
            c_t *= feedBack_Times_Chance_Color * Mathf.PI;
            _feedBack_Text.color = Color.Lerp(feedBack_Color_A,feedBack_Color_B, Mathf.Abs(Mathf.Sin(c_t)));
        }, () =>
        {
            _feedBack_RectTransform.gameObject.SetActive(false);
        }
        , true);
    }
    #endregion
}
