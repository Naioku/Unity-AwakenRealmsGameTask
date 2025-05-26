namespace Dice.States
{
    public abstract class StateBase
    {
        protected DieController.StateMachine stateMachine;
        public abstract Enums.DieState EnumValue { get; }

        public void Initialize(DieController.StateMachine stateMachine) => this.stateMachine = stateMachine;
        public virtual void Enter() {}
        public virtual void Tick() {}
        public virtual void Exit() {}
        public virtual void Destroy() {}
        
        protected bool IsDieIdle()
        {
            if (!stateMachine.IsGrounded) return false;
            if (stateMachine.Rigidbody.velocity.magnitude > stateMachine.MinRollingVelocity) return false;

            return true;
        }
        
        protected void RestoreRigidbodySettings()
        {
            stateMachine.Rigidbody.mass = stateMachine.CacheMass;
            stateMachine.Rigidbody.drag = stateMachine.CacheDrag;
            stateMachine.Rigidbody.angularDrag = stateMachine.CacheAngularDrag;
        }
    }
}