using UnityEngine.InputSystem;

namespace InputSystemExtension.ActionMaps
{
    public class GlobalMap : ActionMap, Controls.IGlobalActions
    {
        public IActionRegistrar OnClickInteractionData => _onClickInteractionData;
        
        private readonly ActionData _onClickInteractionData;
        
        public GlobalMap(Controls.GlobalActions actionMap)
        {
            this.actionMap = actionMap.Get();
            actionMap.SetCallbacks(this);
    
            _onClickInteractionData = new ActionData(actionMap.LClickInteraction);
        }
    
        public void OnLClickInteraction(InputAction.CallbackContext context) => _onClickInteractionData.Invoke(context.phase);
    }
}