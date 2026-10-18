using System.Collections.Generic;
using UnityEngine;

namespace VolatiusDataBase.FastDebug
{
    public static class FastDebug
    {
        public static TextMesh CriarTextoNoMundo(string text, Transform parent = null, Vector3 localposition = default(Vector3), int fontSize = 40, Color cor = default(Color), TextAnchor textAnchor = TextAnchor.LowerLeft, int sortingOrder = 0)
        {
            if (cor == null) cor = Color.white;
            return CriarTextoNoMundo(parent, text, localposition, fontSize, cor, textAnchor, sortingOrder);
        }
        public static TextMesh CriarTextoNoMundo(Transform parent, string text, Vector3 localPosition, int fontSize, Color cor, TextAnchor textAnchor, int sortingOrder)
        {
            GameObject go = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = go.transform;
            transform.localScale = new Vector3(.1f, .1f, .1f);
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = go.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = TextAlignment.Center;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = cor;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;

        }
        static int gapQuant = 5;
        /// <summary>
        /// Desenha uma linha, mas pontilhada
        /// </summary>
        public static void DesenharLinhaPontilhada(Vector3 a, Vector3 b, Color cor)
        {
            Vector3 ab = b - a;
            float tamanho = ab.magnitude;
            float quantModf = 1;
            if (tamanho > gapQuant / 2)
                quantModf = tamanho / (gapQuant / 2);
            ab.Normalize();
            float relacao = (tamanho / (gapQuant * quantModf));
            for (int i = 0; i < gapQuant * quantModf; i++)
            {
                float d1 = 2 * (relacao * i);
                float d2 = (relacao * (i + 1)) + (relacao * i);
                Debug.DrawLine(a + (ab * Mathf.Clamp(d1, 0, tamanho)), a + (ab * Mathf.Clamp(d2, 0, tamanho)), cor);
                if (d1 >= tamanho || d2 >= tamanho)
                    return;
            }
        }
        public static void DesenharHexagono(Vector2 centro, float ysize, float xsize, float xtopFactor)
        {
            Vector2 p0 = centro + (Vector2.right * xsize);//0
            Vector2 p1 = centro + (Vector2.up * ysize) + (Vector2.right * xsize * xtopFactor);//1
            Debug.DrawLine(p0, p1);
            p0 = centro + (Vector2.up * ysize) + (Vector2.right * -xsize * xtopFactor);//2
            Debug.DrawLine(p0, p1);
            p1 = centro + (Vector2.right * -xsize);//3
            Debug.DrawLine(p0, p1);
            p0 = centro + (Vector2.down * ysize) + (Vector2.right * -xsize * xtopFactor);//4
            Debug.DrawLine(p0, p1);
            p1 = centro + (Vector2.down * ysize) + (Vector2.right * xsize * xtopFactor);//5
            Debug.DrawLine(p0, p1);
            p0 = centro + (Vector2.right * xsize);//0
            Debug.DrawLine(p0, p1);
        }
        public static void DesenharHexagono(Vector2 centro, float ysize, float xsize, float xtopFactor, Color cor)
        {
            Vector2 p0 = centro + (Vector2.right * xsize);//0
            Vector2 p1 = centro + (Vector2.up * ysize) + (Vector2.right * xsize * xtopFactor);//1
            Debug.DrawLine(p0, p1, cor);
            p0 = centro + (Vector2.up * ysize) + (Vector2.right * -xsize * xtopFactor);//2
            Debug.DrawLine(p0, p1, cor);
            p1 = centro + (Vector2.right * -xsize);//3
            Debug.DrawLine(p0, p1, cor);
            p0 = centro + (Vector2.down * ysize) + (Vector2.right * -xsize * xtopFactor);//4
            Debug.DrawLine(p0, p1, cor);
            p1 = centro + (Vector2.down * ysize) + (Vector2.right * xsize * xtopFactor);//5
            Debug.DrawLine(p0, p1, cor);
            p0 = centro + (Vector2.right * xsize);//0
            Debug.DrawLine(p0, p1, cor);
        }
        public static void DesenharHexagono(Vector2 centro, float ysize, float xsize, float xtopFactor, Color cor, float tempo)
        {
            Vector2 p0 = centro + (Vector2.right * xsize);//0
            Vector2 p1 = centro + (Vector2.up * ysize) + (Vector2.right * xsize * xtopFactor);//1
            Debug.DrawLine(p0, p1, cor, tempo);
            p0 = centro + (Vector2.up * ysize) + (Vector2.right * -xsize * xtopFactor);//2
            Debug.DrawLine(p0, p1, cor, tempo);
            p1 = centro + (Vector2.right * -xsize);//3
            Debug.DrawLine(p0, p1, cor, tempo);
            p0 = centro + (Vector2.down * ysize) + (Vector2.right * -xsize * xtopFactor);//4
            Debug.DrawLine(p0, p1, cor, tempo);
            p1 = centro + (Vector2.down * ysize) + (Vector2.right * xsize * xtopFactor);//5
            Debug.DrawLine(p0, p1, cor, tempo);
            p0 = centro + (Vector2.right * xsize);//0
            Debug.DrawLine(p0, p1, cor, tempo);
        }
        public static void DesenharHexagono(LineRenderer lr, Vector2 centro, float ysize, float xsize, float xtopFactor, Color cor)
        {
            List<Vector3> ps = new List<Vector3>();
            lr.startColor = cor;
            lr.endColor = cor;
            ps.Add(centro + (Vector2.right * xsize));//0
            ps.Add(centro + (Vector2.up * ysize) + (Vector2.right * xsize * xtopFactor));//1
            ps.Add(centro + (Vector2.up * ysize) + (Vector2.right * -xsize * xtopFactor));//2
            ps.Add(centro + (Vector2.right * -xsize));//3
            ps.Add(centro + (Vector2.down * ysize) + (Vector2.right * -xsize * xtopFactor));//4
            ps.Add(centro + (Vector2.down * ysize) + (Vector2.right * xsize * xtopFactor));//5
            ps.Add(centro + (Vector2.right * xsize));//0
            lr.positionCount = ps.Count;
            lr.SetPositions(ps.ToArray());
        }
        /// <summary>
        /// Desenha uma linha, mas pontilhada
        /// </summary>
        public static void DesenharLinhaPontilhada(Vector3 a, Vector3 b, Color cor, float tempo)
        {
            Vector3 ab = b - a;
            float tamanho = ab.magnitude;
            float quantModf = 1;
            if (tamanho > gapQuant / 2)
                quantModf = tamanho / (gapQuant / 2);
            ab.Normalize();
            float relacao = (tamanho / (gapQuant * quantModf));
            for (int i = 0; i < gapQuant * quantModf; i++)
            {
                float d1 = 2 * (relacao * i);
                float d2 = (relacao * (i + 1)) + (relacao * i);
                Debug.DrawLine(a + (ab * Mathf.Clamp(d1, 0, tamanho)), a + (ab * Mathf.Clamp(d2, 0, tamanho)), cor, tempo);
                if (d1 >= tamanho || d2 >= tamanho)
                    return;
            }
        }
        /// <summary>
        /// Cria 3 linhas que fazem intersecção no ponto que você introduzir, ou seja: cria um ponto visivel
        /// </summary>
        /// <param name="tamanho">o tamanho da linha.Só será usado metade do tamanho pra cada direção</param>
        public static void Point(Vector3 ponto, float tamanho, Color cor)
        {
            Debug.DrawLine(ponto + (Vector3.up * tamanho), ponto - (Vector3.up * tamanho), cor);
            Debug.DrawLine(ponto + (Vector3.right * tamanho), ponto - (Vector3.right * tamanho), cor);
            Debug.DrawLine(ponto + (Vector3.forward * tamanho), ponto - (Vector3.forward * tamanho), cor);
        }
        /// <summary>
        /// Cria 3 linhas que fazem intersecção no ponto que você introduzir, ou seja: cria um ponto visivel pelo tempo que introduzir
        /// </summary>
        /// <param name="tamanho">o tamanho da linha.Só será usado metade do tamanho pra cada direção</param>
        public static void Point(Vector3 ponto, float tamanho, Color cor, float tempo)
        {
            Debug.DrawLine(ponto + (Vector3.up * tamanho), ponto - (Vector3.up * tamanho), cor, tempo);
            Debug.DrawLine(ponto + (Vector3.right * tamanho), ponto - (Vector3.right * tamanho), cor, tempo);
            Debug.DrawLine(ponto + (Vector3.forward * tamanho), ponto - (Vector3.forward * tamanho), cor, tempo);
        }
        public static void DesenharBoundMinMax(Bounds bounds, Color cor, float tempo)
        {
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), bounds.min, cor, tempo);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), bounds.min, cor, tempo);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), bounds.min, cor, tempo);

            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), bounds.max, cor, tempo);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), bounds.max, cor, tempo);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), bounds.max, cor, tempo);

            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), cor, tempo);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), cor, tempo);

            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), cor, tempo);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), cor, tempo);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), cor, tempo);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), cor, tempo);
        }
        public static void DesenharBoundMinMax(Bounds bounds, Color cor)
        {
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), bounds.min, cor);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), bounds.min, cor);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), bounds.min, cor);

            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), bounds.max, cor);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), bounds.max, cor);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), bounds.max, cor);

            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), cor);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), cor);

            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), cor);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), cor);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), cor);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), cor);
        }
        /// <summary>
        /// Desenha um circulo dado o conjunto de pontos que o forma
        /// </summary>
        /// <param name="ps"></param>
        public static void DesenharCirculo(List<Vector2> ps, Color cor)
        {
            for (int i = 0; i < ps.Count; i++)
            {
                Debug.DrawLine(ps[(i + 1)% ps.Count], ps[i], cor);
            }
        }
        /// <summary>
        /// Desenha um circulo dado o conjunto de pontos que o forma
        /// </summary>
        /// <param name="ps"></param>
        public static void DesenharCirculo(List<Vector2> ps, Color cor,float tempo)
        {
            for (int i = 0; i < ps.Count; i++)
            {
                Debug.DrawLine(ps[(i + 1)% ps.Count], ps[i], cor, tempo);
            }
        }
    }
}
