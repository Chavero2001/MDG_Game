using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
        if (cameraPosition.position.z >= 9 && cameraPosition.position.z <= 140)
        {
            Vector3 target = new Vector3(transform.position.x, transform.position.y, cameraPosition.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        }

        if (cameraPosition.position.x >= 17 && cameraPosition.position.x <= 133)
        {
            Vector3 target = new Vector3(cameraPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        }
    }
}
