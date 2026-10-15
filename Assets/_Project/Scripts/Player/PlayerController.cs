using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool lockVertical = false;
    public bool canMove = true;

    private Animator animator;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private Vector2 lastDir = Vector2.down;
    private Vector2 dir;

    void Awake()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (lockVertical) lastDir = Vector2.right;
    }

    void Update()
    {
        float x = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        float y = (canMove && !lockVertical) ? Input.GetAxisRaw("Vertical") : 0f;
        if (x != 0) y = 0;

        dir = new Vector2(x, y);

        bool walking = dir != Vector2.zero;
        if (walking) lastDir = dir;

        animator.SetBool("IsWalking", walking);
        animator.SetFloat("MoveX", lastDir.x);
        animator.SetFloat("MoveY", lastDir.y);

        sprite.flipX = lastDir.x < 0;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    public void SetFacing(Vector2 facing)
    {
        lastDir = facing;
    }
}