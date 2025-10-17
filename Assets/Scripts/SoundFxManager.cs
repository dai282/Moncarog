using UnityEngine;

public class SoundFxManager : MonoBehaviour
{
    public static SoundFxManager Instance { get; private set; }

    [SerializeField] private AudioSource soundFXObject;
    private AudioSource currentWalkingAudio; // Track walking audio

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //spawn game object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign the audio clip
        audioSource.clip = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = audioClip.length;

        //destroy game object after playing
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        int rand = Random.Range(0, audioClip.Length);

        //spawn game object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign the audio clip
        audioSource.clip = audioClip[rand];

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of clip
        float clipLength = audioClip[rand].length;

        //destroy game object after playing
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayWalkingSoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // If already playing, don't restart
        if (currentWalkingAudio != null && currentWalkingAudio.isPlaying)
        {
            return;
        }

        // Clean up old reference if it exists but isn't playing
        if (currentWalkingAudio != null)
        {
            Destroy(currentWalkingAudio.gameObject);
        }

        //spawn game object
        currentWalkingAudio = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign the audio clip
        currentWalkingAudio.clip = audioClip;

        //assign volume
        currentWalkingAudio.volume = volume;

        //play sound
        currentWalkingAudio.Play();

        //get length of clip
        float clipLength = audioClip.length;

        //destroy game object after playing
        Destroy(currentWalkingAudio.gameObject, clipLength);
    }

    public void StopWalkingSound()
    {
        if (currentWalkingAudio != null)
        {
            Destroy(currentWalkingAudio.gameObject);
            currentWalkingAudio = null;
        }
    }
}
