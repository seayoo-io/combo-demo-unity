using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    private static Dictionary<string, AudioSource> musics = new Dictionary<string, AudioSource>();
    private static Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("SoundManager").AddComponent<SoundManager>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    public void PlayMusic(string url, bool isLoop = true)
    {
        AudioSource audioSource = null;
        if(musics.ContainsKey(url))
        {
            audioSource  = musics[url];
        }
        else
        {
            GameObject sound = new GameObject("Musics/"+url);
            sound.transform.parent = instance.transform;
            audioSource = sound.AddComponent<AudioSource>();
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + url);
            audioSource.clip = clip;
            audioSource.loop = isLoop;
            audioSource.playOnAwake = true;

            musics.Add(url,audioSource);
        }
        audioSource.enabled = true;
        audioSource.Play();//开始播放
    }

    public void StopMusic(string url)
    {
        if(!musics.ContainsKey(url))
        {
            return;
        }
        musics[url].Stop();
    }

    public void StopAllMusics()
    {
        foreach(AudioSource source in musics.Values)
        {
            source.Stop();
        }
    }

    public void DeleteMusic(string url)
    {
        if(!musics.ContainsKey(url))
        {
            return;
        }

        musics.Remove(url);
        GameObject.Destroy(musics[url].gameObject);
    }

    public void PlaySound(string url, bool isLoop = false)
    {
        AudioSource audioSource =null;
        if(sounds.ContainsKey(url))
        {
            audioSource = sounds[url];
        }
        else
        {
            GameObject sound = new GameObject("Sounds/"+url);
            sound.transform.parent = instance.transform;
            audioSource = sound.AddComponent<AudioSource>();
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + url);
            audioSource.clip = clip;
            audioSource.loop = isLoop;
            audioSource.playOnAwake = true;

            sounds.Add(url,audioSource);
        }
        audioSource.enabled = true;
        audioSource.Play();
    }

    public void StopSound(string url)
    {
        if(!sounds.ContainsKey(url))
        {
            return;
        }
        sounds[url].Stop();
    }

    public void StopAllSounds()
    {
        foreach(AudioSource source in sounds.Values)
        {
            source.Stop();
        }
    }

    public void DeleteSound(string url)
    {
        if(!sounds.ContainsKey(url))
        {
            return;
        }

        sounds.Remove(url);
        GameObject.Destroy(sounds[url].gameObject);
    }
}
