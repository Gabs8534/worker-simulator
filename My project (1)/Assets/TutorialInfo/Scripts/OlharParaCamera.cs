using UnityEngine;

public class OlharParaCamera : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        transform.LookAt(cam.transform);
    }
}
