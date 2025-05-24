using InputSystemExtension;
using UnityEngine;
using UnityEngine.Serialization;
using UpdateSystem;

public class Managers : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    private UpdateManager _updateManager;
    private InputManager _inputManager;
    
    public static Managers Instance { get; private set; }
    public IUpdateRegistrar UpdateRegistrar => _updateManager;
    public IInputManager InputManager => _inputManager;
    
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
        _inputManager = new InputManager();

        _inputManager.Awake();
    }

    // Todo: Added only for easy testing.
    //  Eventually it should be invoked after signal of the game's start is received from the UI.
    private void Start() => gameManager.StartGame();

    private void OnDestroy()
    {
        gameManager.Destroy();
        _inputManager.Destroy();
    }

    private void Update() => _updateManager.Update();
    private void FixedUpdate() => _updateManager.FixedUpdate();
    private void LateUpdate() => _updateManager.LateUpdate();
}