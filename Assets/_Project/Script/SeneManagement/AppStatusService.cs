using System;
using UnityEngine;

namespace MeowStudio.SceneManagement
{
    public class AppStatusService: MonoBehaviour
    {
        public Action OnFocus = delegate{};
        public Action OnUnfocus = delegate{};
        public Action OnQuit = delegate{};

        private bool appIsFocused = true;

        private void OnApplicationFocus(bool focusStatus) {
            if(focusStatus && !appIsFocused){
                appIsFocused = true;
                OnFocus?.Invoke();
                Debug.Log("App focused");
            }
            else if(!focusStatus && appIsFocused){
                appIsFocused = false;
                OnUnfocus?.Invoke();
                Debug.Log("App unfocused");
            }
        }
        private void OnApplicationPause(bool pauseStatus) {
            if(!pauseStatus && !appIsFocused){
                appIsFocused = true;
                OnFocus?.Invoke();
                Debug.Log("App focused");
            }
            else if(pauseStatus && appIsFocused){
                appIsFocused = false;
                OnUnfocus?.Invoke();
                Debug.Log("App unfocused");
            }
        }
        private void OnApplicationQuit() {
            if(appIsFocused)
                OnQuit?.Invoke();
        }
    }
}
