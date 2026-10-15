using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            player.canMove = true;
            player.SetFacing(Vector2.left); 
        }
    }
}