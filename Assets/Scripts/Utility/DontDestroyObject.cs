using UnityEngine;

public sealed class DontDestroyObject : MonoBehaviour
{
    private void Awake()
    {
        var objects = FindObjectsByType(GetType());

        if (objects.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}