using UnityEngine;
using UnityEngine.UI;
using System;

namespace PipeQuest.Core
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance { get; private set; }

        [SerializeField] private Text timerText;
        private float currentTime;
        private bool isRunning;
        private Action onTimeUp;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (!isRunning) return;
            currentTime -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.Ceil(currentTime).ToString();

            if (currentTime <= 0)
            {
                isRunning = false;
                onTimeUp?.Invoke();
            }
        }

        public void StartTimer(float seconds, Action callback)
        {
            currentTime = seconds;
            isRunning = true;
            onTimeUp = callback;
        }

        public void StopTimer() => isRunning = false;
    }
}
