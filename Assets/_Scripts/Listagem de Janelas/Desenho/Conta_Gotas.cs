using UnityEngine;
using UnityEngine.Rendering;
using System;
using Unity.Collections;

public class Conta_Gotas : MonoBehaviour
{
    public Shader conta_Gotas_shader;
    private Material material;
    private RenderTexture pixelTexture;

    private bool initialized;
    private bool reading;

    private void Initialize()
    {
        if (initialized) return;

        material = new Material(conta_Gotas_shader);

        pixelTexture = new RenderTexture(
            1, 1, 0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        );

        pixelTexture.filterMode = FilterMode.Point;
        pixelTexture.Create();

        initialized = true;
    }

    public bool ReadPixel(RenderTexture source,int x,int y,Action<Color32> callback)
    {
        if (reading || source == null)
            return false;

        if (!SystemInfo.supportsAsyncGPUReadback)
            return false;

        if (x < 0 || x >= source.width ||
            y < 0 || y >= source.height)
            return false;

        Initialize();

        if (!initialized)
            return false;

        reading = true;

        material.SetVector(
            "_PixelCoord",
            new Vector4(x, y, 0, 0)
        );

        material.SetVector(
            "_SourceSize",
            new Vector4(source.width, source.height, 0, 0)
        );

        // Extrai somente o pixel desejado
        Graphics.Blit(source, pixelTexture, material);

        // Transfere somente 1x1 para a CPU
        AsyncGPUReadback.Request(
            pixelTexture,
            0,
            TextureFormat.RGBA32,
            request =>
            {
                reading = false;

                if (request.hasError)
                {
                    Debug.LogError("Erro ao ler pixel da GPU.");
                    return;
                }

                NativeArray<Color32> data =
                    request.GetData<Color32>();

                callback?.Invoke(data[0]);
            }
        );

        return true;
    }

    public bool ReadPixel(Texture source,int x,int y,Action<Color32> callback)
    {
        if (reading || source == null)
            return false;

        if (!SystemInfo.supportsAsyncGPUReadback)
            return false;

        if (x < 0 || x >= source.width ||
            y < 0 || y >= source.height)
            return false;

        Initialize();

        if (!initialized)
            return false;

        reading = true;

        material.SetVector(
            "_PixelCoord",
            new Vector4(x, y, 0, 0)
        );

        material.SetVector(
            "_SourceSize",
            new Vector4(source.width, source.height, 0, 0)
        );

        // Extrai somente o pixel desejado
        Graphics.Blit(source, pixelTexture, material);

        // Transfere somente 1x1 para a CPU
        AsyncGPUReadback.Request(
            pixelTexture,
            0,
            TextureFormat.RGBA32,
            request =>
            {
                reading = false;

                if (request.hasError)
                {
                    Debug.LogError("Erro ao ler pixel da GPU.");
                    return;
                }

                NativeArray<Color32> data =
                    request.GetData<Color32>();

                callback?.Invoke(data[0]);
            }
        );

        return true;
    }

    private void OnDestroy()
    {
        if (pixelTexture != null)
        {
            pixelTexture.Release();
            Destroy(pixelTexture);
        }

        if (material != null)
            Destroy(material);
    }
}
