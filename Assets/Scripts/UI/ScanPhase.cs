using System.Collections;
using PipeQuest.Core.Enums;
using UnityEngine;

namespace PipeQuest.UI
{
    public class ScanPhase : MonoBehaviour
    {
        [SerializeField] private float revealDuration = 1f;
        [SerializeField] private float delayBetweenCells = 0.1f;
        [SerializeField] private RectTransform airplaneIcon;

        private bool isScanning = false;

        public void StartScan()
        {
            if (isScanning) return;
            StartCoroutine(ScanRoutine());
        }

        private IEnumerator ScanRoutine()
        {
            isScanning = true;
            int width = Grid.GridManager.Instance.Width;
            int height = Grid.GridManager.Instance.Height;

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = Grid.GridManager.Instance.GetCell(x, y);
                    if (cell != null)
                    {
                        cell.Reveal(GetOriginalColor(cell.Data.type));
                        if (airplaneIcon != null)
                            airplaneIcon.anchoredPosition = cell.GetComponent<RectTransform>().anchoredPosition;
                        yield return new WaitForSeconds(delayBetweenCells);
                    }
                }
            }

            yield return new WaitForSeconds(1f);
            Grid.GridManager.Instance.HideAllCells();
            Core.StageController.Instance.NextStage();
            isScanning = false;
        }

        private UnityEngine.Color GetOriginalColor(CellType type)
        {
            switch (type)
            {
                case CellType.Passable: return new Color(0.78f, 0.90f, 0.79f);
                case CellType.Obstacle: return new Color(0.26f, 0.26f, 0.26f);
                case CellType.Building: return new Color(0.56f, 0.79f, 0.98f);
                case CellType.SourceSea: return new Color(0.31f, 0.76f, 0.97f);
                case CellType.SourceTank: return new Color(1.0f, 0.54f, 0.40f);
                default: return new Color(0.93f, 0.93f, 0.93f);
            }
        }
    }
}
