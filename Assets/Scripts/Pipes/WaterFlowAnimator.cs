using System.Collections;
using System.Collections.Generic;
using PipeQuest.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace PipeQuest.Pipes
{
    public class WaterFlowAnimator : MonoBehaviour
    {
        [SerializeField] private float flowSpeed = 2f;

        public void AnimateFlow(List<Grid.CellData> path, PipeColor color, System.Action onComplete)
        {
            StartCoroutine(FlowRoutine(path, color, onComplete));
        }

        private IEnumerator FlowRoutine(List<Grid.CellData> path, PipeColor color, System.Action onComplete)
        {
            Color flowColor = color == PipeColor.Blue
                ? new Color(0.13f, 0.59f, 0.95f, 0.8f)
                : new Color(0.96f, 0.26f, 0.21f, 0.8f);

            foreach (var cellData in path)
            {
                var cell = Grid.GridManager.Instance.GetCell(cellData.x, cellData.y);
                if (cell == null) continue;

                var rt = cell.GetComponent<RectTransform>();
                GameObject flowObj = new GameObject("Flow");
                flowObj.transform.SetParent(rt, false);

                Image flowImage = flowObj.AddComponent<Image>();
                flowImage.color = flowColor;
                flowImage.rectTransform.sizeDelta = rt.sizeDelta * 0.6f;
                flowImage.rectTransform.anchoredPosition = Vector2.zero;

                yield return new WaitForSeconds(1f / flowSpeed);
                Destroy(flowObj);
            }

            onComplete?.Invoke();
        }
    }
}
