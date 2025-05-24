namespace InteractionSystem
{
    public interface IInteractionInvoker
    {
        void Interact(InteractionDataSystem interactionDataSystem, InteractionDataArgs interactionDataArgs);
    }
}