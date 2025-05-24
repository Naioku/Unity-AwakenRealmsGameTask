using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class Interaction : MonoBehaviour, IInteractionInvoker
    {
        private readonly Dictionary<Enums.InteractionType, Dictionary<Enums.InteractionState, Action<InteractionDataArgs>>> _interactionTypeLookup = new();

        private void Awake() => InitializeActions();

        private void InitializeActions()
        {
            foreach (Enums.InteractionType actionType in Enum.GetValues(typeof(Enums.InteractionType)))
            {
                var initialActions = new Dictionary<Enums.InteractionState, Action<InteractionDataArgs>>
                {
                    {Enums.InteractionState.EnterType, null},
                    {Enums.InteractionState.Tick, null},
                    {Enums.InteractionState.ExitType, null},
                    {Enums.InteractionState.EnterInteraction, null},
                    {Enums.InteractionState.ExitInteraction, null}
                };
                
                _interactionTypeLookup.Add(actionType, initialActions);
            }
        }
    
        public void SetAction(
            Enums.InteractionType interactionType,
            Enums.InteractionState interactionState,
            Action<InteractionDataArgs> action)
            => _interactionTypeLookup[interactionType][interactionState] = action;

        public void Interact(InteractionDataSystem interactionDataSystem, InteractionDataArgs interactionDataArgs)
        {
            if (!_interactionTypeLookup.TryGetValue(
                    interactionDataSystem.InteractionType,
                    out var actions)) return;
            if (!actions.ContainsKey(interactionDataSystem.InteractionState)) return;
            
            actions[interactionDataSystem.InteractionState]?.Invoke(interactionDataArgs);
        }
    }
}