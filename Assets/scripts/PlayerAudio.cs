using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip kickSound;
    [SerializeField] private AudioClip damagedSound;
    [SerializeField] private AudioClip walkSound; 

    // Métodos públicos que cualquier otro script o animación puede llamar
    public void PlayJump() => PlaySound(jumpSound);
    public void PlayPunch() => PlaySound(punchSound);
    public void PlayKick() => PlaySound(kickSound);
    public void PlayDamage() => PlaySound(damagedSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayHit()
    {
        if (damagedSound != null)
        {
            audioSource.PlayOneShot(damagedSound); 
        }
    }

    public void StartWalking()
    {
        if (audioSource.clip == walkSound && audioSource.isPlaying) return;

        audioSource.clip = walkSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopWalking()
    {
        if (audioSource.clip == walkSound && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }
}