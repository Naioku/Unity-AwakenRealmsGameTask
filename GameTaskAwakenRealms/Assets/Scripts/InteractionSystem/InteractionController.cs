using System;
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
        private IInteractionInvoker _currentInteraction;
        private RaycastHit _currentHitInfo;
            
        /// <summary>
        /// Interaction type, which should be currently used like: Hover, Click, Key. Should be set by input.
        /// </summary>
        private Enums.InteractionType _currentInteractionType = DefaultInteractionType;
        private const Enums.InteractionType DefaultInteractionType = Enums.InteractionType.Hover;

        public void Initialize(Camera camera)
        {
            _mainCamera = camera;
            AddInput();
        }

        public void Destroy()
        {
            StopInteracting();
            RemoveInput();
        }

        public void StartInteracting()
        {
            Managers.Instance.UpdateRegistrar.RegisterOnUpdate(PerformInteraction);
        }

        public void StopInteracting()
        {
            Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(PerformInteraction);
        }

        private void AddInput()
        {
            IActionRegistrar onClickInteractionData = Managers.Instance.InputManager.GameplayMap.OnClickInteractionData;
            onClickInteractionData.Performed += StartClickInteraction;
            onClickInteractionData.Canceled += StopClickInteraction;
        }
        
        private void RemoveInput()
        {
            IActionRegistrar onClickInteractionData = Managers.Instance.InputManager.GameplayMap.OnClickInteractionData;
            onClickInteractionData.Performed -= StartClickInteraction;
            onClickInteractionData.Canceled -= StopClickInteraction;
        }

        private void StartClickInteraction() => SwitchInteractionType(Enums.InteractionType.Click);
        private void StopClickInteraction() => SwitchInteractionType(DefaultInteractionType);

        private void PerformInteraction()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Managers.Instance.InputManager.CursorPosition);
            IInteractionInvoker currentlyCheckedInteraction =
                !Physics.Raycast(ray, out _currentHitInfo, interactionRange, layerMask)
                    ? null
                    : _currentHitInfo.transform.GetComponent<IInteractionInvoker>();
            
            if (currentlyCheckedInteraction != _currentInteraction)
            {
                SwitchInteraction(currentlyCheckedInteraction);
            }
            
            Interact(Enums.InteractionState.Tick);
        }

        private void SwitchInteraction(IInteractionInvoker currentlyCheckedInteraction)
        {
            Interact(Enums.InteractionState.ExitInteraction);
            _currentInteraction = currentlyCheckedInteraction;
            if (_currentInteraction != null)
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
            if (_currentInteraction == null) return;

            InteractionDataSystem interactionDataSystem = new()
            {
                InteractionType = _currentInteractionType,
                InteractionState = interactionState
            };
            
            InteractionDataArgs interactionDataArgs = new()
            {
                HitInfo = _currentHitInfo,
            };
            
            _currentInteraction.Interact(interactionDataSystem, interactionDataArgs);

            if (interactionState == Enums.InteractionState.ExitInteraction)
            {
                _currentInteraction = null;
            }
        }
    }
}