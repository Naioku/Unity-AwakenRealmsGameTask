using UnityEngine;

namespace Dice.States
{
    public class StatePutDown : StateBase
    {
        public override Enums.DieState EnumValue => Enums.DieState.PutDown;
        
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Put down");
            stateMachine.Interaction.Enabled = false;
            stateMachine.Rigidbody.velocity = Vector3.zero;
            RestoreRigidbodySettings();
        }

        public override void Tick()
        {
            base.Tick();
            if (!IsDieIdle()) return;
            
            stateMachine.SwitchState(stateMachine.StateIdle);
        }
    }
}