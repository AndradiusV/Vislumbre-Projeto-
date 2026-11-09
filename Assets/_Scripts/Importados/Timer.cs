using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    #region Persistente
    static List<TimerToken> tokens = new List<TimerToken>();
    static List<TimerToken> tokens_Continuos = new List<TimerToken>();
    #endregion
    #region Descartaveis
    static List<TimerToken> tokens_Descartavel = new List<TimerToken>();
    static List<TimerToken> tokens_Continuos_Descartavel = new List<TimerToken>();
    #endregion
    public static Timer instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    private void Update()
    {
        if (GameManager.Pause)
            return;

        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].Tick())
            {
                if (!tokens[i].IsDispose)
                    tokens[i].Dispose();
                else
                    tokens.RemoveAt(i);
                i--;
            }
        for (int i = 0; i < tokens_Continuos.Count; i++)
            if (tokens_Continuos[i].Tick())
            {
                if(!tokens_Continuos[i].IsDispose)
                    tokens_Continuos[i].Dispose();
                else
                    tokens_Continuos.RemoveAt(i);
                i--;
            }


        for (int i = 0; i < tokens_Descartavel.Count; i++)
            if (tokens_Descartavel[i].Tick())
            {
                if(!tokens_Descartavel[i].IsDispose)
                    tokens_Descartavel[i].Dispose();
                else
                    tokens_Descartavel.RemoveAt(i);
                i--;
            }
        for (int i = 0; i < tokens_Continuos_Descartavel.Count; i++)
            if (tokens_Continuos_Descartavel[i].Tick())
            {
                if(!tokens_Continuos_Descartavel[i].IsDispose)
                    tokens_Continuos_Descartavel[i].Dispose();
                else
                    tokens_Continuos_Descartavel.RemoveAt(i);
                i--;
            }
    }
    /// <summary>
    /// Cria um token para ativar uma ação depois de um tempo t determinado
    /// </summary>
    /// <param name="t">Tempo em segundos</param>
    /// <param name="ac">ação definida</param>
    /// <param name="dispodeac">ação que limpa todas as referencias do token</param>
    /// <returns></returns>
    internal static TimerToken AdicionarToken(float t, Action ac, Action dispodeac, bool descartavel = true)
    {
        TimerToken token = null;
        if (!descartavel)
        {
            token = new TimerToken(t, ac, dispodeac, tokens);
            tokens.Add(token);
        }
        else
        {
            token = new TimerToken(t, ac, dispodeac, tokens_Descartavel);
            tokens_Descartavel.Add(token);
        }
        return token;
    }
    /// <summary>
    /// Cria um token para ativar uma ação depois de um tempo t determinado, e que fica execultando uma função a cada tick
    /// </summary>
    /// <param name="t">Tempo em segundos</param>
    /// <param name="ac">ação feita quando o token é concluido.</param>
    /// <param name="acc">ação continua definida. é uma achão q erepetira a cada tick</param>
    /// <param name="dispodeac">ação para quando o token é finalizado.</param>
    /// <returns></returns>
    internal static TimerToken AdicionarToken(float t, Action ac, Action<float> acc, Action dispodeac, bool descartavel = false)
    {
        TimerTokenContinuo token = null;
        if (!descartavel)
        {
            token = new TimerTokenContinuo(t, ac, acc, dispodeac, tokens_Continuos);
            tokens_Continuos.Add(token);
        }
        else
        {
            token = new TimerTokenContinuo(t, ac, acc, dispodeac, tokens_Continuos_Descartavel);
            tokens_Continuos_Descartavel.Add(token);
        }
        return token;
    }

    #region Tokens Prontos
    internal static TimerToken FeedBack_De_Texto(string texto_inicial,string texto,TextMeshProUGUI _feedBack_Text, float duration_Base, float color_chance_times, Color B)
    {
        Color A = _feedBack_Text.color;
        _feedBack_Text.text = texto;

        float dutarion = duration_Base + Mathf.Clamp((texto.Length / 36f), 0, 3f);

        TimerToken ret = AdicionarToken(dutarion, () =>
        {

        }, t =>
        {
            float c_t = t / dutarion;
            c_t *= color_chance_times * Mathf.PI;
            _feedBack_Text.color = Color.Lerp(A, B, Mathf.Abs(Mathf.Sin(c_t)));
        }, () =>
        {
            _feedBack_Text.text = texto_inicial;
            _feedBack_Text.color = A;
        }
        , true);
        return ret;
    }
    #endregion

    internal static void Limpar()
    {
        for (int i = 0; i < tokens.Count;)
            tokens[i].Dispose();
        for (int i = 0; i < tokens_Continuos.Count;)
            tokens_Continuos[i].Dispose();
    }
    internal static void Limpar_Descartaveis()
    {
        for (int i = 0; i < tokens_Descartavel.Count;)
            tokens_Descartavel[i].Dispose();
        for (int i = 0; i < tokens_Continuos_Descartavel.Count;)
            tokens_Continuos_Descartavel[i].Dispose();
    }

    internal static void Limpar_Todos()
    {
        for (int i = 0; i < tokens.Count;)
            tokens[i].Dispose();
        for (int i = 0; i < tokens_Descartavel.Count;)
            tokens_Descartavel[i].Dispose();
        for (int i = 0; i < tokens_Continuos.Count;)
            tokens_Continuos[i].Dispose();
        for (int i = 0; i < tokens_Continuos_Descartavel.Count;)
            tokens_Continuos_Descartavel[i].Dispose();
    }
}
internal class TimerToken : IDisposable
{
    /// <summary>
    /// Tempo até execultar a ação
    /// </summary>
    protected float _t;
    /// <summary>
    /// Ação a ser executada quando o tempo acabar
    /// </summary>
    protected Action _ac;
    /// <summary>
    /// Cria o Token com tudo que ele precisa
    /// </summary>
    /// <param name="time">tempo em segundos para o timer "tocar"</param>
    /// <param name="ac">ação que executará ao o timer "tocar"</param>
    /// <param name="disposeAC">Ação de dispose</param>
    /// <param name="l">lista em que esse token estará armazenado no Timer</param>
    public TimerToken(float time, Action ac, Action disposeAC, List<TimerToken> l)
    {
        _t = time;
        _ac = ac;
        _IsDispose = false;
        disposeAC += () => l.Remove(this);
        _disposeAction = disposeAC;
    }
    /// <summary>
    /// Cont 1 deltaTime e caso o tempo t seja atingio executa a ação
    /// </summary>
    /// <returns></returns>
    public virtual bool Tick()
    {
        if (IsDispose)
            return true;

        _t -= Time.deltaTime;
        if (_t < 0)
        {
            if (_ac != null)
                _ac();
            return true;
        }
        return false;
    }

    #region dispose
    /// <summary>
    /// Ação de dispode
    /// </summary>
    protected Action _disposeAction;
    protected bool _IsDispose;
    public bool IsDispose { get { return _IsDispose; } private set { _IsDispose = value; } }
    public virtual void Dispose()
    {
        if (IsDispose)
            return;

        _IsDispose = true;
        _disposeAction();
    }
    #endregion
}
internal class TimerTokenContinuo : TimerToken
{
    float _t_Contado;
    Action<float> acc;
    public TimerTokenContinuo(float time, Action ac, Action<float> acContinua, Action disposeAC, List<TimerToken> l) : base(time, ac, disposeAC, l)
    {
        _t_Contado = 0;
        _t = time;
        _ac = ac;
        acc = acContinua;
        _IsDispose = false;
        disposeAC += () => l.Remove(this);
        _disposeAction = disposeAC;
    }
    /// <summary>
    /// Cont 1 deltaTime e caso o tempo t seja atingio executa a ação
    /// </summary>
    /// <returns></returns>
    public override bool Tick()
    {
        if (IsDispose)
            return true;

        _t -= Time.deltaTime;
        _t_Contado += Time.deltaTime;
        acc(_t_Contado);
        if (_t < 0)
        {
            _ac();
            return true;
        }
        return false;
    }
}