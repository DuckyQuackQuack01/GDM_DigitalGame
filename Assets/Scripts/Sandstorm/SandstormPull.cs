using UnityEngine;

public class SandstormGravity : MonoBehaviour
{
    public float gravityStrength = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();

            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 direction = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;
                rb.AddForce(direction * gravityStrength);
            }
        }
    }
}