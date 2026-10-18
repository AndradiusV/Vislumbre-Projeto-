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
        todos_Avisos = "";
        Esconder_Aviso();
    }

    TimerToken tempoParaFechar;
    [SerializeField] TextMeshProUGUI _aviso;
    string todos_Avisos;
    public static bool avisos = true;
    public void Mostrar_Aviso(string mensagem,float tempo = 10)
    {
        if (!avisos)
            return;

        if(tempoParaFechar != null)
            tempoParaFechar.Dispose();

        obj_Aviso.SetActive(true);
        todos_Avisos += "\n" + mensagem;
        _aviso.text = mensagem;
        Debug.Log(mensagem);
        tempoParaFechar = Timer.AdicionarToken(tempo, null,
            Esconder_Aviso,false);
    }
    public void Adicionar_Ao_Log(string mensagem)
    {
        todos_Avisos += "\n" + mensagem;
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
        CriarArquivo(todos_Avisos, "logs");
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
}
