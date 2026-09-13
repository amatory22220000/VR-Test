#undef ADMOB
//#define ADMOB

#if ADMOB
using GoogleMobileAds.Api;
#endif

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeowStudio.Ads
{
    public class AdmobAdsProvider : IAdsProvider
    {
        private AdsData data;

#if ADMOB
        private BannerView bannerAd;
        private InterstitialAd interstitialAd;
        private Dictionary<AdsRewardType, RewardedAd> rewardedAdDict = new Dictionary<AdsRewardType, RewardedAd>()
        {
            { AdsRewardType.main_reward, null}
        };
        private Dictionary<AdsRewardType, string> rewardedIdDict = new();
#endif

        private string bannerID;
        private string interstitialID;
        private bool bannerIsVisible;

        public void Initialize(AdsData data)
        {
            this.data = data;
            Print("Initialize ads");

#if ADMOB
            SetID();
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                StartLoadAds();
            });
#endif
        }

        private async UniTaskVoid StartLoadAds()
        {
#if ADMOB
            LoadInterstitialAd(0);
            await UniTask.Delay(500);
            LoadRewardedAd(AdsRewardType.main_reward, 0);
#endif
        }
        private void SetID()
        {
            if (data.useTestAds)
            {
#if ADMOB
#if UNITY_ANDROID
                bannerID = data.bannerId_test_android;
                interstitialID = data.interstitialId_test_android;
                rewardedIdDict.Add(AdsRewardType.main_reward, data.rewardedId_test_android);
#elif UNITY_IOS
                bannerID = data.bannerId_test_ios;
                interstitialID = data.interstitialId_test_ios;
                rewardedIdDict.Add(AdsRewardType.main_reward, data.rewardedId_test_ios);
#endif
            }
            else
            {
#if UNITY_ANDROID
                bannerID = data.bannerId_android;
                interstitialID = data.interId_android;
                rewardedIdDict.Add(AdsRewardType.main_reward, data.rewardedId_android);
#elif UNITY_IOS
                bannerID = data.bannerId_ios;
                interstitialID = data.interId_ios;
                rewardedIdDict.Add(AdsRewardType.main_reward, data.rewardedId_ios);
#endif
#endif
            }
        }

        public void ShowBanner()
        {
            Print("Show banner!");
        }

        public void HideBanner()
        {
            Print("Hide banner!");
        }

#if ADMOB
        private void RegisterReloadHandler(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Print("ADS: Interstitial Ad full screen content closed");
                LoadInterstitialAd(0);
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Print("ADS: Interstitial ad failed to open full screen content " +
                                "with error : " + error);
                LoadInterstitialAd(2);
            };
        }
        public async UniTaskVoid LoadInterstitialAd(float loadDelay)
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            if (loadDelay > 0)
                await UniTask.WaitForSeconds(loadDelay);

            Print("ADS: Loading the interstitial ad");

            var adRequest = new AdRequest();

            InterstitialAd.Load(interstitialID, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Print("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Print("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    interstitialAd = ad;
                    RegisterReloadHandler(interstitialAd);
                });
        }
#endif
        public void ShowInterstitial()
        {
#if ADMOB
            if (InterstitialIsLoaded())
            {
                Print("ADS: Show interstitial");
                interstitialAd.Show();
            }
            else
            {
                Print("ADS: Interstitial ad is not ready yet");
            }
#else
            Print("ADS: Show interstitial");
#endif
        }
        public bool InterstitialIsLoaded()
        {
#if ADMOB
            return interstitialAd != null && interstitialAd.CanShowAd();
#else
            return true;
#endif
        }

#if ADMOB
        private void RegisterReloadHandler(AdsRewardType rewardedType)
        {
            rewardedAdDict[rewardedType].OnAdFullScreenContentClosed += () =>
            {
                Print("ADS: Rewarded Ad full screen content closed.");
                LoadRewardedAd(rewardedType, 0);
            };
            rewardedAdDict[rewardedType].OnAdFullScreenContentFailed += (AdError error) =>
            {
                Print("ADS: Rewarded ad failed to open full screen content " +
                               "with error : " + error);
                LoadRewardedAd(rewardedType, 2);
            };
        }
        private async UniTaskVoid LoadRewardedAd(AdsRewardType rewardedType, float loadDelay)
        {
            if (rewardedAdDict[rewardedType] != null)
            {
                rewardedAdDict[rewardedType].Destroy();
                rewardedAdDict[rewardedType] = null;
            }
            Print("Loading the rewarded ad");

            if (loadDelay > 0)
                await UniTask.WaitForSeconds(loadDelay);

            var adRequest = new AdRequest();

            RewardedAd.Load(RewardedID(), adRequest,
                (RewardedAd rewarded, LoadAdError error) =>
                {
                    if (error != null || rewarded == null)
                    {
                        Print("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Print("Rewarded ad loaded with response : "
                              + rewarded.GetResponseInfo());
                    rewardedAdDict[rewardedType] = rewarded;
                    RegisterReloadHandler(rewardedType);
                });

            string RewardedID()
            {
                if (data.useTestAds) return data.rewardedId_test_android;
                switch (rewardedType)
                {
                    case AdsRewardType.main_reward:
                        return data.rewardedId_android;
                }
                return "";
            }
        }
#endif
        public bool RewardedIsLoaded(AdsRewardType rewardedType)
        {
#if ADMOB
            return rewardedAdDict[rewardedType] != null && rewardedAdDict[rewardedType].CanShowAd();
#else
            return true;
#endif
        }
        public void ShowRewarded(AdsRewardType rewardedType, Action onReward)
        {
#if ADMOB
            if (RewardedIsLoaded(rewardedType))
            {
                Print("ADS: Show rewarded");
                rewardedAdDict[rewardedType].Show((Reward reward) =>
                {
                    onReward?.Invoke();
                });
            }
            else
            {
                Print("ADS: Rewarded ad is not ready yet");
            }
#else
            Print("ADS: Show rewarded");
#endif
        }


        private bool debug = true;
        private void Print(string message)
        {
            if (!debug) return;
            Debug.Log(message);
        }
    }
}
