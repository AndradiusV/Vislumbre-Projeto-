using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_Trans_Manager : MonoBehaviour
{
    [SerializeField] AudioCapture cap;
    [SerializeField] AudioReceiver rec;
    public static string prog_Audi_Excluido = "";
    public static Audio_Trans_Manager instance;
    private void Awake()
    {
        instance = this;
    }
    #region Capture
    internal void Validar()
    {
        cap.Validar();
    }
    internal bool Cap_Ativo() { return cap.Ativado; }
    /// <summary>
    /// Para capturar o audio de todo computador basta apenas passar 1234
    /// </summary>
    /// <param name="PID"></param>
    internal void Iniciar_Capture(int PID,bool include)
    {
        cap.Iniciar_Captura(PID, include);
    }
    internal void Parar_Capture()
    {
        cap.Parar();
    }
    #endregion
    #region Reciver
    internal bool Rec_Ativo() { return rec.Ativado; }
    internal void Iniciar_Reciver()
    {
        rec.Iniciar_Receive();
    }
    internal void Processar_Som(byte[] data)
    {
        rec.ProcessReceivedBytes(data);
    }
    internal void Parar_Reciver()
    {
        rec.Parar();
    }
    #endregion
}
