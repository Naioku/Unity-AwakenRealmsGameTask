using System;

namespace InputSystemExtension.ActionMaps
{
    public interface IActionRegistrar
    {
        public event Action Started;
        public event Action Performed;
        public event Action Canceled;
    }
    
    public interface IActionRegistrar<out T>
    {
        public event Action<T> Started;
        public event Action<T> Performed;
        public event Action<T> Canceled;
    }
}