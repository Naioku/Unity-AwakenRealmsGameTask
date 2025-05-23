using InputSystemExtension;
using UnityEngine;
using UpdateSystem;

public class Managers : MonoBehaviour
{
    public static Managers Instance { get; private set; }
    
    public IUpdateRegistrar UpdateRegistrar => _updateManager;
    public InputManager InputManager { get; private set; }
    
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
        InputManager = new InputManager();

        InputManager.Awake();
    }

    private void Update() => _updateManager.Update();
    private void FixedUpdate() => _updateManager.FixedUpdate();
    private void LateUpdate() => _updateManager.LateUpdate();
}