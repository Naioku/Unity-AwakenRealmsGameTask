using UnityEngine.InputSystem;

namespace InputSystemExtension.ActionMaps
{
    public class GameplayMap : ActionMap, Controls.IGameplayActions
    {
        public IActionRegistrar OnLClickInteractionData => _onLClickInteractionData;
        
        private readonly ActionData _onLClickInteractionData;
        
        public GameplayMap(Controls.GameplayActions actionMap)
        {
            this.actionMap = actionMap.Get();
            actionMap.SetCallbacks(this);
    
            _onLClickInteractionData = new ActionData(actionMap.LClickInteraction);
        }
    
        public void OnLClickInteraction(InputAction.CallbackContext context) => _onLClickInteractionData.Invoke(context.phase);
    }
}