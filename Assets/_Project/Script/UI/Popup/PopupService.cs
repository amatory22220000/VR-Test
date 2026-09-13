using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace MeowStudio.UI
{
    public class PopupService : MonoBehaviour
    {
        [SerializeField] private PopupHolder popupHolder;

        [Inject] private DiContainer container;

        private readonly Dictionary<Type, GameObject> popupPrefabs = new();
        private readonly List<PopupData> openPopups = new();

        public bool isOpenPopup => openPopups.Count > 0;

        public void Initialize()
        {
            popupPrefabs.Clear();
            foreach (var popup in popupHolder.popupList)
            {
                var type = popup.GetType();
                if (!popupPrefabs.ContainsKey(type))
                    popupPrefabs.Add(type, popup.gameObject);
                else
                    Debug.LogWarning($"Duplicate popup type: {type.Name} in PopupHolder.");
            }
        }

        public TPopup ShowPopup<TPopup>(Action onClose = null) where TPopup : Popup
        {
            var type = typeof(TPopup);

            if (!popupPrefabs.TryGetValue(type, out var prefab))
            {
                Debug.LogError($"Popup of type {type.Name} not found in PopupHolder.");
                return null;
            }

            TPopup instance = container
                .InstantiatePrefab(prefab, transform)
                .GetComponent<TPopup>();

            if (instance == null)
            {
                Debug.LogError($"Instantiated prefab does not contain component of type {type.Name}.");
                return null;
            }

            openPopups.Add(new PopupData(instance, onClose));
            return instance;
        }
        public async UniTask ShowPopupAsync<TPopup>() where TPopup : Popup
        {
            var tcs = new UniTaskCompletionSource();
            var popup = ShowPopup<TPopup>(() => tcs.TrySetResult());
            if (popup == null)
            {
                tcs.TrySetCanceled();
                return;
            }
            await tcs.Task;
        }

        public void CloseLastPopup(float delay = 0)
        {
            if (openPopups.Count == 0) return;
            ClosePopup(openPopups[^1].popup, delay).Forget();
        }

        public async UniTaskVoid ClosePopup(Popup popup, float delay)
        {
            var popupData = openPopups.FirstOrDefault(x => x.popup == popup);

            if (popupData == null)
            {
                Debug.LogError($"Trying to close popup that is not open: {popup.name}");
                return;
            }

            openPopups.Remove(popupData);

            await UniTask.WaitForSeconds(delay);

            popupData.onClose?.Invoke();
            popupData.popup.DoClosePopup();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                CloseLastPopup();
        }
    }

    [Serializable]
    public class PopupData
    {
        public Popup popup;
        public Action onClose;
        public PopupData(Popup popup, Action onClose)
        {
            this.onClose = onClose;
            this.popup = popup;
        }
    }
}
