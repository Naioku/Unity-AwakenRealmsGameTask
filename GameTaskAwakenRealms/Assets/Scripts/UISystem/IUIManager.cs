namespace UISystem
{
    public interface IUIManager
    {
        public event System.Action OnRoll;
        public void SetRollingActive(bool active);
        public void SetResult(string result);
        public void StartGame();
        public void StopGame();
        public void SaveScore(int score, int totalScore);
    }
}