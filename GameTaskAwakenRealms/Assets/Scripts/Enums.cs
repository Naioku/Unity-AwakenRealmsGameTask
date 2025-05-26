public abstract class Enums
{
    #region InteractionSystem

    public enum InteractionType
    {
        Hover,
        Click
    }

    public enum InteractionState
    {
        Tick,
        EnterType,
        ExitType,
        EnterInteraction,
        ExitInteraction
    }

    #endregion

    #region Dice

    public enum DieState
    {
        Idle,
        Drag,
        AutoThrow,
        Throw,
        PutDown,
        ScoreDetection,
    }

    #endregion
}