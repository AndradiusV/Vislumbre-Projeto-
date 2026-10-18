using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using uWindowCapture;

public class EscolheJanela : MonoBehaviour
{
    public static EscolheJanela instance;
    private void Start()
    {
        instance = this;
        Ativado = false;

        UwcManager.onWindowAdded.AddListener(Listar_Janelas);
        UwcManager.onWindowRemoved.AddListener(Remover_Janela);
    }
    public static bool Ativado
    {
        get { return _Ativado; } private set {  _Ativado = value; }
    }
    static bool _Ativado;
    public void Alterar_Estado(bool ativado) => Ativado = ativado;

    internal Dictionary<string, Complet_Screen_ID> windows_IDs = new();
    [SerializeField] Janela_Item _prefab_Item;
    [SerializeField] CA_Vertical _vertical;
    [SerializeField] CA_item _horizontal_Pref;
    [SerializeField] List<CA_Horizontal> _horizontal_List;
    int janelas_Por_Linha = 5;
    public void Listar_Janelas(UwcWindow window)
    {
        if (!Ativado)
            return;

        windows_IDs.Clear();
        Achar_Janelas_TEXTURE();
        //Achar_Janelas_Audio();

        List<string> to_Remove = new List<string>();
        foreach (var pair in windows_IDs)
        {
            if(pair.Value.texture_PID == 0)
            {
                to_Remove.Add(pair.Key);
            }
        }
        for (int i = 0; i < to_Remove.Count; i++)
        {
            windows_IDs.Remove(to_Remove[i]);
        }
        to_Remove.Clear();

        //+$"<color=red> PID Áudio: {pair.Value.audio_PID} </color>|
        foreach (var pair in windows_IDs)
            UnityEngine.Debug.Log($"<color=yellow>Janela: {pair.Key} </color>| <color=cyan>Título: {pair.Value.titulo} </color>|" +
                " <color=green> PID Texture: {pair.Value.texture_PID} </color>|" +
                $"<color=blue> ID Janela: {pair.Value.window_ID}</color>");



        int quant_Janelas = windows_IDs.Count;
        int linhas = (quant_Janelas / janelas_Por_Linha) + 1;
        int janelas_Adicionadas = 0;
        string[] keys = windows_IDs.Keys.ToArray();
        _vertical.LimparFilhos();
        for (int i = 0; i < linhas; i++)
        {
            CA_item ca_H_Pai = _vertical.Adicionar(_horizontal_Pref, false);
            CA_Horizontal ca_H = ca_H_Pai.gameObject.GetComponentInChildren<CA_Horizontal>();
            _horizontal_List.Add(ca_H);
            for (int e = 0; e < janelas_Por_Linha && janelas_Adicionadas < quant_Janelas; e++)
            {
                Janela_Item item = ca_H.Adicionar(_prefab_Item, false);
                item.Set(windows_IDs[keys[janelas_Adicionadas]]);

                janelas_Adicionadas++;
            }
        }
        _vertical.PedirParaAjustar();
    }
    public void Remover_Janela(UwcWindow window)
    {
        if (!Ativado)
            return;

        List<string> to_Remove = new List<string>();
        foreach (var pair in windows_IDs)
        {
            if (pair.Value.texture_PID == window.processId)
            {
                to_Remove.Add(pair.Key);
                break;
            }
        }
        for (int i = 0; i < to_Remove.Count; i++)
        {
            windows_IDs.Remove(to_Remove[i]);
        }
        to_Remove.Clear();

        //+$"<color=red> PID Áudio: {pair.Value.audio_PID} </color>|
        foreach (var pair in windows_IDs)
            UnityEngine.Debug.Log($"<color=yellow>Janela: {pair.Key} </color>| <color=cyan>Título: {pair.Value.titulo} </color>|" +
                " <color=green> PID Texture: {pair.Value.texture_PID} </color>|" +
                $"<color=blue> ID Janela: {pair.Value.window_ID}</color>");


        int quant_Janelas = windows_IDs.Count;
        int linhas = (quant_Janelas / janelas_Por_Linha) + 1;
        int janelas_Adicionadas = 0;
        string[] keys = windows_IDs.Keys.ToArray();
        _vertical.LimparFilhos();
        for (int i = 0; i < linhas; i++)
        {
            CA_item ca_H_Pai = _vertical.Adicionar(_horizontal_Pref, false);
            CA_Horizontal ca_H = ca_H_Pai.gameObject.GetComponentInChildren<CA_Horizontal>();
            _horizontal_List.Add(ca_H);
            for (int e = 0; e < janelas_Por_Linha && janelas_Adicionadas < quant_Janelas; e++)
            {
                if (windows_IDs.ContainsKey(keys[janelas_Adicionadas]))
                {
                    Janela_Item item = ca_H.Adicionar(_prefab_Item, false);
                    item.Set(windows_IDs[keys[janelas_Adicionadas]]);

                    janelas_Adicionadas++;
                }
            }
        }
        _vertical.PedirParaAjustar();
    }
    private void Achar_Janelas_TEXTURE()
    {
        foreach (var pair in UwcManager.windows)
        {
            if (!pair.Value.isAltTabWindow || pair.Value.isBackground) continue;
            print(pair.Value.processId + " : " + pair.Value.title);

            try
            {
                Process p = Process.GetProcessById(pair.Value.processId);

                if (windows_IDs.ContainsKey(p.ProcessName))
                {
                    windows_IDs[p.ProcessName].texture_PID = pair.Value.processId;
                    windows_IDs[p.ProcessName].window_ID = pair.Value.id;
                }
                else
                {
                    Complet_Screen_ID screenID = new Complet_Screen_ID();
                    screenID.titulo = pair.Value.title;
                    screenID.texture_PID = p.Id;
                    screenID.window_ID = pair.Key;
                    windows_IDs.Add(p.ProcessName, screenID);
                    pair.Value.RequestCaptureIcon();
                }
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError($"Erro ao obter processo com PID {pair.Value.processId}: {ex.Message}");
                continue;
            }
        }
    }
    //private void Achar_Janelas_Audio()
    //{
    //    try
    //    {
    //        using (var enumerator = new MMDeviceEnumerator())
    //        {
    //            Avisos.instance.Adicionar_Ao_Log("1 - MMDeviceEnumerator criado");

    //            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render,Role.Multimedia);

    //            Avisos.instance.Adicionar_Ao_Log("2 - Dispositivo de áudio encontrado");

    //            var sessionManager = device.AudioSessionManager;

    //            Avisos.instance.Adicionar_Ao_Log("3 - SessionManager obtido");

    //            sessionManager.RefreshSessions();

    //            Avisos.instance.Adicionar_Ao_Log("4 - RefreshSessions terminou");

    //            int count = sessionManager.Sessions.Count;

    //            Avisos.instance.Adicionar_Ao_Log($"5 - Quantidade de sessões: {count}");

    //            if (count == 0)
    //            {
    //                Avisos.instance.Adicionar_Ao_Log("Nenhuma sessão de áudio encontrada no Windows.");

    //                return;
    //            }

    //            for (int i = 0; i < count; i++)
    //            {
    //                Avisos.instance.Adicionar_Ao_Log($"6 - Começando sessão {i}/{count}");

    //                try
    //                {

    //                    var session = sessionManager.Sessions[i];

    //                    Avisos.instance.Adicionar_Ao_Log($"7 - Sessão {i} obtida");

    //                    uint pid = session.GetProcessID;

    //                    Avisos.instance.Adicionar_Ao_Log($"8 - PID encontrado: {pid}");

    //                    if (pid == 0)
    //                        continue;

    //                    Process p;

    //                    try
    //                    {
    //                        p = Process.GetProcessById((int)pid);
    //                        UnityEngine.Debug.LogError($"A janela é {p.ProcessName}, PID é: {p.Id}");
    //                    }
    //                    catch
    //                    {
    //                        Avisos.instance.Adicionar_Ao_Log($"Processo {pid} não existe mais.");

    //                        continue;
    //                    }

    //                    // NÃO usa MainModule
    //                    string nome = p.ProcessName;

    //                    Avisos.instance.Adicionar_Ao_Log($"9 - Processo: {nome} | PID: {pid}");

    //                    if (windows_IDs.ContainsKey(nome))
    //                    {
    //                        windows_IDs[nome].audio_PID = (int)pid;
    //                    }
    //                    else
    //                    {
    //                        Complet_Screen_ID screenID = new Complet_Screen_ID();

    //                        screenID.titulo = nome;
    //                        screenID.audio_PID = (int)pid;

    //                        windows_IDs.Add(nome, screenID);
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Avisos.instance.Adicionar_Ao_Log($"Erro na sessão {i}: {ex.GetType().Name} - {ex.Message}");
    //                }
    //            }

    //            Avisos.instance.Adicionar_Ao_Log("10 - Terminou o FOR");

    //            Avisos.instance.Mostrar_Aviso("Acabei e vou salvar o log");

    //            Avisos.instance.Salvar_Log();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Avisos.instance.Adicionar_Ao_Log("Erro ao buscar PIDs: " +ex.GetType().Name +" - " +ex.Message);

    //        Avisos.instance.Salvar_Log();
    //    }

    //    Avisos.instance.Salvar_Log();
    //}
    public int Get_PID_By_Program_Nam(string nome)
    {
        int ret = 1234;
        try
        {
            foreach (var item in windows_IDs.Keys)
            {
                if(item == nome)
                {
                    return windows_IDs[item].texture_PID;
                }
            }
            //    using (var enumerator = new MMDeviceEnumerator())
            //    {
            //        // Pega o dispositivo de saída de som padrão (Caixas de som / Fone)
            //        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            //        var sessionManager = device.AudioSessionManager;

            //        // Atualiza a lista de sessões ativas do Windows
            //        sessionManager.RefreshSessions();

            //        int count = sessionManager.Sessions.Count;
            //        if (count == 0)
            //        {
            //            UnityEngine.Debug.LogWarning("Nenhuma sessão de áudio encontrada no Windows.");
            //            return ret;
            //        }

            //        bool foundSomething = false;

            //        for (int i = 0; i < count; i++)
            //        {
            //            var session = sessionManager.Sessions[i];
            //            uint pid = session.GetProcessID;

            //            if (pid == 0) continue; // Ignora o PID 0 (Sons nativos do Sistema)

            //            try
            //            {
            //                Process p = Process.GetProcessById((int)pid);

            //                if (p.MainModule.ModuleName == nome)
            //                {
            //                    return (int)pid;
            //                }
            //            }
            //            catch (ArgumentException)
            //            {
            //                // O processo já foi fechado, ignoramos
            //            }
            //        }

            //        if (!foundSomething)
            //        {
            //            UnityEngine.Debug.LogWarning("Nenhum dos processos listados está emitindo som neste exato instante. Certifique-se de que a música não está pausada!");
            //        }
            //    }
            //    //UnityEngine.Debug.Log("<color=cyan>--- FIM DA BUSCA ---</color>");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Erro ao buscar PIDs: " + ex.Message);
        }

        return ret;
    }
}
internal class Complet_Screen_ID
{
    public string titulo;
    //public int audio_PID;
    public int texture_PID;
    public int window_ID;
}