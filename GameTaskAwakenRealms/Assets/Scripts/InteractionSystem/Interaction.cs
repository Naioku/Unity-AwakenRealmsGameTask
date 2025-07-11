﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class Interaction : InteractionInvoker
    {
        private readonly Dictionary<Enums.InteractionType, Dictionary<Enums.InteractionState, Action<InteractionDataArgs>>> _interactionTypeLookup = new();

        public bool Enabled { private get; set; } = true;
        public MonoBehaviour Owner { private get; set; }

        private void Awake() => InitializeActions();
        
        public void SetAction(
            Enums.InteractionType interactionType,
            Enums.InteractionState interactionState,
            Action<InteractionDataArgs> action)
            => _interactionTypeLookup[interactionType][interactionState] = action;

        public override MonoBehaviour Interact(InteractionDataSystem interactionDataSystem, InteractionDataArgs interactionDataArgs)
        {
            if (!Enabled) return null;
            if (!_interactionTypeLookup.TryGetValue(
                    interactionDataSystem.InteractionType,
                    out var actions)) return null;
            if (!actions.ContainsKey(interactionDataSystem.InteractionState)) return null;
            
            actions[interactionDataSystem.InteractionState]?.Invoke(interactionDataArgs);
            return Owner;
        }

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
    }
}