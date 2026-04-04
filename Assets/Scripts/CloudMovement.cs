using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public float speed;
    public int startingPoint;
    public Transform[] points;

    private int i;

    void Start()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("CloudsMovement: no points assigned.");
            return;
        }

        i = Mathf.Clamp(startingPoint, 0, points.Length - 1);
        transform.position = points[i].position;
    }

    void Update()
    {
        if (points == null || points.Length == 0)
            return;

        transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, points[i].position) < 0.02f)
        {
            i++;
            if (i >= points.Length)
            {
                i = 0;
            }
        }
    }
}
