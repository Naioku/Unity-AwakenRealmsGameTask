using TMPro;
using UnityEngine;

namespace Dice
{
    public class Side : MonoBehaviour
    {
        [SerializeField] private TextMeshPro number;

        public int Number
        {
            set
            {
                number.text = value.ToString();
                name = name.Split("_")[0] + $"_{value}";
            }
        }
    }
}