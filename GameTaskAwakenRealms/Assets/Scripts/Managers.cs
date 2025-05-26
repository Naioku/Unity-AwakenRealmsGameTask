using InputSystemExtension;
using Timer;
using UISystem;
using UnityEngine;
using UpdateSystem;

public class Managers : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    
    private UpdateManager _updateManager;
    private InputManager _inputManager;
    private TimerManager _timerManager;
    
    public static Managers Instance { get; private set; }
    public IUIManager UIManager => uiManager;
    public IUpdateRegistrar UpdateRegistrar => _updateManager;
    public IInputManager InputManager => _inputManager;
    public ITimerManager TimerManager => _timerManager;
    
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
        _timerManager = new TimerManager();

        _inputManager.Awake();
        _timerManager.Awake();
    }

    // Todo: Added only for easy testing.
    //  Eventually it should be invoked after signal of the game's start is received from the UI.
    private void Start() => gameManager.StartGame();
    
    // Todo: Added only for testing.
    [ContextMenu("Stop Game")]
    private void StopGame() => gameManager.StopGame();

    private void OnDestroy()
    {
        _inputManager.Destroy();
    }

    private void Update() => _updateManager.Update();
    private void FixedUpdate() => _updateManager.FixedUpdate();
    private void LateUpdate() => _updateManager.LateUpdate();
}