using UnityEngine;



public class SlingshotMechanic : MonoBehaviour
{
    public GameObject fellPopup;

    [Header("Slingshot Settings")]
    public float maxDragDistance = 3f;
    public float launchPower = 10f;

    [Header("Trajectory Dots")]
    public GameObject dotPrefab;
    public int dotCount = 40;
    public float dotSpacing = 0.02f;

    private GameObject[] dots;
    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        // Create dots
        dots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            dots[i] = Instantiate(dotPrefab, startPos, Quaternion.identity);
            dots[i].SetActive(false);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = mousePos - startPos;

        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);
        transform.position = startPos + dragVector;

        Vector2 launchDir = startPos - (Vector2)transform.position;
        Vector2 velocity = launchDir * launchPower;

        DrawTrajectory(velocity);
    }

    void OnMouseUp()
    {
        isDragging = false;

        Vector2 launchDir = startPos - (Vector2)transform.position;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = launchDir * launchPower;

        // Hide dots
        foreach (var dot in dots)
            dot.SetActive(false);
    }

    void DrawTrajectory(Vector2 velocity)
    {
        for (int i = 0; i < dotCount; i++)
        {
            float t = i * dotSpacing;

            Vector2 pos =
                startPos +
                velocity * t +
                0.5f * Physics2D.gravity * t * t;

            dots[i].transform.position = pos;
            dots[i].SetActive(true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
   

        // MISS: Hit the ground ? reset
        if (collision.collider.CompareTag("Ground"))
        {
            Debug.Log("Missed the platform!");

            rb.linearVelocity = Vector2.zero;
            transform.position = startPos;
            rb.bodyType = RigidbodyType2D.Kinematic;

            fellPopup.SetActive(true);
            Invoke("HideFellPopup", 1.5f);
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlatformTop"))
        {
            Debug.Log("Landed on TOP!");
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void HideFellPopup()
    {
        fellPopup.SetActive(false);
    }
}