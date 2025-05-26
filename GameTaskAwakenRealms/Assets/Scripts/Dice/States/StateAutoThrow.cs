using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dice.States
{
    public class StateAutoThrow : StateBase
    {
        public override Enums.DieState EnumValue => Enums.DieState.AutoThrow;
        private Guid _delayedActionSwitchStateThrow;
        
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Auto throw");
            stateMachine.Interaction.Enabled = false;
            stateMachine.Rigidbody.isKinematic = false;
            var randomForce = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(stateMachine.StatesData.autoThrow.minVerticalDirection, 1f),
                Random.Range(-1f, 1f)).normalized;
                    
            var randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)).normalized;
                    
            stateMachine.Rigidbody.AddForce(randomForce * stateMachine.StatesData.autoThrow.throwForce, ForceMode.Impulse);
            stateMachine.Rigidbody.AddTorque(randomTorque * stateMachine.StatesData.autoThrow.torqueForce, ForceMode.Impulse);
            _delayedActionSwitchStateThrow = Managers.Instance.TimerManager.RunDelayedAction(() => stateMachine.SwitchState(stateMachine.StateThrow), 0.5f);
        }

        public override void Exit()
        {
            base.Exit();
            Managers.Instance.TimerManager.CancelDelayedAction(_delayedActionSwitchStateThrow);
            _delayedActionSwitchStateThrow = Guid.Empty;
        }

        public override void Destroy()
        {
            base.Destroy();
            Managers.Instance.TimerManager.CancelDelayedAction(_delayedActionSwitchStateThrow);
        }
    }
}