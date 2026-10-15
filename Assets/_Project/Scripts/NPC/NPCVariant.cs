using UnityEngine;

[System.Serializable]
public class NPCVariant
{
    public string name;
    public Sprite idle;
    public Sprite[] walk;
    public RuntimeAnimatorController animator;
    public Sprite sit, drinkA, drinkB;
}