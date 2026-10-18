#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CA_Vertical))]
public class CA_Vertical_Editor : Editor
{
    SerializedProperty canvasType;
    public override void OnInspectorGUI()
    {
        CA_Vertical a = (CA_Vertical)target;
        a.RefRect = (RectTransform)EditorGUILayout.ObjectField("Rect Parametro", (a.RefRect == null ? null : a.RefRect), typeof(RectTransform));

        if (a.RefRect == null)
            return;

        a.item = (CA_item)EditorGUILayout.ObjectField("Objeto para preencher", (a.item == null ? null : a.item), typeof(CA_item));

        a.scrool = EditorGUILayout.ToggleLeft("pode scroolar", a.scrool);
        a.LimitScrool = EditorGUILayout.ToggleLeft("Limitar o scrool", a.LimitScrool);
        a.scroolPos = EditorGUILayout.Slider("Scrool pos", a.scroolPos, 0, 1f);
        a.scroolSpeed = EditorGUILayout.FloatField("Scrool sensitv", a.scroolSpeed);

        #region definição de alteração
        a.UseNormalChild = EditorGUILayout.ToggleLeft("usar Normal Child?", a.UseNormalChild);

        // Ofcourse you also want to change the list size here
        //a.ignorarDesativados = EditorGUILayout.ToggleLeft("Não ignorar os desativados", a.ignorarDesativados);

        a.cpa = (ChildPivoAling)EditorGUILayout.EnumPopup("ChildPivo", a.cpa);
        a.at = (AlingType)EditorGUILayout.EnumPopup("Alinhamento", a.at);


        EditorGUILayout.Space(10);
        a.quadriangular = EditorGUILayout.ToggleLeft("Squared", a.quadriangular);
        if(a.quadriangular)
            a.travarDentro = EditorGUILayout.ToggleLeft("travar dentro", a.travarDentro);

        a.porcentagem = EditorGUILayout.ToggleLeft("Espaçamento Por %", a.porcentagem);
        #endregion
        #region amounts e sizes
        EditorGUILayout.Space(10);
        GUILayout.Label("Parametros Verticais");

        a.fullVertical = EditorGUILayout.ToggleLeft("Tamanho Completo", a.fullVertical);

        if (!a.fullVertical)
        {
            if (a.porcentagem)
            {
                a.verticalAmount = EditorGUILayout.Slider("Tamanho %", a.verticalAmount, 0,  a.quadriangular && a.travarDentro ? a.RefRect.rect.width / a.RefRect.rect.height : 1f);
            }
            else
            {
                a.verticalSize = EditorGUILayout.FloatField("Tamanho", a.verticalSize);
            }
        }
        else
        {
            a.verticalAmount = 1f;
        }
        if (!a.quadriangular)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Parametros Horizontais");

            a.fullHorizontal = EditorGUILayout.ToggleLeft("Tamanho Completo", a.fullHorizontal);
            if (!a.fullHorizontal)
            {
                if (a.porcentagem)
                {
                    a.horizontalAmount = EditorGUILayout.Slider("Tamanho %", a.horizontalAmount, 0, 1f);
                }
                else
                {
                    a.horizontalSize = EditorGUILayout.FloatField("TamanhoUni", a.horizontalSize);
                }
            }
            else
            {
                a.horizontalAmount = 1f;
            }
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Parametros Spacing");
        if (a.porcentagem)
        {
            a.spaceAmount = EditorGUILayout.Slider("Tamanho %", a.spaceAmount, 0, 1f);
        }
        else
        {
            a.spaceSize = EditorGUILayout.FloatField("Tamanho", a.spaceSize);
        }
        EditorGUILayout.Space(5);
        if (a.porcentagem)
        {
            a.BorderAmoun = EditorGUILayout.Slider("Bordas Laterais %", a.BorderAmoun, 0, 1f);
        }
        else
        {
            a.BorderSize = EditorGUILayout.FloatField("Bordas Laterais", a.BorderSize);
        }
        #endregion

        #region fit Type
        a.ft = (fitType)EditorGUILayout.EnumPopup("Fit type", a.ft);
        #endregion

        //Ajusta Os objetos e atualiza o tamanho do Content
        if (a.RefRect.rect.width == 0f || a.activeChildren == 0)
        {
            a.ResetarTamanho();
            return;
        }
        a.DefinirPropriaPos();
        a.DefinirProprioTamanho();
        a.DefinirFilhos();
        a.FitFilhos();


        if (GUILayout.Button("Fixar Filhos"))
        {
            a.FixarFilhos();
        }
    }
}
#endif