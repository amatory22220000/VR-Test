using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Audio;
using MeowStudio.DataSave;


namespace MeowStudio.Audio
{
    public class AudioPlayer: MonoBehaviour
    {
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource stopableSource;

        [SerializeField] private AudioMixer mixer;
        [field: SerializeField] public AudioData audioData { get; private set; }

        private SaveDataProvider saveDataProvider;
        private Coroutine fadeCor;
        private float fadeSpeed = 1.5f;

        public void Initialize(SaveDataProvider saveDataProvider)
        {
            this.saveDataProvider = saveDataProvider;

            SetSoundVolume(saveDataProvider.saveData.soundVolume);
            SetMusicVolume(saveDataProvider.saveData.musicVolume);
        }

        public void SetSoundVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            saveDataProvider.saveData.soundVolume = volume;
            float setVolume = Mathf.Clamp(saveDataProvider.saveData.soundVolume, 0.0001f, 1f);
            mixer.SetFloat("SoundVolume", Mathf.Log10(setVolume) * 20);
        }
        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0, 1);
            saveDataProvider.saveData.musicVolume = volume;
            float setVolume = Mathf.Clamp(saveDataProvider.saveData.musicVolume, 0.0001f, 1f);
            mixer.SetFloat("MusicVolume", Mathf.Log10(setVolume) * 20);
        }


        public void PlaySound(SoundData data)
        {
            if (!data.clip || data.volume == 0) return;
            if (saveDataProvider.saveData.soundVolume == 0 || data.volume == 0) return;
            soundSource.PlayOneShot(data.clip, data.volume);
        }

        public void PlayStopableSound(SoundData data)
        {
            if (!data.clip || data.volume == 0) return;
            if (saveDataProvider.saveData.soundVolume == 0 || data.volume == 0) return;
            stopableSource.clip = data.clip;
            stopableSource.volume = data.volume;
            stopableSource.Play();
        }
        public void StopStopableSound()
        {
            if (stopableSource.isPlaying)
                stopableSource.Stop();
        }

        public void PlayMusic(SoundData data)
        {
            if (!data.clip || data.volume == 0) return;
            Action playNewMusic = () =>
            {
                musicSource.clip = data.clip;
                musicSource.Play();
                Fade(musicSource,
                        targetVol: data.volume,
                        fadeSpeed,
                        null);
            };
            Fade(musicSource,
                targetVol: 0,
                fadeSpeed,
                () => playNewMusic.Invoke());
        }
        public void StopMusic()
        {
            if (musicSource.volume > 0 && musicSource.clip && musicSource.isPlaying)
            {
                Fade(musicSource,
                    targetVol: 0,
                    fadeSpeed,
                    null);
            }
        }

        private void Fade(AudioSource source, float targetVol, float fadeSpeed, Action onComplete)
        {
            if (fadeCor != null) StopCoroutine(fadeCor);
            fadeCor = StartCoroutine(FadeCor());
            IEnumerator FadeCor()
            {
                while (source.volume != targetVol)
                {
                    source.volume = Mathf.MoveTowards(source.volume, targetVol, fadeSpeed * Time.deltaTime);
                    yield return null;
                }
                onComplete?.Invoke();
            }
        }
    }
}