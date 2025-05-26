using UnityEngine;

namespace Dice.States
{
    public class StateThrow : StateBase
    {
        public override Enums.DieState EnumValue => Enums.DieState.Throw;
        
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Throw");
            RestoreRigidbodySettings();
        }

        public override void Tick()
        {
            base.Tick();
            if (!IsDieIdle()) return;

            stateMachine.SwitchState(stateMachine.StateScoreDetection);
        }
    }
}