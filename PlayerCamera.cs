using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform cameraPosition;

    void Update()
    {
       transform.position =  cameraPosition.position;
    }
}
