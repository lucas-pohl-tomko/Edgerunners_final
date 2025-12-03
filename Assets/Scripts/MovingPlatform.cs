using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Rigidbody2D rb;

    [Header("Move Properties")]
    [SerializeField] bool moveRight = true;
    [SerializeField] bool moveUpAndDown = false;
    [SerializeField] bool spin = false;
    [SerializeField] float speed = 2f;

    [Header("Limits")]
    [SerializeField] float horizontalLength = 3f;
    [SerializeField] float elevationUpDown = 2f;

    Vector2 startPos;
    int direction = 1;
    public Vector2 PlatformVelocity { get; private set; }
    Vector2 lastPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;
        lastPos = rb.position;
    }

    void FixedUpdate()
    {
        Vector2 newPos = rb.position;

        Vector2 currentPos = rb.position;

        PlatformVelocity = (currentPos - lastPos) / Time.fixedDeltaTime;

        lastPos = currentPos;

        if (moveRight)
        {
            newPos.x += direction * speed * Time.fixedDeltaTime;

            if (Mathf.Abs(newPos.x - startPos.x) >= horizontalLength)
                direction *= -1;
        }

        if (moveUpAndDown)
        {
            newPos.y += direction * speed * Time.fixedDeltaTime;

            if (Mathf.Abs(newPos.y - startPos.y) >= elevationUpDown)
                direction *= -1;
        }

        if (spin)
        {
            transform.Rotate(Vector3.up * 5f * Time.fixedDeltaTime);
        }
        rb.MovePosition(newPos);
    }
}
