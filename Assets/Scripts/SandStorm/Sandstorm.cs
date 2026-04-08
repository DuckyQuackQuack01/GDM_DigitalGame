using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 0f;
        }
    }
}