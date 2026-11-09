using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Regiões_ADM : MonoBehaviour
{
    public static bool descrições_Discretas = true;
    static float time_Over_Região;
    static float time_Over_Região_Max = 1.5f;
    static RegiãoClicavel last_R;
    public static void Mouse_Over_Area(RegiãoClicavel r)
    {
        if (!descrições_Discretas)
            return;

        if (r == null)
        {
            time_Over_Região = time_Over_Região_Max * (last_R != null ? .35f : 1f);
            last_R = null;
            return;
        }
        if (last_R != r)
        {
            time_Over_Região = time_Over_Região_Max * (last_R != null ? .35f : 1f);
            last_R = r;
        }
        else if(time_Over_Região > 0)
        {
            time_Over_Região -= Time.deltaTime * r.mod_Speed_Aparecer;
            if (time_Over_Região <= 0)
            {
                titulo = r.titulo;
                descrição = r.descrição;
            }
        }
        last_R = r;
    }
    static string titulo;
    static string descrição;
    static bool Ativou_Descrição;
    public RectTransform descrição_RT;
    public TextMeshProUGUI titulo_txt;
    public TextMeshProUGUI descrição_txt;
    Vector2 tamano_Total_Alvo = Vector2.zero;
    private void Update()
    {
        if(!descrições_Discretas){
            descrição_RT.gameObject.SetActive(false);
            return;
        }

        //Aqui ele ativa
        if(time_Over_Região <= 0 && !Ativou_Descrição)
        {
            Ativou_Descrição = true;
            titulo_txt.text = titulo;
            descrição_txt.text = descrição;
            Vector2 tamanho_Descrição = CalcularTamanhoDoTexto(descrição, 15);
            tamano_Total_Alvo = tamanho_Descrição / new Vector2(0.88f, 0.49f);
            tamano_Total_Alvo.y = Mathf.Clamp(tamano_Total_Alvo.y, 100, float.PositiveInfinity);
            descrição_RT.sizeDelta = tamano_Total_Alvo;
            descrição_RT.gameObject.SetActive(true);
        }
        //Aqui vai desativar a parte visual da descrição
        else if (time_Over_Região > 0 && Ativou_Descrição)
        {
            Ativou_Descrição = false;
            titulo_txt.text = "--";
            descrição_txt.text = "--";
            descrição_RT.gameObject.SetActive(false);
        }

        if (Ativou_Descrição) { 
            if(Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) ||
                Input.GetKeyDown(KeyCode.Mouse2))
            {
                time_Over_Região = time_Over_Região_Max * (last_R != null ? .35f : 1f);
                last_R = null;
                Ativou_Descrição = false;
                titulo_txt.text = "--";
                descrição_txt.text = "--";
                descrição_RT.gameObject.SetActive(false);
                return;
            }
            Posicionar_Descrição();
        }
    }

    void Posicionar_Descrição()
    {
        Vector2 m_pos = Input.mousePosition;
        //Precisamos ver onde a descrição vai aparecer, se vai para a esquerda ou direita do mouse, acima ou abaixo.
        #region Varifica se deve estar acima ou abaixo do mouse
        if (m_pos.y - tamano_Total_Alvo.y < 0)
        {
            descrição_RT.pivot = new Vector2(descrição_RT.pivot.x, 0);
        }
        else
        {
            descrição_RT.pivot = new Vector2(descrição_RT.pivot.x, 1);
        }
        #endregion
        #region Varifica se deve estar esquerda ou direita do mouse
        if (m_pos.x - tamano_Total_Alvo.x < 0)
        {
            descrição_RT.pivot = new Vector2(0, descrição_RT.pivot.y);
        }
        else
        {
            descrição_RT.pivot = new Vector2(1, descrição_RT.pivot.y);
        }
        #endregion

        //Posiciona
        descrição_RT.position = FunçõesPrecalculadas.mousePositionOnWorld;
    }
    /// <summary>
    /// Calcula o tamanho (largura e altura) que um texto ocupará com base no tamanho da fonte.
    /// </summary>
    public Vector2 CalcularTamanhoDoTexto(string texto, float fontLength)
    {
        if (descrição_txt == null)
        {
            Debug.LogError("Componente TextMeshProUGUI não foi atribuído.");
            return Vector2.zero;
        }

        // Altera temporariamente o tamanho da fonte para o cálculo correto
        float tamanhoOriginal = descrição_txt.fontSize;
        descrição_txt.fontSize = fontLength;

        //Ajeita o texto para o processo sobrepor o padrão de string -> TMP.text
        texto = texto.Replace("\\n", "\n");
        descrição_txt.text = texto;

        // Retorna a largura (x) e altura (y) preferidas para o texto informado
        Vector2 tamanhoCalculado = descrição_txt.GetPreferredValues(texto);

        // Restaura o tamanho original do componente (opcional)
        descrição_txt.fontSize = tamanhoOriginal;

        return tamanhoCalculado;
    }
}
