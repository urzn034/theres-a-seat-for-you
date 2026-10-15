using UnityEngine;

[DefaultExecutionOrder(100)]
public class Parallax : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [Range(0f, 1f)][SerializeField] private float factor = 0.5f;
    [SerializeField] private float startCamX = 0f;
    private float startX;

    void Awake()
    {
        startX = transform.position.x;
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main.transform;
        float x = startX + (cam.position.x - startCamX) * factor;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}