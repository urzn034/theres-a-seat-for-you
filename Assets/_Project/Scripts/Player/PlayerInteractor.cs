using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private Vector2 center = new Vector2(0f, 0.5f);

    public IInteractable Current { get; private set; }

    void Update()
    {
        Current = FindNearest();
        if (Current != null && Input.GetKeyDown(KeyCode.E))
            Current.Interact();
    }

    IInteractable FindNearest()
    {
        Vector2 origin = (Vector2)transform.position + center;
        var hits = Physics2D.OverlapCircleAll(origin, radius);
        IInteractable best = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            var i = h.GetComponentInParent<IInteractable>();
            if (i == null) continue;
            float d = ((Vector2)h.transform.position - origin).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + center, radius);
    }
}