using PipeQuest.Core.Enums;
using UnityEngine;

namespace PipeQuest.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private StageController stageController;
        [SerializeField] private TimerManager timerManager;
        [SerializeField] private PipeQuest.Pipes.PipeValidator pipeValidator;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            stageController.StartStage(GameStage.Scanning);
        }

        public void CheckSystems()
        {
            var blueResult = pipeValidator.ValidateSystem(PipeColor.Blue);
            var redResult = pipeValidator.ValidateSystem(PipeColor.Red);
            bool intersection = pipeValidator.CheckIntersections();

            if (intersection)
            {
                UI.UIManager.Instance.ShowError("Пересечение систем!", Vector2Int.zero);
                return;
            }

            if (!blueResult.isValid)
            {
                UI.UIManager.Instance.ShowError(blueResult.errorMessage, blueResult.errorPosition);
                return;
            }

            if (!redResult.isValid)
            {
                UI.UIManager.Instance.ShowError(redResult.errorMessage, redResult.errorPosition);
                return;
            }

            UI.UIManager.Instance.ShowSuccess();
        }
    }
}
