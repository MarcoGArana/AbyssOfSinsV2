using UnityEngine;

public class DetectorPrueba : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("<color=red>GOLPE DETECTADO :v</color>");
    }
}