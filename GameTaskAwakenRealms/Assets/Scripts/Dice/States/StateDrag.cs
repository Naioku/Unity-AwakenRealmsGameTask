using UnityEngine;

namespace Dice.States
{
    public class StateDrag : StateBase
    {
        public override Enums.DieState EnumValue => Enums.DieState.Drag;
        
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Drag");
            stateMachine.Interaction.Enabled = false;
            stateMachine.Rigidbody.isKinematic = false;
            stateMachine.Rigidbody.mass = stateMachine.StatesData.drag.mass;
            stateMachine.Rigidbody.drag = stateMachine.StatesData.drag.drag;
            stateMachine.Rigidbody.angularDrag = stateMachine.StatesData.drag.angularDrag;
        }
    }
}