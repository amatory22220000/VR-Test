using System;
using UnityEngine;

namespace MeowStudio.Audio
{
    [Serializable]
    public class SoundData
    {
        public AudioClip clip;
        [Range(0.1f, 1f)]public float volume = 1f;
    }
}
