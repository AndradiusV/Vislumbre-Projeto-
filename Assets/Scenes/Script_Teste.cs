using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class Script_Teste : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProComponent;
    private void Update()
    {
        string minhaFrase = textMeshProComponent.text;
        float tamanhoDaFonte = textMeshProComponent.fontSize;

        Vector2 tamanhoDoTexto = CalcularTamanhoDoTexto(minhaFrase, tamanhoDaFonte);

        Debug.Log($"Largura: {tamanhoDoTexto.x} | Altura: {tamanhoDoTexto.y}");
    }
    /// <summary>
    /// Calcula o tamanho (largura e altura) que um texto ocupará com base no tamanho da fonte.
    /// </summary>
    public Vector2 CalcularTamanhoDoTexto(string texto, float fontLength)
    {
        if (textMeshProComponent == null)
        {
            Debug.LogError("Componente TextMeshProUGUI não foi atribuído.");
            return Vector2.zero;
        }

        // Altera temporariamente o tamanho da fonte para o cálculo correto
        float tamanhoOriginal = textMeshProComponent.fontSize;
        textMeshProComponent.fontSize = fontLength;

        // Retorna a largura (x) e altura (y) preferidas para o texto informado
        Vector2 tamanhoCalculado = textMeshProComponent.GetPreferredValues(texto);

        // Restaura o tamanho original do componente (opcional)
        textMeshProComponent.fontSize = tamanhoOriginal;

        return tamanhoCalculado;
    }
}
