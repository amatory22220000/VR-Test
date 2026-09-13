using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeowStudio.SceneManagement
{
    public class SceneLoader: MonoBehaviour
    {
        [SerializeField] private GameObject loaderObj; 

        public void LoadScene(SceneNames scene)
        {
            LoadSceneAsync(scene.ToString()).Forget();
        }
        public void ReloadScene()
        {
            LoadSceneAsync(SceneManager.GetActiveScene().name).Forget();
        }

        private async UniTaskVoid LoadSceneAsync(string sceneName)
        {
            ShowLoader();
            await UniTask.WaitForSeconds(0.1f);
            await SceneManager.LoadSceneAsync(sceneName);
            HideLoader();
        }

        public void ShowLoader()
        {
            loaderObj.SetActive(true);
        }
        public void HideLoader()
        {
            loaderObj.SetActive(false);
        }
    }
}
