using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GameServer;

internal static class Opcoes
{
    internal static DadosDeConfiguracao config_Atual;
    // Caminho onde o arquivo será salvo// Função que você vai chamar quando o jogador clicar em "Salvar Opções"
    internal static void SalvarConfiguracoes()
    {
        string caminhoDoArquivo = Application.dataPath + "/configuracoes.json";
        // =========================================================
        // 1. ÁREA ONDE VOCÊ JUNTA A INFORMAÇÃO
        // =========================================================

        // Criamos um "pacote" vazio com a nossa classe de dados
        config_Atual = new DadosDeConfiguracao();

        // Aqui você pega as informações do seu jogo e coloca dentro do pacote.
        config_Atual.volume_Geral = AudioListener.volume;
        config_Atual.volume_Ping = SoundManager.volume;
        config_Atual.qualidadeGrafica = QualitySettings.GetQualityLevel();
        config_Atual.idioma = "PT-BR";
        config_Atual.resloução_Atual = new Vector2Int(Screen.width, Screen.height);

        config_Atual.Avisos = Avisos.avisos;
        config_Atual.is_Pined = TransparencyWindowController.is_Pined;

        config_Atual.permite_Desenho = Desenho_Na_Tela.permi_Desenho;
        config_Atual.desenho_tipo_Preferido = Desenho_Na_Tela.desenho_tipo_Preferido;
        config_Atual.brush_Size = Desenho_Na_Tela.brushSize;
        config_Atual.brush_Color = Desenho_Na_Tela.brushColor;
        config_Atual.D_width = Desenho_Na_Tela.width;
        config_Atual.D_height = Desenho_Na_Tela.height;

        config_Atual.cor_base = Selecionador_De_Cor.cor;
        config_Atual.intencidade_Branco = Selecionador_De_Cor.intencidade_Branco;
        config_Atual.intencidade_Preto = Selecionador_De_Cor.intencidade_Preto;
        config_Atual.opacidade = Selecionador_De_Cor.opacidade;

        config_Atual.prog_Audi_Excluido = Audio_Trans_Manager.prog_Audi_Excluido;
        config_Atual.meu_ip = Local_Client.meu_IP;
        config_Atual.BitRate = (ulong)WebRTC_Video_S_R.BitRate;

        config_Atual.FPS_Target = (uint)WebRTC_Video_S_R.FPS_Target;
        config_Atual.encoder_Preferido = WebRTC_Video_S_R.encoder_Preferido;

        // =========================================================
        // 2. SALVA O ARQUIVO (JSON)
        // =========================================================
        try
        {
            // Transforma o nosso "pacote" de dados em um texto formato JSON
            // O "true" no final faz o JSON ficar formatado bonitinho com quebras de linha (fácil de ler)
            string textoJson = JsonUtility.ToJson(config_Atual, true);

            // Escreve o texto no arquivo no computador do jogador
            File.WriteAllText(caminhoDoArquivo, textoJson);

            Debug.Log("Configurações salvas com sucesso em: " + caminhoDoArquivo);
        }
        catch (System.Exception erro)
        {
            Debug.LogError("Erro ao tentar salvar o arquivo: " + erro.Message);
        }
    }

    // BÔNUS: Como ler o arquivo de volta quando o jogo abrir
    public static void CarregarConfiguracoes()
    {
        string caminhoDoArquivo = Application.dataPath + "/configuracoes.json";
        if (File.Exists(caminhoDoArquivo))
        {
            // Lê o texto do arquivo JSON
            string textoJson = File.ReadAllText(caminhoDoArquivo);

            // Transforma o texto de volta na nossa classe de dados
            config_Atual = JsonUtility.FromJson<DadosDeConfiguracao>(textoJson);

            // Agora você aplica no jogo!
            AudioListener.volume = config_Atual.volume_Geral;
            SoundManager.volume = config_Atual.volume_Ping;
            Avisos.avisos = config_Atual.Avisos;
            TransparencyWindowController.is_Pined = config_Atual.is_Pined;
            QualitySettings.SetQualityLevel(config_Atual.qualidadeGrafica);

            Desenho_Na_Tela.permi_Desenho = config_Atual.permite_Desenho;
            Desenho_Na_Tela.desenho_tipo_Preferido = config_Atual.desenho_tipo_Preferido;
            Desenho_Na_Tela.brushSize = config_Atual.brush_Size;
            Desenho_Na_Tela.brushColor = config_Atual.brush_Color;
            Desenho_Na_Tela.width = config_Atual.D_width;
            Desenho_Na_Tela.height = config_Atual.D_height;

            Selecionador_De_Cor.cor = config_Atual.cor_base;
            Selecionador_De_Cor.intencidade_Branco = config_Atual.intencidade_Branco;
            Selecionador_De_Cor.intencidade_Preto = config_Atual.intencidade_Preto;
            Selecionador_De_Cor.opacidade = config_Atual.opacidade;

            Audio_Trans_Manager.prog_Audi_Excluido = config_Atual.prog_Audi_Excluido;
            Local_Client.meu_IP = config_Atual.meu_ip;
            WebRTC_Video_S_R.BitRate = config_Atual.BitRate;

            WebRTC_Video_S_R.FPS_Target = config_Atual.FPS_Target;
            WebRTC_Video_S_R.encoder_Preferido= config_Atual.encoder_Preferido;

            WebRTC_Video_S_R.Atualizar_Transmissao();
            Debug.Log("Configurações carregadas com sucesso!");
        }
        else
        {
            SalvarConfiguracoes();
            Debug.Log("Arquivo de save não encontrado. O jogo usará as configurações padrão.");
        }
    }
}
// 1. CLASSE DE DADOS (O que vamos salvar)
// Precisa ter [System.Serializable] para a Unity conseguir transformar em JSON
[System.Serializable]
public class DadosDeConfiguracao
{
    public float volume_Geral;
    public float volume_Ping;
    public int qualidadeGrafica;
    public string idioma;
    public Vector2Int resloução_Atual;
    public bool Avisos;
    public bool is_Pined;

    #region Desenho
    public Permissão_De_Desenho permite_Desenho;
    public Permissão_De_Desenho desenho_tipo_Preferido = Permissão_De_Desenho.fixo;
    public float brush_Size = 1;
    public Color brush_Color = Color.white;
    public Color cor_base;

    public int D_width = 640;
    public int D_height = 480;

    public float intencidade_Branco;
    public float intencidade_Preto;
    public float opacidade;
    #endregion

    #region Transmissão
    public string prog_Audi_Excluido;
    public string meu_ip;
    public ulong BitRate = 4_000_000;
    public uint FPS_Target = 30;
    public string encoder_Preferido;
    #endregion
}