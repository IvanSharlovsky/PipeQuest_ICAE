using PipeQuest.Core.Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PipeQuest.Grid
{
    public class Cell : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image cellImage;
        [SerializeField] private Image pipeImage;
        [SerializeField] private Image markerImage;
        [SerializeField] private Text pipeSymbolText;

        private CellData cellData;
        private Color originalColor;
        private bool isRevealed = false;

        public CellData Data => cellData;

        public void Initialize(CellData data, Color color)
        {
            cellData = data;
            originalColor = color;

            if (cellImage == null) cellImage = GetComponent<Image>();
            cellImage.color = color;

            if (pipeImage != null) pipeImage.gameObject.SetActive(false);
            if (markerImage != null) markerImage.gameObject.SetActive(false);
            if (pipeSymbolText != null) pipeSymbolText.gameObject.SetActive(false);
        }

        public void Reveal(Color color)
        {
            cellImage.color = color;
            isRevealed = true;
        }

        public void Hide(Color hiddenColor)
        {
            if (cellData.type == CellType.Building ||
                cellData.type == CellType.SourceSea ||
                cellData.type == CellType.SourceTank)
                return;

            cellImage.color = hiddenColor;
            isRevealed = false;
        }

        public void ShowPipe(PipeType pipeType, int rotation, PipeColor color)
        {
            if (pipeImage == null) return;

            pipeImage.gameObject.SetActive(true);
            pipeImage.color = color == PipeColor.Blue
                ? new Color(0.13f, 0.59f, 0.95f)
                : new Color(0.96f, 0.26f, 0.21f);
            pipeImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -rotation);

            if (pipeSymbolText != null)
            {
                pipeSymbolText.gameObject.SetActive(true);
                pipeSymbolText.color = Color.white;
                pipeSymbolText.text = GetPipeSymbol(pipeType);
                pipeSymbolText.rectTransform.localRotation = Quaternion.Euler(0, 0, -rotation);
            }
        }

        public void RemovePipe()
        {
            if (pipeImage != null) pipeImage.gameObject.SetActive(false);
            if (pipeSymbolText != null) pipeSymbolText.gameObject.SetActive(false);
        }

        public void ShowMarker(bool show, Color markerColor)
        {
            if (markerImage == null) return;
            markerImage.gameObject.SetActive(show);
            if (show) markerImage.color = markerColor;
        }

        public void ShowError()
        {
            StartCoroutine(ErrorBlink());
        }

        private System.Collections.IEnumerator ErrorBlink()
        {
            Color errorColor = new Color(0.83f, 0.18f, 0.18f);
            for (int i = 0; i < 3; i++)
            {
                cellImage.color = errorColor;
                yield return new WaitForSeconds(0.2f);
                cellImage.color = isRevealed ? originalColor : new Color(0.93f, 0.93f, 0.93f);
                yield return new WaitForSeconds(0.2f);
            }
        }

        private string GetPipeSymbol(PipeType type)
        {
            switch (type)
            {
                case PipeType.Straight: return "│";
                case PipeType.Elbow: return "└";
                case PipeType.Tee: return "┴";
                case PipeType.Valve: return "⊕";
                default: return "";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Pipes.PipeDraggable.CurrentDraggedPipe != null) return;

            if (cellData.placedPipe != PipeType.None)
            {
                GridManager.Instance.RemovePipe(cellData.x, cellData.y);
            }
            else if (UI.UIManager.Instance.CurrentStage == GameStage.Scanning)
            {
                GridManager.Instance.SetMarker(cellData.x, cellData.y, !cellData.hasMarker);
            }
        }
    }
}
