using InteractionSystem;
using UnityEngine;

[System.Serializable]
public class GameManager
{
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private DragNDropController dragNDropController;

    public void StartGame()
    {
        Camera mainCamera = Camera.main;
        InitInteraction(mainCamera);
        dragNDropController.Initialize(mainCamera);
        Managers.Instance.InputManager.GameplayMap.OnLClickInteractionData.Canceled += HandleLClickCanceled;
        Managers.Instance.InputManager.GameplayMap.Enable();
    }

    public void Destroy()
    {
        interactionController.Destroy();
        dragNDropController.Destroy();
        Managers.Instance.InputManager.GameplayMap.OnLClickInteractionData.Canceled -= HandleLClickCanceled;
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
}