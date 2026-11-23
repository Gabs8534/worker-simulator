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

        Vector3 direcao = cam.transform.position - transform.position;

        // Ignora a diferença de altura
        direcao.y = 0;

        if (direcao.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direcao);
            transform.Rotate(0, 180, 0);
        }
    }
}