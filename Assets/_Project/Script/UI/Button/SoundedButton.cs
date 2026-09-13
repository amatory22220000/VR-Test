using MeowStudio.Audio;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MeowStudio.UI
{
    [RequireComponent(typeof(Button))]
    public class SoundedButton : MonoBehaviour
    {
        [Inject] private AudioPlayer audioPlayer;

        private void Start()
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(PlayClick);
        }

        private void PlayClick()
        {
            audioPlayer.PlaySound(audioPlayer.audioData.buttonClick);
        }
    }
}
