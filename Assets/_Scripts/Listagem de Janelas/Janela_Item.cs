using GameServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;

public class Janela_Item : CA_item
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI txt;
    [SerializeField] TextMeshProUGUI info_txt;
    Complet_Screen_ID info;
    internal void Set(Complet_Screen_ID id_ref)
    {
        info = id_ref;
        txt.text = UwcManager.windows[id_ref.window_ID].title;
        if(txt.text.Length > 10)
        {
            txt.text = txt.text.Substring(0, 10) + "...";
        }
        info_txt.text = $"Process PID: {info.texture_PID} | ID window: {info.window_ID}";
        window = UwcManager.windows[id_ref.window_ID];
        arrumado = false;
    }
    public UwcWindow window { get; set; }
    bool arrumado = false;
    private void Update()
    {
        if (window == null || arrumado) return;

        if (!(!window.hasIconTexture && !window.isIconic))
        {
            //icon.texture = window.iconTexture;
            icon.sprite = Sprite.Create(window.iconTexture,
                new Rect(0, 0, window.iconTexture.width, window.iconTexture.height),
                new Vector2(0.5f, 0.5f));
            arrumado = true;
        }
    }
    public  void Escolher()
    {
        Local_Client.instance.Escolher_Tela(info);
    }
}
