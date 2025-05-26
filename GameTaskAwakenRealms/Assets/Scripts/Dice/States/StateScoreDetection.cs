using UnityEngine;

namespace Dice.States
{
    public class StateScoreDetection : StateBase
    {
        public override Enums.DieState EnumValue => Enums.DieState.ScoreDetection;
        
        public override void Enter()
        {
            base.Enter();
            Debug.Log("Score calculation");
            DieController.SideData? result = null;
            foreach (DieController.SideData sideData in stateMachine.SidesData)
            {
                if (Vector3.Dot(sideData.ForwardVector, Vector3.up) > stateMachine.StatesData.scoreDetection.minDotProductPassing)
                {
                    result = sideData;
                    break;
                }
            }
                    
            if (result == null)
            {
                stateMachine.SwitchState(stateMachine.StateAutoThrow);
                return;
            }
            
            stateMachine.OnScoreDetected(result.Value.Number);
            stateMachine.SwitchState(stateMachine.StateIdle);
        }
    }
}