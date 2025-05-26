using UnityEngine;

namespace UISystem
{
    [System.Serializable]
    public class UIManager : IUIManager
    {
        [SerializeField] private UIController uiController;
        
        public event System.Action OnRoll;
        public void SetResult(string result) => uiController.Result = result;
        public void SetRollingActive(bool active) => uiController.SetRollingActive(active);

        public void Awake()
        {
            uiController.OnRollButtonClicked += HandleRollButtonClicked;
        }

        public void Destroy()
        {
            if (uiController)
            {
                Object.Destroy(uiController.gameObject);
            }
        }

        public void SaveScore(int score, int totalScore)
        {
            uiController.Result = score.ToString();
            uiController.Total = totalScore.ToString();
        }
        
        private void HandleRollButtonClicked() => OnRoll?.Invoke();
    }
}