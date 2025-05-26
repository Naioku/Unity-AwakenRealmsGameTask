using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelResult;
        [SerializeField] private TextMeshProUGUI labelTotal;
        [SerializeField] private Button buttonRoll;

        public string Result { set => labelResult.text = value; }
        public string Total { set => labelTotal.text = value; }
        public event System.Action OnRollButtonClicked;

        private void Awake()
        {
            buttonRoll.onClick.AddListener(HandleRollButtonClicked);
            labelResult.text = "0";
            labelTotal.text = "0";
        }

        private void OnDestroy() => buttonRoll.onClick.RemoveListener(HandleRollButtonClicked);

        private void HandleRollButtonClicked() => OnRollButtonClicked?.Invoke();

        public void SetRollingActive(bool active) => buttonRoll.interactable = active;
    }
}