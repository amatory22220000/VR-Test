using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace MeowStudio.Ads
{
    public class AdsTester: MonoBehaviour
    {
        [Inject] private AdsService adsService;

        [Button]
        private void ShowBanner()
        {
            adsService.ShowBanner();
        }
        [Button]
        private void HideBanner()
        {
            adsService.HideBanner();
        }
        [Button]
        private void ShowInterstitial()
        {
            adsService.ShowInterstitial();
        }
        [Button]
        private void ShowRewarded(AdsRewardType adsRewardType)
        {
            adsService.ShowRewarded(adsRewardType, ()=> Debug.Log("Get test reward!"));
        }
    }
}
