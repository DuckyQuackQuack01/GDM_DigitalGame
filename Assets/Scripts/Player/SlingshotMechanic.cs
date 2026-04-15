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

    private Transform platformTransform;
    private Vector3 lastPlatformPosition;

    private Vector2 defaultGravity = new Vector2(0f, -9.8f);

    public CameraMovement cameraMovement;
    public SandstormGravity sandstorm;
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
        if (onPlatform && platformTransform != null)
        {
            Vector3 platformDelta = platformTransform.position - lastPlatformPosition;
            transform.position += platformDelta;

            lastPlatformPosition = platformTransform.position;
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
        platformTransform = null;

        if (currentPlatform != null)
        {
            currentPlatform.RestoreSpeed();
            currentPlatform = null;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = mousePos - startPos;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        Vector2 launchDir = -dragVector;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = launchDir * launchPower;

        foreach (var dot in dots)
            dot.SetActive(false);
    }

    void DrawTrajectory(Vector2 initialVeclocity)
    {
        Vector2 simPosition = startPos;
        Vector2 simVelocity = initialVeclocity;
        float timeStep = dotSpacing;
       
        for (int i = 0; i < dotCount; i++)
        {
            Vector2 totalAcceleration = Physics2D.gravity * rb.gravityScale;
            
            if(sandstorm != null)
            {
                Vector2 force = sandstorm.GetForceAtPosition(simPosition);
                totalAcceleration += force / rb.mass;
            }

            simVelocity += totalAcceleration * timeStep;
            simPosition += simVelocity * timeStep;

            dots[i].transform.position = simPosition;
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

            onPlatform = false;
            platformTransform = null;

            if (currentPlatform != null)
            {
                currentPlatform.RestoreSpeed();
                currentPlatform = null;
            }

            cameraMovement.ResetCameraRotation();
            Physics2D.gravity = defaultGravity;

            ScoreManager.ResetScore();
            LevelEntryState.playIntro = false;
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

            onPlatform = true;

            currentPlatform = other.GetComponentInParent<MovingPlatform>();
            if (currentPlatform != null)
            {
                currentPlatform.SlowDown();
                platformTransform = currentPlatform.transform;
                lastPlatformPosition = platformTransform.position;
            }
            else
            {
                platformTransform = other.transform;
                lastPlatformPosition = platformTransform.position;
            }

            startPos = transform.position;

            ScoreManager.AddPoint();
        }
    }

    void HideFellPopup()
    {
        fellPopup.SetActive(false);
    }
}




