using GameServer;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using UnityEngine;

public class AudioCapture : MonoBehaviour
{
    private System.Diagnostics.Process captureProcess;
    private NamedPipeClientStream pipeClient;
    private Thread receiveThread;
    internal bool Ativado = false;

    [Header("Configurações de Áudio")]
    [SerializeField] private int targetPid = 25380;
    
    internal void Iniciar_Captura(int PID, bool include)
    {
        string exePath = "";
#if UNITY_EDITOR
        exePath = @"C:\Users\enrib\OneDrive\Desktop\AudioCatcher_Old.Net\AudioCatcher\bin\Release\AudioCatcher.exe";
#else
        exePath = Path.Combine(Application.dataPath, @"AudioCapture\net9.0\AudioCatcher.exe");
#endif
        targetPid = PID;

        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                Arguments = targetPid.ToString() + " " +include,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            captureProcess = System.Diagnostics.Process.Start(startInfo);
            //Avisos.instance.Mostrar_Aviso("AudioCatcher.exe iniciado. Conectando ao Pipe...");

            Ativado = true;
            receiveThread = new Thread(ConnectAndListenPipe);
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            Avisos.instance.Adicionar_Ao_Log("Erro ao iniciar o processo de áudio: " + ex.Message);
            Avisos.instance.Salvar_Log();
        }
    }

    void ConnectAndListenPipe()
    {
        string pipeName = $"AudioCapturePipe_{targetPid}";
        pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        try
        {
            pipeClient.Connect(5000);
            Debug.Log("Conectado ao pipe com sucesso!");

            byte[] lengthBuffer = new byte[4];

            while (Ativado && pipeClient.IsConnected)
            {
                // 1. Lê o tamanho do pacote Opus
                int bytesRead = 0;
                while (bytesRead < 4 && Ativado && pipeClient.IsConnected)
                {
                    int read = pipeClient.Read(lengthBuffer, bytesRead, 4 - bytesRead);
                    if (read == 0) break;
                    bytesRead += read;
                }

                if (bytesRead < 4) break;

                int opusLength = BitConverter.ToInt32(lengthBuffer, 0);
                if (opusLength <= 0 || opusLength > 1275) continue;

                // 2. Lê os bytes comprimidos do Opus
                byte[] opusData = new byte[opusLength];
                int totalOpusRead = 0;
                while (totalOpusRead < opusLength && Ativado && pipeClient.IsConnected)
                {
                    int read = pipeClient.Read(opusData, totalOpusRead, opusLength - totalOpusRead);
                    if (read == 0) break;
                    totalOpusRead += read;
                }

                if (totalOpusRead != opusLength) continue;


                if (Local_Client.instance != null && Local_Client.instance.transmitindo)
                {
                    Local_Client.instance.Enviar_SOM(opusData);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Conexão de áudio encerrada: " + ex.Message);
        }
    }

    private void OnDestroy() { Parar(); }
    private void OnApplicationQuit() { Parar(); }

    internal void Parar()
    {
        if (!Ativado)
            return;

        Ativado = false;
        if (pipeClient != null)
        { try { pipeClient.Close(); } catch { } }
        if (receiveThread != null && receiveThread.IsAlive) 
        { receiveThread.Join(1000); }
        if (captureProcess != null && !captureProcess.HasExited)
        {
            try { captureProcess.Kill(); captureProcess.Dispose(); } catch { }
        }
    }
}