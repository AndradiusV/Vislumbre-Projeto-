using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using Debug = UnityEngine.Debug;
using TMPro;

public class Updater_Call : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI version_Aviso;
    public float velocidade = 0.2f;

    private float hue;
    [SerializeField]
    Button bt_OB;
    public static bool Desatualizado;
    private void Update()
    {
        if (Desatualizado)
        {
            hue += Time.deltaTime * velocidade;
            hue %= 1f;

            version_Aviso.color = Color.HSVToRGB(hue, 1f, 1f);
            version_Aviso.text = "V" + Opcoes.config_Atual.Versão_Da_Build + " - Atualização disponível! Clique aqui para atualizar.";
            bt_OB.interactable = true;
        }
        else if(Opcoes.config_Atual != null)
        {
            bt_OB.interactable = false;
            version_Aviso.color = Color.green;
            version_Aviso.text = "V" + Opcoes.config_Atual.Versão_Da_Build;
        }
    }
    private const string versionUrl = "https://raw.githubusercontent.com/AndradiusV/Vislumbre/main/Vislumbre_Compilado/Versao.txt";

    public static IEnumerator VerificarVersao()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(versionUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao consultar versão: " + request.error);
                yield break;
            }

            string versaoRemota = request.downloadHandler.text.Trim();

            Debug.Log("Versão local: " + Opcoes.config_Atual.Versão_Da_Build);
            Debug.Log("Versão remota: " + versaoRemota);

            Desatualizado = versaoRemota != Opcoes.config_Atual.Versão_Da_Build;
            if (versaoRemota != Opcoes.config_Atual.Versão_Da_Build)
            {
                Debug.Log("Existe uma versão diferente disponível!");
            }
            else
            {
                Debug.Log("O programa já está atualizado.");
            }
        }
    }
    public void Atualizar()
    {
        string pasta = Path.GetDirectoryName(Application.dataPath);
        string caminho = Path.Combine(pasta,"Updater.exe");
        Avisos.instance.Mostrar_Aviso( caminho );
        int pid = Process.GetCurrentProcess().Id;

        Process.Start(new ProcessStartInfo
        {
            FileName = caminho,
            UseShellExecute = true,
            Arguments = pid.ToString()
        });

        Application.Quit();
    }
}
