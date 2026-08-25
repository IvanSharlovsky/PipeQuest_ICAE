using PipeQuest.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace PipeQuest.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject scanPanel;
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private GameObject checkPanel;
        [SerializeField] private GameObject resultPanel;

        [Header("Result UI")]
        [SerializeField] private Text resultTitle;
        [SerializeField] private Text resultFact;
        [SerializeField] private GameObject successIcon;
        [SerializeField] private GameObject failIcon;

        [Header("Build UI")]
        [SerializeField] private Text stageTitle;
        [SerializeField] private Text attemptText;
        [SerializeField] private Button runWaterButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button resetButton;

        private GameStage currentStage;

        public GameStage CurrentStage => currentStage;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            runWaterButton?.onClick.AddListener(() => Core.GameManager.Instance.CheckSystems());
            resetButton?.onClick.AddListener(() => Grid.GridManager.Instance.ClearAllPipes());
            hintButton?.onClick.AddListener(ShowHint);
        }

        public void SwitchPanel(GameStage stage)
        {
            currentStage = stage;
            scanPanel?.SetActive(stage == GameStage.Scanning);
            buildPanel?.SetActive(stage == GameStage.BuildHeatRemoval || stage == GameStage.BuildFirefighting);
            checkPanel?.SetActive(stage == GameStage.Check);
            resultPanel?.SetActive(stage == GameStage.Result);

            if (stage == GameStage.BuildHeatRemoval)
                stageTitle.text = "Теплоотвод: соедините море → АЭС";
            else if (stage == GameStage.BuildFirefighting)
                stageTitle.text = "Пожаротушение: соедините резервуар → АЭС";
        }

        public void ShowError(string message, Vector2Int pos)
        {
            if (attemptText != null)
                attemptText.text = "Ошибка: " + message;
            if (pos.x >= 0 && pos.y >= 0)
            {
                var cell = Grid.GridManager.Instance.GetCell(pos.x, pos.y);
                cell?.ShowError();
            }
            Core.StageController.Instance.RegisterAttempt();
        }

        public void ShowSuccess()
        {
            resultPanel?.SetActive(true);
            successIcon?.SetActive(true);
            failIcon?.SetActive(false);
            resultTitle.text = "ОБЕ СИСТЕМЫ РАБОТАЮТ";
            resultFact.text = "Морская вода охлаждает турбину, а резервуар пресной воды — независимая система пожаротушения АЭС.";
        }

        public void ShowCorrectPath()
        {
            Grid.GridManager.Instance.RevealAllCells();
        }

        private void ShowHint()
        {
            // Подсветить одну правильную ячейку на пути
            Grid.GridManager.Instance.RevealAllCells();
            Invoke(nameof(HideAfterHint), 1.5f);
        }

        private void HideAfterHint()
        {
            if (currentStage != GameStage.Scanning)
                Grid.GridManager.Instance.HideAllCells();
        }
    }
}
