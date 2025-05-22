using System;

namespace UpdateSystem
{
    public interface IUpdateRegistrar
    {
        public void RegisterOnUpdate(Action updatableObj);
        public void RegisterOnFixedUpdate(Action updatableObj);
        public void RegisterOnLateUpdate(Action updatableObj);
        public void UnregisterFromUpdate(Action updatableObj);
        public void UnregisterFromFixedUpdate(Action updatableObj);
        public void UnregisterFromLateUpdate(Action updatableObj);
    }
}