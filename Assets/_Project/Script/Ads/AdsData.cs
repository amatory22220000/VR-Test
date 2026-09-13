using UnityEngine;

namespace MeowStudio.Ads
{
    [CreateAssetMenu(menuName = "DATA/Ads/AdsData", fileName = "AdsData")]
    public class AdsData : ScriptableObject
    {
        [field: SerializeField] public bool useTestAds { get; private set; }

        public string bannerId_test_android { get { return "ca-app-pub-3940256099942544/6300978111"; } private set { } }
        public string interstitialId_test_android { get { return "ca-app-pub-3940256099942544/1033173712"; } private set { } }
        public string rewardedId_test_android { get { return "ca-app-pub-3940256099942544/5224354917"; } private set { } }

        public string bannerId_test_ios { get { return "ca-app-pub-3940256099942544/2934735716"; } private set { } }
        public string interstitialId_test_ios { get { return "ca-app-pub-3940256099942544/4411468910"; } private set { } }
        public string rewardedId_test_ios { get { return "ca-app-pub-3940256099942544/1712485313"; } private set { } }

        [field: SerializeField, Space, Header("Android")] public string bannerId_android { get; private set; }
        [field: SerializeField] public string interId_android { get; private set; }
        [field: SerializeField] public string rewardedId_android { get; private set; }

        [field: SerializeField, Space, Header("iOS")] public string bannerId_ios { get; private set; }
        [field: SerializeField] public string interId_ios { get; private set; }
        [field: SerializeField] public string rewardedId_ios { get; private set; }
    }
}
