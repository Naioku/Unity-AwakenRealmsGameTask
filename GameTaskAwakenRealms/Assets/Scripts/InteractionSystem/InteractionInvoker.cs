using UnityEngine;

namespace InteractionSystem
{
    public abstract class InteractionInvoker : MonoBehaviour
    {
        public abstract MonoBehaviour Interact(InteractionDataSystem interactionDataSystem, InteractionDataArgs interactionDataArgs);
    }
}