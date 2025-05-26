using Dice;
using DragNDropSystem;
using InteractionSystem;
using UnityEngine;

[System.Serializable]
public class GameManager
{
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private DragNDropController dragNDropController;
    [SerializeField] private DieController selectedDie; // Todo: SpawnManager.
    
    private int _scoreLast;
    private int _scoreTotal;

    public void StartGame()
    {
        Camera mainCamera = Camera.main;
        InitInteraction(mainCamera);
        dragNDropController.Initialize(mainCamera);
        Managers.Instance.InputManager.GameplayMap.OnLClickInteractionData.Canceled += HandleLClickCanceled;
        Managers.Instance.InputManager.GameplayMap.Enable();
        
        Managers.Instance.UIManager.StartGame();
        Managers.Instance.UIManager.OnRoll += HandleUIRoll;
        selectedDie.OnScoreDetected += HandleDieScoreDetected;
        selectedDie.OnStateChanged += HandleDieStateChanged;
    }

    // Todo: Added for future needs.
    //  Use it when the player moves back to the main menu.
    public void StopGame()
    {
        Managers.Instance.UIManager.StopGame();
        interactionController.Reset();
        dragNDropController.Reset();
        Managers.Instance.InputManager.GameplayMap.OnLClickInteractionData.Canceled -= HandleLClickCanceled;
        Managers.Instance.UIManager.OnRoll -= HandleUIRoll;
        selectedDie.OnScoreDetected -= HandleDieScoreDetected;
        selectedDie.OnStateChanged -= HandleDieStateChanged;
    }

    private void InitInteraction(Camera mainCamera)
    {
        interactionController.Initialize(mainCamera);
        interactionController.SetAction(Enums.InteractionType.Click, Enums.InteractionState.EnterType, HandleClickEnterType);
        interactionController.StartInteracting();
    }

    private void HandleClickEnterType(MonoBehaviour interactionOwner, InteractionDataArgs dataArgs)
    {
        if (interactionOwner is not IDraggable draggable) return;
        
        dragNDropController.Drag(draggable);
    }

    private void HandleLClickCanceled() => dragNDropController.Drop();
    private void HandleUIRoll() => selectedDie.PerformAutoThrow();

    private void HandleDieScoreDetected(int score)
    {
        _scoreLast = score;
        _scoreTotal += score;
        Managers.Instance.UIManager.SaveScore(score, _scoreTotal);
    }

    private void HandleDieStateChanged(Enums.DieState dieState)
    {
        Managers.Instance.UIManager.SetRollingActive(dieState == Enums.DieState.Idle);
        if (dieState == Enums.DieState.Throw ||
            dieState == Enums.DieState.AutoThrow ||
            dieState == Enums.DieState.ScoreDetection)
        {
            Managers.Instance.UIManager.SetResult("?");
        }
        else
        {
            Managers.Instance.UIManager.SetResult(_scoreLast.ToString());
        }
    }
}