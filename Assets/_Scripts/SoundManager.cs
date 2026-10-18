using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource m_AudioSource;
    public static float volume;
    [SerializeField] AudioSource m_AudioSource_Pref;
    List<AudioSource> audios = new List<AudioSource>();
    public static SoundManager instance;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < audios.Count; i++)
        {
            if (!audios[i].isPlaying)
            {
                Destroy(audios[i].gameObject);
                audios.RemoveAt(i);
                i--;
            }
        }
    }
    public void Tocar_Som(AudioClip som)
    {
        AudioSource aud = Instantiate(m_AudioSource_Pref);
        aud.volume = volume;
        aud.clip = som;
        audios.Add(aud);

        aud.Play();
    }
    [SerializeField] AudioClip[] pings;
    public void Pingar(int ping_id)
    {
        AudioSource aud = Instantiate(m_AudioSource_Pref);
        aud.clip = pings[Mathf.Clamp(ping_id,0,pings.Length-1)];
        audios.Add(aud);

        aud.Play();
    }
    public int Pingar()
    {
        int ret = Random.Range(0, pings.Length);
        AudioSource aud = Instantiate(m_AudioSource_Pref);
        aud.clip = pings[ret];
        audios.Add(aud);

        aud.Play();
        return ret;
    }
    public int Ping_Id() { return Random.Range(0, pings.Length); }
}
