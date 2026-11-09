using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Selecionador_De_Cor : MonoBehaviour
{
    public static Color cor = Color.red;

    public static float intencidade_Branco = 1;
    public static float intencidade_Preto = 1;
    public static float opacidade = 1;
    [SerializeField] RectTransform _C_I_Pos_RT;
    [SerializeField] RectTransform _color_Intensity_RT;
    [SerializeField] Slider _color_Base_SL;
    [SerializeField] Slider _opacidade_SL;
    [SerializeField] Image _OP_handler_IMG;
    [SerializeField] Image _C_I_IMG;
    [SerializeField] Image _brush_Color_IMG;
    bool mudando_Intencidade = false;
    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            float x = FunçõesPrecalculadas.mousePositionOnWorld.x - _color_Intensity_RT.position.x;
            float y = FunçõesPrecalculadas.mousePositionOnWorld.y - _color_Intensity_RT.position.y;

            float x_Amount = x / (_color_Intensity_RT.rect.width * FunçõesPrecalculadas.Canvas_Ajust_Size.x);
            float y_Amount = y / (_color_Intensity_RT.rect.height * FunçõesPrecalculadas.Canvas_Ajust_Size.y);

            if ((Mathf.Abs(x_Amount) <= .5f && Mathf.Abs(y_Amount) <= .5f) || mudando_Intencidade)
            {
                mudando_Intencidade = true;
                Escolher_Intencidade(x_Amount, y_Amount);
            }
        }
        if (mudando_Intencidade && Input.GetKeyUp(KeyCode.Mouse0))
        {
            mudando_Intencidade = false;
            Opcoes.SalvarConfiguracoes();
        }
    }
    public void Escolher_Cor(float valor)
    {
        valor = Mathf.Clamp01(valor);

        // Hue: vermelho -> verde -> azul -> vermelho
        float hue = valor * 3f / 3f;

        cor = Color.HSVToRGB(hue, 1f, 1f);

        Calcular_Cor_Final();

        Opcoes.SalvarConfiguracoes();
    }
    public void Escolher_Opacidade(float valor)
    {
        opacidade = valor;

        Calcular_Cor_Final();


        Opcoes.SalvarConfiguracoes();
    }
    public void Escolher_Intencidade(float x_Amount,float y_Amount)
    {
        x_Amount += .5f;
        y_Amount += .5f;

        intencidade_Branco = Mathf.Clamp01(x_Amount);
        intencidade_Preto = Mathf.Clamp01(y_Amount);

        _C_I_Pos_RT.position = new Vector3((intencidade_Branco - .5f) * _color_Intensity_RT.rect.width,
           (intencidade_Preto - .5f) * _color_Intensity_RT.rect.height, 0);
        _C_I_Pos_RT.position *= FunçõesPrecalculadas.Canvas_Ajust_Size;

        _C_I_Pos_RT.position += _color_Intensity_RT.position;

        intencidade_Branco = 1 - Mathf.Clamp01(x_Amount);

        Calcular_Cor_Final();
    }

    void Calcular_Cor_Final()
    {
        float r = cor.r + intencidade_Branco;
        float g = cor.g + intencidade_Branco;
        float b = cor.b + intencidade_Branco;
        r = Mathf.Clamp01(r);
        g = Mathf.Clamp01(g);
        b = Mathf.Clamp01(b);
        Color cor_Final = new Color(r, g, b);

        cor_Final *= intencidade_Preto;
        cor_Final.a = opacidade;

        Desenho_Na_Tela.brushColor = cor_Final;
        _brush_Color_IMG.color = cor_Final;
        _OP_handler_IMG.color = cor;
        _C_I_IMG.color = cor;
    }
    /// <summary>
    /// Seta o alpha, a cor e a intencidade sem notificar, só para deixar mais claro que tudo foi salvo.
    /// </summary>
    internal void Carregar_Info()
    {
        Calcular_Cor_Final();

        float x = (1 - intencidade_Branco);

        _C_I_Pos_RT.position = new Vector3((intencidade_Branco - .5f) * _color_Intensity_RT.rect.width,
           (intencidade_Preto - .5f) * _color_Intensity_RT.rect.height, 0);
        _C_I_Pos_RT.position *= FunçõesPrecalculadas.Canvas_Ajust_Size;

        _C_I_Pos_RT.position += _color_Intensity_RT.position;

        _opacidade_SL.SetValueWithoutNotify(opacidade);

        Color.RGBToHSV(cor, out float color_valor, out float s, out float v);

        _color_Base_SL.SetValueWithoutNotify(color_valor);
    }
    internal void Carregar_Cor(Color novaCor)
    {
        // Recupera a opacidade
        Color corAjustada = novaCor.gamma; // Ou novaCor.linear dependendo do Color Space do projeto

        //opacidade = corAjustada.a;
        opacidade = corAjustada.a;

        // Recupera a cor HSV
        Color.RGBToHSV(corAjustada, out float h, out float s, out float v);

        // Recupera a cor base, mantendo saturação e matiz
        cor = Color.HSVToRGB(h, s, 1f);

        // Recupera as intensidades
        intencidade_Preto = v;
        intencidade_Branco = 0;

        // Atualiza os parâmetros visuais
        Carregar_Info();
    }
}