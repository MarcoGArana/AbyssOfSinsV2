using UnityEngine;

public enum AttackType
{
    High,
    Low,
    Air
}

[System.Serializable]
public class AttackData
{
    public string attackName;

    public int damage;

    public AttackType attackType;
}