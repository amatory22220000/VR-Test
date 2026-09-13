using UnityEngine;

namespace MeowStudio.Audio
{
    [CreateAssetMenu(menuName = "DATA/Audio/AudioData")]
    public class AudioData : ScriptableObject
    {
        [field: SerializeField] public SoundData buttonClick { get; private set; }
    }
}
