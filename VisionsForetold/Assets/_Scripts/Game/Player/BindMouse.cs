using UnityEngine;

public class BindMouse : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = transform.position.z;
        transform.position = worldPosition;
    }
}
