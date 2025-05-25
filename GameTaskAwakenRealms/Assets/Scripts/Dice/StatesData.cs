namespace Dice
{
    [System.Serializable]
    public struct StatesData
    {
        public Drag drag;
    }

    [System.Serializable]
    public struct Drag
    {
        public float mass;
        public float drag;
        public float angularDrag;
    }
}