using System.Net;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class NetworkUtils
{
    /// <summary>
    /// Faz um pedido para um servidor online super simples e publico onde ele te devolve o seu ip publico :v
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetPublicIP()
    {
        using HttpClient client = new HttpClient();

        string ip = await client.GetStringAsync("https://api.ipify.org");

        return ip.Trim();
    }

    public static async Task<string> ObterIPv6PublicoAsync()
    {
        try
        {
            using HttpClient cliente = new HttpClient();

            // Esse endpoint específico (api6) FORÇA a checagem via IPv6.
            // Ele vai retornar apenas o IP em formato de texto.
            string ip = await cliente.GetStringAsync("https://api6.ipify.org");

            return ip;
        }
        catch (System.Exception ex)
        {
            // Se cair aqui, é a prova definitiva de que a sua operadora não te deu um IPv6 de verdade.
            UnityEngine.Debug.LogError($"<color=red>Falha ao buscar IPv6.</color> Sua rede não suporta IPv6 ou está bloqueada. Erro: {ex.Message}");
            return null;
        }
    }
}