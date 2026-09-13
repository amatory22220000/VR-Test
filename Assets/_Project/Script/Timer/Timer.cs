using System;

namespace MeowStudio.Utils
{
    public abstract class Timer
    {
        protected float initialTime;
        protected float Time { get; set; }
        public bool IsRunning { get; protected set; }

        public float Progress => Time / initialTime;

        public Action OnTimerStart = delegate { };
        public Action<int> OnTimerTick = delegate { };
        public Action OnTimerStop = delegate { };
        public Action OnTimerFinish = delegate { };

        protected Timer(float value)
        {
            initialTime = value;
            IsRunning = false;
        }

        public virtual void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
            }
        }

        public void Finish()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerFinish.Invoke();
            }
        }

        public void Pause() => IsRunning = false;
        public void Resume() => IsRunning = true;

        public abstract void Tick(float deltaTime);
    }

    public class CountdownTimer : Timer
    {
        public CountdownTimer(float value) : base(value) { }
        public int restSeconds { get; private set; }

        public override void Start()
        {
            base.Start();
            restSeconds = (int)Time;
            OnTimerTick?.Invoke(restSeconds);
        }
        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0)
            {
                Time -= deltaTime;
                if(Time < restSeconds - 1)
                {
                    restSeconds--;
                    OnTimerTick?.Invoke(restSeconds);
                }
            }

            if (IsRunning && Time <= 0)
            {
                Finish();
            }
        }

        public bool IsFinished => Time <= 0;

        public void Reset() => Time = initialTime;

        public void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }
    }

    public class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0) { }

        public override void Tick(float deltaTime)
        {
            if (IsRunning)
            {
                Time += deltaTime;
            }
        }

        public void Reset() => Time = 0;

        public float GetTime() => Time;
    }
}