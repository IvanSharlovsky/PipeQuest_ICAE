using PipeQuest.Core.Enums;
using PipeQuest.Grid;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PipeQuest.Pipes
{
    public class PipeDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Pipe Settings")]
        [SerializeField] private PipeType pipeType;
        [SerializeField] private PipeColor pipeColor;
        [SerializeField] private int defaultRotation = 0;

        [Header("Visual")]
        [SerializeField] private Image pipeImage;
        [SerializeField] private Text pipeSymbolText;

        [Header("Feedback")]
        [SerializeField] private float dragScale = 1.1f;
        [SerializeField] private float returnDuration = 0.3f;

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        private Transform originalParent;
        private int currentRotation = 0;

        public static PipeDraggable CurrentDraggedPipe { get; private set; }

        public PipeType Type => pipeType;
        public PipeColor Color => pipeColor;
        public int Rotation => currentRotation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            currentRotation = defaultRotation;
            UpdateVisual();
        }

        private void Start()
        {
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CurrentDraggedPipe = this;
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
            rectTransform.localScale = Vector3.one * dragScale;
            originalPosition = rectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null) return;

            Vector2 localPoint;
            // Choose camera based on canvas render mode. For Screen Space - Overlay use null.
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                cam,
                out localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
            else
            {
                // Fallback: try world point to avoid lag on some canvas setups.
                Vector3 worldPoint;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvas.transform as RectTransform,
                    eventData.position,
                    cam,
                    out worldPoint))
                {
                    rectTransform.position = worldPoint;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CurrentDraggedPipe = null;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = Vector3.one;

            Cell targetCell = FindCellUnderPointer(eventData.position);

            if (targetCell != null && CanPlacePipe(targetCell.Data))
            {
                GridManager.Instance.PlacePipe(
                    targetCell.Data.x,
                    targetCell.Data.y,
                    pipeType,
                    currentRotation,
                    pipeColor);
                ResetToPool();
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }

        private Cell FindCellUnderPointer(Vector2 screenPosition)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = screenPosition;

            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            foreach (var result in raycastResults)
            {
                Cell cell = result.gameObject.GetComponent<Cell>();
                if (cell != null) return cell;
            }
            return null;
        }

        private bool CanPlacePipe(CellData cellData)
        {
            if (cellData == null) return false;
            if (cellData.type == CellType.Obstacle) return false;
            if (cellData.placedPipe != PipeType.None) return false;
            if (cellData.type == CellType.SourceSea && pipeColor != PipeColor.Blue) return false;
            if (cellData.type == CellType.SourceTank && pipeColor != PipeColor.Red) return false;
            return true;
        }

        private void ReturnToOriginalPosition()
        {
            StartCoroutine(SmoothReturn());
        }

        private System.Collections.IEnumerator SmoothReturn()
        {
            float elapsed = 0f;
            Vector2 start = rectTransform.anchoredPosition;

            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                t = Mathf.SmoothStep(0, 1, t);
                rectTransform.anchoredPosition = Vector2.Lerp(start, originalPosition, t);
                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
        }

        private void ResetToPool()
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        public void RotatePipe()
        {
            currentRotation = (currentRotation + 90) % 360;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (pipeSymbolText != null)
            {
                pipeSymbolText.text = GetPipeSymbol(pipeType);
                pipeSymbolText.rectTransform.localRotation = Quaternion.Euler(0, 0, -currentRotation);
            }

            if (pipeImage != null)
            {
                pipeImage.color = pipeColor == PipeColor.Blue
                    ? new Color(0.13f, 0.59f, 0.95f)
                    : new Color(0.96f, 0.26f, 0.21f);
                pipeImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -currentRotation);
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

        public void SetPipeType(PipeType type, PipeColor color)
        {
            pipeType = type;
            pipeColor = color;
            UpdateVisual();
        }
    }
}