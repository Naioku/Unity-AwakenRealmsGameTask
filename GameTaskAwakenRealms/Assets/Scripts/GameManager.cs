using InteractionSystem;
using UnityEngine;

[System.Serializable]
public class GameManager
{
    [SerializeField] private InteractionController interactionController;
    
    public void StartGame()
    {
        interactionController.Initialize(Camera.main);
        interactionController.StartInteracting();
        Managers.Instance.InputManager.GameplayMap.Enable();
    }

    public void Destroy()
    {
        interactionController.Destroy();
    }
}