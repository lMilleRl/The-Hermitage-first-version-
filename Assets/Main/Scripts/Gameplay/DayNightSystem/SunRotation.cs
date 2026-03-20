using UnityEngine;

public class SunRotation : MonoBehaviour
{
    public float rotationSpeed = 1f; // градусов в секунду

    void Update()
    {
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }
}
