using UnityEngine;
using UpdateSystem;

public class Managers : MonoBehaviour
{
    public static Managers Instance { get; private set; }
    
    public IUpdateRegistrar UpdateRegistrar => _updateManager;
    
    private UpdateManager _updateManager;
        
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
        
        _updateManager = new UpdateManager();
    }

    private void Update() => _updateManager.Update();
    private void FixedUpdate() => _updateManager.FixedUpdate();
    private void LateUpdate() => _updateManager.LateUpdate();
}