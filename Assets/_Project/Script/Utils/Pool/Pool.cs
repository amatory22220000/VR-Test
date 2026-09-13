using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MeowStudio.Utils
{
    public class Pool : MonoBehaviour
    {
        [Inject] private DiContainer container;

        private Dictionary<string, List<GameObject>> poolDict = new();
        private Dictionary<string, Transform> parentsDict = new();

        public GameObject GetObject(GameObject obj, Vector3 position)
        {
            GameObject result;

            if (poolDict.ContainsKey(obj.name))
                result = GetObjectFromPool(obj);
            else
                result = CreateObject(obj);
            result.transform.position = position;
            result.SetActive(true);
            return result;
        }
        public GameObject GetObject(GameObject obj, Vector3 position, Quaternion rotation)
        {
            GameObject result = GetObject(obj, position);
            result.transform.rotation = rotation;
            return result;
        }
        public GameObject GetObject(GameObject obj, Vector3 position, Transform parent)
        {
            GameObject result = GetObject(obj, position);
            result.transform.SetParent(parent);
            return result;
        }
        public void ReturnToPool(PoolObject poolObj)
        {
            poolObj.transform.parent = parentsDict[poolObj.name];
            poolObj.gameObject.SetActive(false);
        }


        private GameObject GetObjectFromPool(GameObject obj)
        {
            List<GameObject> poolList = poolDict[obj.name];
            poolList.RemoveAll(item => item == null);
            for (int i = 0; i < poolList.Count; i++)
                if (!poolList[i].activeInHierarchy) return poolList[i];
            GameObject result = CreateObject(obj);
            return result;
        }
        private GameObject CreateObject(GameObject obj)
        {
            if(!poolDict.ContainsKey(obj.name))
            {
                Transform parent = new GameObject(obj.name).transform;
                parent.parent = transform;
                parentsDict.Add(obj.name, parent);
                poolDict.Add(obj.name, new());
            }

            GameObject result = container.InstantiatePrefab(obj, parentsDict[obj.name]);
            result.name = obj.name;
            var poolComponent = result.GetComponent<PoolObject>();
            if (poolComponent == null)
                Debug.LogError($"{result.name} is missing PoolObject component!");
            poolComponent.Initialize(this); poolDict[obj.name].Add(result);


            return result;
        }
        public void ReturnToPoolAll()
        {
            foreach (var item in poolDict)
            {
                for (int i = 0; i < item.Value.Count; i++)
                    item.Value[i].SetActive(false);
            }
        }
    }
}
