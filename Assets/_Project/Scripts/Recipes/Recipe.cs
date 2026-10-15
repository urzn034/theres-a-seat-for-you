using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Cafe/Recipe")]
public class Recipe : ScriptableObject
{
    public float qteSpeed = 1.8f;
    public float perfectHalfWidth = 0.1f;
    public string drinkName;
    public List<string> ingredients;
    public int price = 10;
    public bool isUnlocked = false;
    public bool isModern = false;
}