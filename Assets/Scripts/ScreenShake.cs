using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public float shakeAmount = 50000f;      // Higher = more intense shake
    public float shakeSpeed = 50000f;       // How fast the shake moves
    private bool isShaking = false;
    private Vector3 originalPos;
    private Coroutine activeShakeCoroutine;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    // Start intense continuous shake
    public void StartShake()
    {
        if (!isShaking)
        {
            isShaking = true;
            if (activeShakeCoroutine != null)
                StopCoroutine(activeShakeCoroutine);
            activeShakeCoroutine = StartCoroutine(IntenseShake());
        }
    }

    // Stop shaking
    public void StopShake()
    {
        isShaking = false;
        if (activeShakeCoroutine != null)
        {
            StopCoroutine(activeShakeCoroutine);
            activeShakeCoroutine = null;
        }
        // Return to original position
        transform.localPosition = originalPos;
    }

    IEnumerator IntenseShake()
    {
        float time = 0f;

        while (isShaking)
        {
            // Create intense, rapid shaking using sine waves
            float offsetX = Mathf.Sin(time * shakeSpeed) * shakeAmount;
            float offsetY = Mathf.Cos(time * shakeSpeed * 1.3f) * shakeAmount;
            float offsetZ = Mathf.Sin(time * shakeSpeed * 0.7f) * (shakeAmount * 0.5f);

            Vector3 shakeOffset = new Vector3(offsetX, offsetY, offsetZ);
            transform.localPosition = originalPos + shakeOffset;

            time += Time.deltaTime;
            yield return null;
        }
    }

    // For 2D collision detection (stays shaking while touching)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StartShake();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StopShake();
        }
    }

    // For 3D collision detection
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StartShake();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StopShake();
        }
    }

    // For trigger zones (if using IsTrigger)
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            StartShake();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            StopShake();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            StartShake();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            StopShake();
        }
    }
}