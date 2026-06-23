using UnityEngine;

public class FighterStats : MonoBehaviour
{
    [Header("Character")]
    public string characterName;

    [Header("Stats")]
    public int maxHealth = 100;
    public float power = 1f;
    public float defense = 1f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
}