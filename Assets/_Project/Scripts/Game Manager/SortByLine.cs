using UnityEngine;

public class SortByLine : MonoBehaviour
{
    public Transform line;
    public int orderAbove = 10;
    public int orderBelow = 13;
    SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void LateUpdate()
    {
        sr.sortingOrder = transform.position.y > line.position.y ? orderAbove : orderBelow;
    }
}