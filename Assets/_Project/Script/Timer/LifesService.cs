using System;
using MeowStudio.Data;
using MeowStudio.DataSave;
using MeowStudio.SceneManagement;
using MeowStudio.Utils;
using UnityEngine;
using Zenject;

namespace MeowStudio.Currency
{
    public class LifesService: ITickable, IDisposable
    {
        [Inject] private SaveDataProvider saveDataProvider;
        private SaveData save => saveDataProvider.saveData;
        [Inject] private GameSettingsProvider settingsProvider;
        private GameSettings settings => settingsProvider.settings;
        [Inject] private AppStatusService appStatusService;

        public int lifes => save.lives;

        public bool isFullLifes => lifes >= settings.maxLives;
        public bool isInfinityLifes;
        public bool isRestoreTimer => timer.IsRunning;
        public int timerRestSeconds => timer.restSeconds;
        public int infinityTimerRestSeconds => infinityTimer.restSeconds;

        public Action OnTimerTick = delegate { };
        public Action OnTimerFinish = delegate { };

        public Action OnInfinityTimerTick = delegate { };
        public Action OnInfinityTimerFinish = delegate { };

        public Action OnLifesChanged;

        private CountdownTimer timer;
        private CountdownTimer infinityTimer;
        private int restoreSeconds;

        public void Initialize()
        {
            restoreSeconds = settings.lifeRestoreTime.x * 60 + settings.lifeRestoreTime.y;
            timer = new CountdownTimer(restoreSeconds);
            infinityTimer = new CountdownTimer(0);
            AddOfflineLifes();
            CheckContinueInfinityTimer();

            timer.OnTimerTick += DoOnTimerTick;
            timer.OnTimerFinish += DoOnFinishRestoreTimer;

            infinityTimer.OnTimerTick += DoOnInfinityTimerTick;
            infinityTimer.OnTimerFinish += DoOnFinishInfinityTimer;

            appStatusService.OnFocus += AddOfflineLifes;
            appStatusService.OnFocus += CheckContinueInfinityTimer;
        }
        public void Dispose()
        {
            timer.OnTimerTick -= DoOnTimerTick;
            timer.OnTimerFinish -= DoOnFinishRestoreTimer;

            infinityTimer.OnTimerTick -= DoOnInfinityTimerTick;
            infinityTimer.OnTimerFinish -= DoOnFinishInfinityTimer;

            appStatusService.OnFocus -= AddOfflineLifes;
            appStatusService.OnFocus -= CheckContinueInfinityTimer;
        }
        public void Tick()
        {
            timer.Tick(Time.deltaTime);
            infinityTimer.Tick(Time.deltaTime);
        }

        public void SetLifes(int amount)
        {
            save.lives = amount;
            CheckStartTimer();
            OnLifesChanged?.Invoke();
        }

        private void AddOfflineLifes()
        {
            if (save.startLifeRestoreDate == 0)
            {
                if (!isFullLifes)
                    FullRestartTimer();
                return;
            }

            var secondsPassed = (int)(DateTime.Now - DateTime.FromBinary(save.startLifeRestoreDate)).TotalSeconds;
            int restored = Mathf.FloorToInt(secondsPassed / restoreSeconds);
            if (restored > 0)
            {
                save.lives += restored;
                save.lives = Mathf.Min(save.lives, settings.maxLives);
            }

            if (isFullLifes)
            {
                StopTimer();
                save.startLifeRestoreDate = 0;
            }
            else
            {
                int secondsToNext = restoreSeconds - (secondsPassed % restoreSeconds);
                timer.Reset(secondsToNext);
                timer.Start();
                save.startLifeRestoreDate = DateTime.Now.AddSeconds(-secondsPassed % restoreSeconds).ToBinary();
            }

            OnLifesChanged?.Invoke();
        }


        private void CheckContinueInfinityTimer()
        {
            DateTime finishInfinityLifesDate = DateTime.FromBinary(save.finishInfLifesDate);
            if (finishInfinityLifesDate <= DateTime.Now) return;

            int seconds = (int)finishInfinityLifesDate.Subtract(DateTime.Now).TotalSeconds;
            SetInfinityTimer(seconds);
        }

        public bool CanRemoveLifes(int value) => isInfinityLifes || lifes >= value;
        public void AddLifes(int value)
        {
            save.lives += value;
            CheckStartTimer();
            OnLifesChanged?.Invoke();
        }
        public void RemoveLifes(int value)
        {
            if (isInfinityLifes) return;
            if (!CanRemoveLifes(value))
                return;
            save.lives -= value;
            CheckStartTimer();
            OnLifesChanged?.Invoke();
        }
        private void CheckStartTimer()
        {
            if (isFullLifes)
                StopTimer();
            else if (!timer.IsRunning)
                FullRestartTimer();
        }

        public void SetInfinityTimer(int seconds)
        {
            isInfinityLifes = true;
            save.finishInfLifesDate = DateTime.Now.AddSeconds(seconds).ToBinary();
            infinityTimer.Reset(seconds);
            infinityTimer.Start();
        }

        private void DoOnTimerTick(int restSeconds)
        {
            OnTimerTick?.Invoke();
        }
        private void DoOnFinishRestoreTimer()
        {
            AddLifes(1);
            OnTimerFinish?.Invoke();
        }

        private void DoOnInfinityTimerTick(int restSeconds)
        {
            OnInfinityTimerTick?.Invoke();
        }
        private void DoOnFinishInfinityTimer()
        {
            isInfinityLifes = false;
            OnInfinityTimerFinish?.Invoke();
        }

        private void FullRestartTimer()
        {
            timer.Reset(restoreSeconds);
            timer.Start();
            save.startLifeRestoreDate = DateTime.Now.ToBinary();
        }
        private void StopTimer()
        {
            timer.Stop();
        }
    }
}