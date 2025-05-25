using UnityEngine;

namespace Dice
{
    [System.Serializable]
    public struct StatesData
    {
        public Drag drag;
        public AutoThrow autoThrow;
        public ScoreDetection scoreDetection;
    }

    [System.Serializable]
    public struct Drag
    {
        public float mass;
        public float drag;
        public float angularDrag;
    }
    
    [System.Serializable]
    public struct AutoThrow
    {
        [Tooltip("How much the direction can be inclined horizontally. 1 means that die will be always thrown vertically up.")]
        public float minVerticalDirection;
        public float throwForce;
        public float torqueForce;
    }
    
    [System.Serializable]
    public struct ScoreDetection
    {
        [Tooltip("Used for detecting, which side has been drawn." +
                 " Higher value means the die has to fall clearer to be counted as a success, e.g. when die is blocked by wall.")]
        public float minDotProductPassing;
    }
}