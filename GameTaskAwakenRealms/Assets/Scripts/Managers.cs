using InputSystemExtension;
using UISystem;
using UnityEngine;
using UpdateSystem;

public class Managers : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    
    private UpdateManager _updateManager;
    private InputManager _inputManager;
    
    public static Managers Instance { get; private set; }
    public IUpdateRegistrar UpdateRegistrar => _updateManager;
    public IInputManager InputManager => _inputManager;
    public IUIManager UIManager => uiManager;
    
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
        uiManager.Awake();
    }

    // Todo: Added only for easy testing.
    //  Eventually it should be invoked after signal of the game's start is received from the UI.
    private void Start() => gameManager.StartGame();

    private void OnDestroy()
    {
        gameManager.Destroy();
        _inputManager.Destroy();
        uiManager.Destroy();
    }

    private void Update() => _updateManager.Update();
    private void FixedUpdate() => _updateManager.FixedUpdate();
    private void LateUpdate() => _updateManager.LateUpdate();
}