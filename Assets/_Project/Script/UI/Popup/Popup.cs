using UnityEngine;
using Zenject;

namespace MeowStudio.UI
{
    public class Popup : MonoBehaviour
    {
        [Inject] private PopupService popupService;

        private IPopupOpener popupOpener;

        protected virtual void Start()
        {
            popupOpener = GetComponent<IPopupOpener>();
            GetComponent<Canvas>().worldCamera = Camera.main;
            UpdatePopup();
            popupOpener.Open();
        }
        protected virtual void UpdatePopup() { }

        public void ClosePopup()=> popupService.ClosePopup(this, 0).Forget();
        public void ClosePopup(float delay)=> popupService.ClosePopup(this, delay).Forget();
        public void DoClosePopup()=> popupOpener.Close();
    }
}
