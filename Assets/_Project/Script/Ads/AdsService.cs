using System;
using Zenject;

namespace MeowStudio.Ads
{
    public class AdsService
    {
        [Inject] private IAdsProvider adsProvider;
        private AdsData adsData;

        public AdsService(AdsData adsData)
        {
            this.adsData = adsData;
        }
        public void Initialize()
        {
            adsProvider.Initialize(adsData);
        }

        public void ShowBanner()
        {
            adsProvider.ShowBanner();
        }
        public void HideBanner()
        {
            adsProvider.HideBanner();
        }
        public bool InterstitialIsLoaded()
        {
            return adsProvider.InterstitialIsLoaded();
        }
        public void ShowInterstitial()
        {
            adsProvider.ShowInterstitial();
        }
        public bool RewardedIsLoaded(AdsRewardType adsRewardType)
        {
            return adsProvider.RewardedIsLoaded(adsRewardType);
        }
        public void ShowRewarded(AdsRewardType adsRewardType, Action onComplete)
        {
            adsProvider.ShowRewarded(adsRewardType, onComplete);
        }
    }
}
