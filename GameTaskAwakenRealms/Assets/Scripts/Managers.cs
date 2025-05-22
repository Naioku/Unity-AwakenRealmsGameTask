using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Instance { get; private set; }
        
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}