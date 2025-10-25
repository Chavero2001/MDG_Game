using UnityEngine;

public class turnOff : MonoBehaviour
{
    public float radius = 20f;
    public GameObject object_to_deactivate;
    public GameObject player;
    // Update is called once per frame
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    void Update()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 objPos = transform.position;
        float dx = playerPos.x - objPos.x;
        float dz = playerPos.z - objPos.z;
        bool inRange = Mathf.Abs(dx) <= radius && Mathf.Abs(dz) <= radius;
        if (inRange) { object_to_deactivate.SetActive(true); } else {  object_to_deactivate.SetActive(false);}
    }
}
