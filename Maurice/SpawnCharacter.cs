using UnityEngine;

public class SpawnCharacter : MonoBehaviour
{
    public GameObject SpawnCharacterPrefab;
    public void spawn()
    {
        SpawnCharacterPrefab.SetActive(true);
    }
}
