using UnityEngine;

public class UIButtonSoundHandler : MonoBehaviour
{
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private float volume = 1f;

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null && SoundFxManager.Instance != null)
        {
            SoundFxManager.Instance.PlaySoundFXClip(buttonClickSound, transform, volume);
        }
    }
}