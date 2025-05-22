using TMPro;
using UnityEngine;

namespace Dice
{
    public class Side : MonoBehaviour
    {
        [SerializeField] private TextMeshPro number;

        public string Number
        {
            set
            {
                number.text = value;
                name = name.Split("_")[0] + $"_{value}";
            }
        }
    }
}