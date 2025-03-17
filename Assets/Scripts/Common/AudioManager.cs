using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; 
    public AudioSource soundEffectSource; 
    public AudioClip[] soundEffects; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); 
    }

    public void PlaySound(string soundName)
    {
        AudioClip clip = GetAudioClip(soundName);
        if (clip != null)
        {
            soundEffectSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetAudioClip(string name)
    {
        foreach (var clip in soundEffects)
        {
            if (clip.name == name)
                return clip;
        }
        Debug.LogWarning("Sound not found: " + name);
        return null;
    }
}