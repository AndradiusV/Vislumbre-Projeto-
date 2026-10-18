using System;
using UnityEngine;
using Concentus.Structs; // Exige a Concentus.dll na pasta Assets/Plugins

public class AudioReceiver : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AudioSource audioSource;

    private OpusDecoder decoder;
    private short[] pcmDecodeBuffer;

    private float[] ringBuffer;
    private int bufferSize = 48000; // 1 seg a 48kHz
    private int writeHead = 0;
    private int readHead = 0;
    private int bufferedSamples = 0;
    private bool isBuffering = true;
    private const int PREBUFFER_SIZE = 2400; // ~50ms de segurança contra estalos

    private readonly object lockObject = new object();

    internal bool Ativado;
    internal void Iniciar_Receive()
    {
        if(Ativado) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Inicializa o Decodificador Opus (48kHz, 1 Canal Mono)
        decoder = new OpusDecoder(48000, 1);
        pcmDecodeBuffer = new short[960]; // Buffer para armazenar até 20ms de PCM decodificado

        ringBuffer = new float[bufferSize];
        writeHead = 0;
        readHead = 0;
        bufferedSamples = 0;
        isBuffering = true;

        if (audioSource != null)
        {
            audioSource.Play();
        }

        Ativado = true;
    }

    /// <summary>
    /// Recebe os bytes COMPRIMIDOS em OPUS (da rede ou do capture), decodifica e joga no RingBuffer
    /// </summary>
    internal void ProcessReceivedBytes(byte[] opusBytes)
    {
        if (!Ativado || ringBuffer == null || decoder == null) return;

        lock (lockObject)
        {
            // 1. Decodifica o Opus comprimido para amostras PCM16
            int decodedSamples = decoder.Decode(opusBytes, 0, opusBytes.Length, pcmDecodeBuffer, 0, 960, false);

            // 2. Converte e insere no ringBuffer
            for (int i = 0; i < decodedSamples; i++)
            {
                ringBuffer[writeHead] = pcmDecodeBuffer[i] / 32768.0f;

                writeHead = (writeHead + 1) % bufferSize;
                bufferedSamples++;

                if (writeHead == readHead)
                {
                    readHead = (readHead + 1) % bufferSize;
                    bufferedSamples--;
                }
            }

            if (isBuffering && bufferedSamples >= PREBUFFER_SIZE)
            {
                isBuffering = false;
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (ringBuffer == null) return;

        int dataLength = data.Length;

        lock (lockObject)
        {
            for (int i = 0; i < dataLength; i += channels)
            {
                float sample = 0f;

                if (!isBuffering && bufferedSamples > 0)
                {
                    sample = ringBuffer[readHead];
                    readHead = (readHead + 1) % bufferSize;
                    bufferedSamples--;
                }
                else
                {
                    isBuffering = true;
                }

                for (int c = 0; c < channels; c++)
                {
                    data[i + c] = sample;
                }
            }
        }
    }
    internal void Parar()
    {
        Ativado = false;
        ringBuffer = null;
        try
        {
            audioSource.Stop();

            if (decoder != null)
                decoder.Dispose();

            pcmDecodeBuffer = null;
            isBuffering = false;
        }
        catch
        {

        }
    }

    private void OnDestroy()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}