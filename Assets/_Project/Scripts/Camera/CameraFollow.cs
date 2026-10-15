using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float offsetY = 3f;
    [SerializeField] private bool lockY = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-26f, -15.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(26f, 15.5f);
    private Camera cam;
    private float startY;

    void Awake()
    {
        cam = GetComponent<Camera>();
        startY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = Mathf.Clamp(target.position.x, minBounds.x + halfW, maxBounds.x - halfW);
        float y = lockY
            ? startY
            : Mathf.Clamp(target.position.y + offsetY, minBounds.y + halfH, maxBounds.y - halfH);
        transform.position = new Vector3(x, y, transform.position.z);
    }
}