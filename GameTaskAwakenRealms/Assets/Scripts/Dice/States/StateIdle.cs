using UnityEngine;

namespace Dice.States
{
    public class StateIdle : StateBase
    {
        public override Enums.DieState EnumValue => Enums.DieState.Idle;

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Idle");
            stateMachine.Rigidbody.isKinematic = true;
            stateMachine.Interaction.Enabled = true;
        }
    }
}