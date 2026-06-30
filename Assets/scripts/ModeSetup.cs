using UnityEngine;

public class ModeSetup : MonoBehaviour
{
    void Awake()
    {
        PlayerController pc = GetComponent<PlayerController>();
        EnemyAI ai = GetComponent<EnemyAI>();

        if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
        {
            if (pc != null) pc.enabled = false;
            if (ai != null) ai.enabled = true;
        }
        else
        {
            if (pc != null) pc.enabled = true;
            if (ai != null) ai.enabled = false;
        }
    }
}
