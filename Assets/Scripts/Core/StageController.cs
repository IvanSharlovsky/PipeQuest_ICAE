using PipeQuest.Core.Enums;
using UnityEngine;

namespace PipeQuest.Core
{
    public class StageController : MonoBehaviour
    {
        public static StageController Instance { get; private set; }

        [SerializeField] private float scanTime = 20f;
        [SerializeField] private float buildTimePerStage = 120f;

        private GameStage currentStage;
        private int attemptCount = 0;
        private const int MaxAttempts = 2;

        public GameStage CurrentStage => currentStage;
        public int AttemptCount => attemptCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartStage(GameStage stage)
        {
            currentStage = stage;
            UI.UIManager.Instance.SwitchPanel(stage);

            switch (stage)
            {
                case GameStage.Scanning:
                    TimerManager.Instance.StartTimer(scanTime, OnScanTimeUp);
                    break;
                case GameStage.BuildHeatRemoval:
                case GameStage.BuildFirefighting:
                    attemptCount = 0;
                    TimerManager.Instance.StartTimer(buildTimePerStage, OnBuildTimeUp);
                    break;
            }
        }

        public void NextStage()
        {
            if (currentStage == GameStage.Scanning)
                StartStage(GameStage.BuildHeatRemoval);
            else if (currentStage == GameStage.BuildHeatRemoval)
                StartStage(GameStage.BuildFirefighting);
            else if (currentStage == GameStage.BuildFirefighting)
                StartStage(GameStage.Check);
        }

        public void RegisterAttempt()
        {
            attemptCount++;
            if (attemptCount >= MaxAttempts)
            {
                UI.UIManager.Instance.ShowCorrectPath();
                Invoke(nameof(NextStage), 2f);
            }
        }

        private void OnScanTimeUp() => NextStage();
        private void OnBuildTimeUp()
        {
            UI.UIManager.Instance.ShowCorrectPath();
            Invoke(nameof(NextStage), 2f);
        }
    }
}
