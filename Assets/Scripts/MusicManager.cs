using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;

    public AudioClip bgMusic;
    public AudioClip combatMusic;

    private bool bgPlaying = true;

    void Awake()
    {
        //Singleton pattern 
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //playing regular background music on start
        if (bgMusic != null)
        {
            audioSource.clip = bgMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    //swap track function to be used by other scripts
    public void SwapTrack()
    {
        if (bgPlaying)
        {
            audioSource.clip = combatMusic;
            audioSource.volume = audioSource.volume + 0.7f;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = bgMusic;
            audioSource.volume = audioSource.volume - 0.7f;
            audioSource.Play();
        }

        bgPlaying = !bgPlaying;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
