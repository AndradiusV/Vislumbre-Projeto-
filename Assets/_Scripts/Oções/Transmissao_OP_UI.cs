using GameServer;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class Transmissao_OP_UI : MonoBehaviour
{
    #region Escolher D/E-Conding Type
    [SerializeField] TMP_Dropdown dp_Coding_Type;
    public void Carregar_Coding_Type()
    {
        var codecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;

        dp_Coding_Type.ClearOptions();
        List<string> options = new List<string>();
        foreach (var item in codecs)
        {
            options.Add(item.mimeType);
        }
        dp_Coding_Type.AddOptions(options);
        dp_Coding_Type.SetValueWithoutNotify(WebRTC_Video_S_R.encoder_Preferido_ID);
    }
    public void Escolher_Coding_Type(int valor)
    {
        WebRTC_Video_S_R.encoder_Preferido_ID = valor;
        WebRTC_Video_S_R.encoder_Preferido = WebRTC_Video_S_R.encoders[valor];
        Opcoes.SalvarConfiguracoes();
        WebRTC_Video_S_R.Atualizar_Transmissao();
    }
    #endregion
    #region FPS
    [SerializeField] TMP_InputField imp_FPS;
    public void Carregar_FPS()
    {
        imp_FPS.SetTextWithoutNotify(WebRTC_Video_S_R.FPS_Target.ToString());
    }
    public void Escolher_FPS(string valor)
    {
        if(int.TryParse(valor,out int result))
        {
            WebRTC_Video_S_R.FPS_Target = (uint)Mathf.Clamp(result,10,60);
            imp_FPS.SetTextWithoutNotify(result.ToString());
        }
        else
        {
            imp_FPS.SetTextWithoutNotify("30");
            WebRTC_Video_S_R.FPS_Target = 30;
        }
        Opcoes.SalvarConfiguracoes();
        WebRTC_Video_S_R.Atualizar_Transmissao();
    }
    #endregion
    #region Pacotes size
    [SerializeField] TextMeshProUGUI _tamanho_Pacote_TXT;
    public Slider tamanho_Pacote_SL;
    public void Carregar_BitRate()
    {
        ulong nivel = ((ulong)WebRTC_Video_S_R.BitRate - 1_000_000) / 1_000_000;
        tamanho_Pacote_SL.SetValueWithoutNotify((float)nivel);
        _tamanho_Pacote_TXT.text = $"BitRate: {WebRTC_Video_S_R.BitRate % 1_000_000} MB";
    }
    public void Definir_BitRate(float valor)
    {
        WebRTC_Video_S_R.BitRate = 1_000_000 + ((ulong)(int)valor * 1_000_000);
        Opcoes.SalvarConfiguracoes();
        WebRTC_Video_S_R.Atualizar_Transmissao();
        _tamanho_Pacote_TXT.text = $"BitRate: {WebRTC_Video_S_R.BitRate / 1_000_000} MB";
    }
    #endregion
    #region Resolution Scale
    [SerializeField] TextMeshProUGUI _Resolução_TXT;
    [SerializeField] TMP_Dropdown dp_Resolução;
    public void Carregar_Resolução()
    {
        switch (WebRTC_Video_S_R.scaleResolutionDownBy)
        {
            case 1.0:
                dp_Resolução.SetValueWithoutNotify(0);
                break;
            case 1.5:
                dp_Resolução.SetValueWithoutNotify(1);
                break;
            case 2.25:
                dp_Resolução.SetValueWithoutNotify(2);
                break;
            case 3.0:
                dp_Resolução.SetValueWithoutNotify(3);
                break;
            default:
                break;
        }
        _Resolução_TXT.text = $"Qualidade Alvo: ";
    }
    public void Definir_Resolução(int valor)
    {
        switch (valor)
        {
            case 0:
                WebRTC_Video_S_R.scaleResolutionDownBy = 1.0;
                break;
            case 1:
                WebRTC_Video_S_R.scaleResolutionDownBy = 1.5;
                break;
            case 2:
                WebRTC_Video_S_R.scaleResolutionDownBy = 2.25;
                break;
            case 3:
                WebRTC_Video_S_R.scaleResolutionDownBy = 3.0;
                break;
            default:
                break;
        }
        _Resolução_TXT.text = $"Qualidade Alvo: ";
        WebRTC_Video_S_R.Atualizar_Transmissao();
        Opcoes.SalvarConfiguracoes();
    }
    #endregion

    #region Seletor de Audio Excluder
    [SerializeField] Seletor_Exclude_Program_Audio Seletor_De_Prog_Ignorar;
    public void Abrir_Seletor()
    {
        Seletor_De_Prog_Ignorar.Abrir();
    }
    #endregion
}
