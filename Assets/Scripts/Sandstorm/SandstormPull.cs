using UnityEngine;

public class SandstormPull : MonoBehaviour
{
    public float pullStrength = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();

            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 direction = (transform.position - other.transform.position).normalized;
                float distance = Mathf.Max(Vector2.Distance(transform.position, other.transform.position), 0.5f);

                float force = pullStrength / (distance * distance);

                rb.AddForce(direction * force);
            }
        }
    }
}