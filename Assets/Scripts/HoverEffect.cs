using UnityEngine;

public class HoverEffect : MonoBehaviour
{
    public void onHoverEnterEffect(GameObject go)
    {
        go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    }

    public void onHoverExitEffect(GameObject go)
    {
        go.transform.localScale = Vector3.one;
    }
}
