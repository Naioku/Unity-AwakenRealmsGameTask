using UnityEngine.InputSystem;

namespace InputSystemExtension.ActionMaps
{
    public class GameplayMap : ActionMap, Controls.IGameplayActions
    {
        public IActionRegistrar OnClickInteractionData => _onClickInteractionData;
        
        private readonly ActionData _onClickInteractionData;
        
        public GameplayMap(Controls.GameplayActions actionMap)
        {
            this.actionMap = actionMap.Get();
            actionMap.SetCallbacks(this);
    
            _onClickInteractionData = new ActionData(actionMap.LClickInteraction);
        }
    
        public void OnLClickInteraction(InputAction.CallbackContext context) => _onClickInteractionData.Invoke(context.phase);
    }
}