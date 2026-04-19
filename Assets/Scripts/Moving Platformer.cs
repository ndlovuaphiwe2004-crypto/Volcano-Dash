using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformer : MonoBehaviour
{
    [Tooltip("Movement speed of the platform")]
    public float speed;

    [Tooltip("Index of the point to start at")]
    public int startingPoint = 0;

    [Tooltip("Waypoints the platform will move between")]
    public Transform[] points;

    private int i;
    private readonly Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();

    void Start()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("MovingPlatformer: no points assigned.");
            return;
        }

        startingPoint = Mathf.Clamp(startingPoint, 0, points.Length - 1);
        i = startingPoint;
        transform.position = points[i].position;
    }

    void Update()
    {
        if (points == null || points.Length == 0)
        {
            return;
        }

        if (Vector2.Distance(transform.position, points[i].position) < 0.02f)
        {
            i = (i + 1) % points.Length;
        }

        transform.position = Vector2.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
    }

    // Unity calls this when a 2D collision begins.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.transform == null)
        {
            return;
        }

        // Only parent objects tagged "Player"
        if (collision.transform.CompareTag("Player"))
        {
            // Store original parent to restore later
            if (!originalParents.ContainsKey(collision.transform))
            {
                originalParents[collision.transform] = collision.transform.parent;
            }

            collision.transform.SetParent(transform);
        }
    }

    // Unity calls this when a 2D collision ends.
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision == null || collision.transform == null)
        {
            return;
        }

        if (collision.transform.CompareTag("Player"))
        {
            if (originalParents.TryGetValue(collision.transform, out var original))
            {
                collision.transform.SetParent(original);
                originalParents.Remove(collision.transform);
            }
            else
            {
                collision.transform.SetParent(null);
            }
        }
    }
}