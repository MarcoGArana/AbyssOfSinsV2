using UnityEngine;

/// <summary>
/// Singleton que controla la m�sica de fondo del juego.
/// Col�calo en un GameObject vac�o llamado "AudioManager" SOLO en la primera
/// escena que se carga (por ejemplo, MainMenu). Gracias a DontDestroyOnLoad,
/// sobrevivir� los cambios de escena y no se duplicar�.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("M�sica")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip[] stageMusic; // Una pista por cada Stage del GameManager (mismo orden que el enum)
    [SerializeField] private float defaultVolume = 0.5f;
    [SerializeField] private float fadeDuration = 0.6f;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        // Patr�n Singleton: si ya existe una instancia, esta se destruye.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = defaultVolume;
    }

    private void Start()
    {
        // Reproduce la m�sica del men� al iniciar el juego.
        if (menuMusic != null)
            PlayMusic(menuMusic);
    }

    /// <summary>Reproduce un clip con fade-in/out simple. Si ya est� sonando, no lo reinicia.</summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || (_audioSource.clip == clip && _audioSource.isPlaying)) return;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeToNewClip(clip));
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);

    /// <summary>Reproduce la m�sica correspondiente al escenario indicado.</summary>
    public void PlayStageMusic(GameManager.Stage stage)
    {
        int index = (int)stage;

        if (stageMusic == null || index < 0 || index >= stageMusic.Length || stageMusic[index] == null)
        {
            Debug.LogWarning($"No hay m�sica asignada para el escenario {stage} en AudioManager. " +
                              "Revisa el arreglo 'stageMusic' en el Inspector.");
            return;
        }

        PlayMusic(stageMusic[index]);
    }

    public void SetVolume(float volume) => _audioSource.volume = Mathf.Clamp01(volume);

    public void Stop() => _audioSource.Stop();

    private System.Collections.IEnumerator FadeToNewClip(AudioClip newClip)
    {
        float startVolume = _audioSource.volume;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        _audioSource.Stop();
        _audioSource.clip = newClip;
        _audioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            _audioSource.volume = Mathf.Lerp(0f, defaultVolume, t / fadeDuration);
            yield return null;
        }

        _audioSource.volume = defaultVolume;
    }
}
