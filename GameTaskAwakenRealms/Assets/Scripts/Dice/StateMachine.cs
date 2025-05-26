using System.Collections.Generic;
using Dice.States;
using InteractionSystem;
using UnityEngine;

namespace Dice
{
    public partial class DieController
    {
        public class StateMachine
        {
            private readonly DieController _dieController;
            
            internal StateBase currentState;

            public StateMachine(DieController dieController)
            {
                _dieController = dieController;
            }

            public StateBase StateIdle { get; } = new StateIdle();
            public StateBase StateDrag { get; } = new StateDrag();
            public StateBase StateAutoThrow { get; } = new StateAutoThrow();
            public StateBase StateThrow { get; } = new StateThrow();
            public StateBase StatePutDown { get; } = new StatePutDown();
            public StateBase StateScoreDetection { get; } = new StateScoreDetection();

            public float MinRollingVelocity => _dieController.minRollingVelocity;
            public StatesData StatesData => _dieController.statesData;
            public Rigidbody Rigidbody => _dieController.Rigidbody;
            public Interaction Interaction => _dieController.interaction;
            public float CacheMass => _dieController._cacheMass;
            public float CacheDrag => _dieController._cacheDrag;
            public float CacheAngularDrag => _dieController._cacheAngularDrag;
            public bool IsGrounded => _dieController.IsGrounded;
            public List<SideData> SidesData => _dieController.sidesData;
            public void OnScoreDetected(int number) => _dieController.OnScoreDetected?.Invoke(number);

            public void SwitchState(StateBase newState)
            {
                currentState?.Exit();
                currentState = newState;
                _dieController.OnStateChanged?.Invoke(currentState.EnumValue);
                if (currentState == null)
                {
                    Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(Tick);
                }
                else
                {
                    currentState.Initialize(this);
                    currentState.Enter();
                    Managers.Instance.UpdateRegistrar.RegisterOnUpdate(Tick);
                }
            }

            public void Destroy() => currentState.Destroy();
            
            private void Tick()
            {
                currentState.Tick();
            }
        }
    }
}