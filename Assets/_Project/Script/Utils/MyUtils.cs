using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace MeowStudio.Utils
{
    public class MyUtils
    {
        public static bool ListHasDoubles<T>(List<T> list)
        {
            var duplicates = list.GroupBy(item => item).
                Where(array => array.Count() > 1);
            return duplicates.Count() > 1;
        }
        public static Quaternion AngleToTarget2D(Vector2 objPosition, Vector2 targetPosition)
        {
            Vector3 vectorToTarget = (targetPosition - objPosition).normalized;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(new Vector3(0, 0, angle));
        }
        public static int RandomFromWeightGroup(List<int> weights)
        {
            int summ = 0;
            int curRand = 0;
            foreach (var item in weights)
                summ += item;
            int rand = Random.Range(0, summ + 1);
            for (int i = 0; i < weights.Count; i++)
            {
                curRand += weights[i];
                if (rand <= curRand)
                    return i;
            }
            return 0;
        }

        public static int GetVersionCode()
        {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
            string packageName = context.Call<string>("getPackageName");
            AndroidJavaObject packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode");
        }
        public static string GetVersionName()
        {
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
            string packageName = context.Call<string>("getPackageName");
            AndroidJavaObject packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<string>("versionName");
        }

        public static T CloneClass<T>(T classToClone)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(classToClone));
        }
    }
}
