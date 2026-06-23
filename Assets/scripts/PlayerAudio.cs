using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip kickSound;

    // Métodos públicos que cualquier otro script o animación puede llamar
    public void PlayJump() => PlaySound(jumpSound);
    public void PlayPunch() => PlaySound(punchSound);
    public void PlayKick() => PlaySound(kickSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}