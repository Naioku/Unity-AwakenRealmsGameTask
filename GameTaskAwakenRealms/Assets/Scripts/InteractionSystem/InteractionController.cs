using System;
using System.Collections.Generic;
using InputSystemExtension.ActionMaps;
using UnityEngine;

namespace InteractionSystem
{
    [Serializable]
    public class InteractionController
    {
        [SerializeField] private float interactionRange = 99;
        [SerializeField] private LayerMask layerMask = 1 << 6;
        
        private Camera _mainCamera;
        private Dictionary<Enums.InteractionType, Dictionary<Enums.InteractionState, Action<MonoBehaviour, InteractionDataArgs>>> _interactionTypeLookup = new();
        private InteractionInvoker _currentInteraction;
        private RaycastHit _currentHitInfo;
            
        /// <summary>
        /// Interaction type, which should be currently used like: Hover, Click, Key. Should be set by input.
        /// </summary>
        private Enums.InteractionType _currentInteractionType = DefaultInteractionType;

        private const Enums.InteractionType DefaultInteractionType = Enums.InteractionType.Hover;

        public void Initialize(Camera camera)
        {
            _mainCamera = camera;
            InitializeActions();
            AddInput();
        }

        public void Reset()
        {
            _mainCamera = null;
            _interactionTypeLookup = new Dictionary<Enums.InteractionType, Dictionary<Enums.InteractionState, Action<MonoBehaviour, InteractionDataArgs>>>();
            _currentInteraction = null;
            StopInteracting();
            RemoveInput();
        }

        public void SetAction(
            Enums.InteractionType interactionType,
            Enums.InteractionState interactionState,
            Action<MonoBehaviour, InteractionDataArgs> action)
            => _interactionTypeLookup[interactionType][interactionState] = action;

        public void StartInteracting()
        {
            Managers.Instance.UpdateRegistrar.RegisterOnUpdate(PerformInteraction);
        }

        public void StopInteracting()
        {
            Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(PerformInteraction);
        }

        private void InitializeActions()
        {
            foreach (Enums.InteractionType actionType in Enum.GetValues(typeof(Enums.InteractionType)))
            {
                var initialActions = new Dictionary<Enums.InteractionState, Action<MonoBehaviour, InteractionDataArgs>>
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

        private void AddInput()
        {
            IActionRegistrar onClickInteractionData = Managers.Instance.InputManager.GameplayMap.OnLClickInteractionData;
            onClickInteractionData.Performed += StartClickInteraction;
            onClickInteractionData.Canceled += StopClickInteraction;
        }
        
        private void RemoveInput()
        {
            IActionRegistrar onClickInteractionData = Managers.Instance.InputManager.GameplayMap.OnLClickInteractionData;
            onClickInteractionData.Performed -= StartClickInteraction;
            onClickInteractionData.Canceled -= StopClickInteraction;
        }

        private void StartClickInteraction() => SwitchInteractionType(Enums.InteractionType.Click);
        private void StopClickInteraction() => SwitchInteractionType(DefaultInteractionType);

        private void PerformInteraction()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Managers.Instance.InputManager.CursorPosition);
            InteractionInvoker currentlyCheckedInteraction =
                !Physics.Raycast(ray, out _currentHitInfo, interactionRange, layerMask)
                    ? null
                    : _currentHitInfo.collider.GetComponent<InteractionInvoker>();
            
            if (currentlyCheckedInteraction != _currentInteraction)
            {
                SwitchInteraction(currentlyCheckedInteraction);
            }
            
            Interact(Enums.InteractionState.Tick);
        }

        private void SwitchInteraction(InteractionInvoker currentlyCheckedInteraction)
        {
            Interact(Enums.InteractionState.ExitInteraction);
            _currentInteraction = currentlyCheckedInteraction;
            if (_currentInteraction)
            {
                Interact(Enums.InteractionState.EnterInteraction);
            }
        }

        private void SwitchInteractionType(Enums.InteractionType interactionType)
        {
            Interact(Enums.InteractionState.ExitType);
            _currentInteractionType = interactionType;
            Interact(Enums.InteractionState.EnterType);
        }
        
        private void Interact(Enums.InteractionState interactionState)
        {
            if (!_currentInteraction) return;

            InteractionDataSystem interactionDataSystem = new()
            {
                InteractionType = _currentInteractionType,
                InteractionState = interactionState
            };
            
            InteractionDataArgs interactionDataArgs = new()
            {
                HitInfo = _currentHitInfo,
            };
            
            MonoBehaviour interactionOwner = _currentInteraction.Interact(interactionDataSystem, interactionDataArgs);
            OnInteraction(interactionOwner, interactionDataSystem, interactionDataArgs);
            
            if (interactionState == Enums.InteractionState.ExitInteraction)
            {
                _currentInteraction = null;
            }
        }
        
        private void OnInteraction(MonoBehaviour interactionOwner, InteractionDataSystem interactionDataSystem, InteractionDataArgs interactionDataArgs)
        {
            if (!interactionOwner) return;
            if (!_interactionTypeLookup.TryGetValue(
                    interactionDataSystem.InteractionType,
                    out var actions)) return;
            if (!actions.ContainsKey(interactionDataSystem.InteractionState)) return;
            
            actions[interactionDataSystem.InteractionState]?.Invoke(interactionOwner, interactionDataArgs);
        }
    }
}