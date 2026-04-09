using UnityEngine;
using UnityEngine.SceneManagement;

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
    private Vector2 originalStartPos;

    private bool isDragging = false;

    private bool onPlatform = false;
    private MovingPlatform currentPlatform;

    private Vector2 defaultGravity = new Vector2(0f, -9.8f);

    public CameraMovement cameraMovement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        originalStartPos = transform.position;
        startPos = originalStartPos;

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
        rb.angularVelocity = 0f;
    }

    void Update()
    {
        if (onPlatform)
        {
            startPos = transform.position;
        }

        if (isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = mousePos - startPos;

            dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

            Vector2 launchDir = -dragVector;
            Vector2 velocity = launchDir * launchPower;

            DrawTrajectory(velocity);
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Physics2D.gravity = defaultGravity;
            ScoreManager.ResetScore();
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        onPlatform = false;

        if (currentPlatform != null)
        {
            currentPlatform.RestoreSpeed();
            currentPlatform = null;
        }

        transform.SetParent(null);

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = mousePos - startPos;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        Vector2 launchDir = -dragVector;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = launchDir * launchPower;

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
        if (collision.collider.CompareTag("Ground"))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            transform.position = originalStartPos;

            rb.bodyType = RigidbodyType2D.Kinematic;

            fellPopup.SetActive(true);
            Invoke("HideFellPopup", 1.5f);

            startPos = originalStartPos;

            transform.SetParent(null);

            onPlatform = false;

            if (currentPlatform != null)
            {
                currentPlatform.RestoreSpeed();
                currentPlatform = null;
            }

            cameraMovement.ResetCameraRotation();
            Physics2D.gravity = defaultGravity;

            // ? RESET SCORE WHEN HITTING GROUND
            ScoreManager.ResetScore();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlatformTop"))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.bodyType = RigidbodyType2D.Kinematic;

            transform.position += new Vector3(0, 0.02f, 0);

            transform.SetParent(other.transform);

            onPlatform = true;
            startPos = transform.position;

            // ? Add a point
            ScoreManager.AddPoint();

            currentPlatform = other.GetComponentInParent<MovingPlatform>();
            if (currentPlatform != null)
                currentPlatform.SlowDown();
        }
    }

    void HideFellPopup()
    {
        fellPopup.SetActive(false);
    }
}




