using UnityEngine;

/// <summary>
/// Colócalo en el mismo GameObject que tiene el SpriteRenderer del fondo,
/// dentro de tu escena de lucha. Al iniciar, lee qué escenario eligió el
/// jugador (guardado en GameManager) y asigna el sprite correspondiente.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class StageBackgroundSetter : MonoBehaviour
{
    [Header("Asigna aquí los sprites de cada escenario, en el mismo orden que el enum Stage")]
    [SerializeField] private Sprite stage1Background;
    [SerializeField] private Sprite stage2Background;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplySelectedStageBackground();

        // Cambia la música a la del escenario elegido, si el AudioManager existe.
        if (AudioManager.Instance != null && GameManager.Instance != null)
        {
            AudioManager.Instance.PlayStageMusic(GameManager.Instance.SelectedStage);
        }
    }

    private void ApplySelectedStageBackground()
    {
        // Si por alguna razón se entra a esta escena sin pasar por el menú
        // (ej. probando directo desde el editor), GameManager.Instance será null.
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager no encontrado. Usando escenario por defecto (Stage1). " +
                              "Esto es normal si estás probando la escena de lucha directamente desde el editor.");
            _spriteRenderer.sprite = stage1Background;
            return;
        }

        switch (GameManager.Instance.SelectedStage)
        {
            case GameManager.Stage.Stage1:
                _spriteRenderer.sprite = stage1Background;
                break;
            case GameManager.Stage.Stage2:
                _spriteRenderer.sprite = stage2Background;
                break;
        }
    }
}