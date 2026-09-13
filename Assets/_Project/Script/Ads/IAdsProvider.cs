using System;

namespace MeowStudio.Ads
{
    public interface IAdsProvider
    {
        void Initialize(AdsData adsData);
        void ShowBanner();
        void HideBanner();
        bool InterstitialIsLoaded();
        void ShowInterstitial();
        bool RewardedIsLoaded(AdsRewardType adsRewardType);
        void ShowRewarded(AdsRewardType adsRewardType, Action onComplete);
    }
}
