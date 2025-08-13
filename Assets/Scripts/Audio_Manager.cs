using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    [Header("Audio instance")]
    public static Audio_Manager instance;
    [Header("Audio properties")]
    [SerializeField] private AudioSource audioSourceInteraction;
    [SerializeField] private AudioSource audioSourceMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //This is going to be called when I need to play an interaction audio clip from an NPC or other object.
    public void PlayInteractionAudio(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSourceInteraction.PlayOneShot(audioClip);
        }
        else
        {
            Debug.LogWarning("Audio clip is null, cannot play interaction audio.");
        }
    }
    //This is going to be called when I need to play music for a especific stage.
    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            audioSourceMusic.clip = clip;
            audioSourceMusic.loop = true;
            audioSourceMusic.Play();
        }
        else
        {
            Debug.LogWarning("Music clip is null, cannot play music.");
        }
    }
}
