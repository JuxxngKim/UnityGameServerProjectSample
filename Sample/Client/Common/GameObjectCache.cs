using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YeongJ
{
    public class GameObjectCache : MonoBehaviour
    {
        public class Cache
        {
            public int sourceInstanceID;
            public string name;
            public Transform root;
            public Stack<Transform> freeStack;
        }

        private Transform _root;
        private Dictionary<int, Cache> _cacheMap;
        private Dictionary<int, Cache> _cacheMapForInstances;

        private static GameObjectCache _instance;

        public static GameObjectCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject newObject = new GameObject("GameObjectCache");
                    _instance = newObject.AddComponent<GameObjectCache>();
                    _instance.InitSingleton();
                }

                return _instance;
            }
        }

        void InitSingleton()
        {
            _cacheMap = new Dictionary<int, Cache>();
            _cacheMapForInstances = new Dictionary<int, Cache>();
        }

        void OnDestroy()
        {
            _instance = null;
            _cacheMap.Clear();
        }

        public void Reserve(Transform source, int count)
        {
            Cache cache = PrepareCache(source);

            for (int i = 0; i < count; ++i)
            {
                GameObject clone = GameObject.Instantiate(source.gameObject);
                clone.gameObject.SetActive(false);
                _cacheMapForInstances.Add(clone.GetInstanceID(), cache);

                clone.transform.SetParent(cache.root, false);

                if (!cache.freeStack.Contains(clone.transform))
                {
                    cache.freeStack.Push(clone.transform);
                }
            }
        }

        public Transform Make(Transform source)
        {
            Cache cache = PrepareCache(source);

            Transform newTransform = null;

            if (cache.freeStack.Count <= 0)
            {
                GameObject newOne = GameObject.Instantiate(source.gameObject);
                newTransform = newOne.transform;
                newOne.gameObject.SetActive(true);

                _cacheMapForInstances.Add(newOne.GetInstanceID(), cache);
            }
            else
            {
                newTransform = cache.freeStack.Pop();
                newTransform.SetParent(null, false);

                Init(newTransform);
            }

            return newTransform;
        }

        public static typeT Make<typeT>(typeT source, Transform parent) where typeT : Component
        {
            Transform new_t = Instance.Make(source.transform);
            new_t.SetParent(parent, false);
            new_t.gameObject.SetActive(true);

            typeT script = new_t.GetComponent<typeT>();

            return script;
        }

        public void Init(Transform tr)
        {
            tr.gameObject.SetActive(true);
        }

        public static void Delete(Transform clone)
        {
            if (clone == null || clone.gameObject == null)
            {
                return;
            }

            clone.gameObject.SetActive(false);

            Cache cache = null;
            Instance._cacheMapForInstances.TryGetValue(clone.gameObject.GetInstanceID(), out cache);
            if (cache == null || cache.root == null)
            {
                GameObject.Destroy(clone.gameObject);
                return;
            }

            clone.SetParent(cache.root, false);

            if (!cache.freeStack.Contains(clone))
            {
                cache.freeStack.Push(clone);
            }
        }

        public static void DeleteDelayed(Transform tr, float delayTime)
        {
            Instance.StartCoroutine(Instance._DeleteDelayed(tr, delayTime));
        }

        public IEnumerator _DeleteDelayed(Transform tr, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            Delete(tr);
        }

        private Cache PrepareCache(Transform source)
        {
            Cache cache = null;
            if (!_cacheMap.TryGetValue(source.GetInstanceID(), out cache))
            {
                GameObject newObject = new GameObject(source.GetInstanceID().ToString());
                newObject.SetActive(false);

                cache = new Cache();
                cache.sourceInstanceID = source.GetInstanceID();
                cache.name = source.gameObject.name;
                cache.root = newObject.transform;
                cache.root.SetParent(_root, false);
                cache.freeStack = new Stack<Transform>();

                _cacheMap.Add(source.GetInstanceID(), cache);
            }
            return cache;
        }

        public void PurgeAllCache()
        {
            List<Cache> removeList = new List<Cache>();

            var d_enum = _cacheMap.GetEnumerator();
            while (d_enum.MoveNext())
            {
                removeList.Add(d_enum.Current.Value);
            }

            foreach(var cache in removeList)
            {
                PurgeCache(cache);
            }

            removeList.Clear();
            
            _cacheMap.Clear();
            _cacheMapForInstances.Clear();
        }

        public void PurgeCache(Transform source)
        {
            Cache cache = null;
            if (!_cacheMap.TryGetValue(source.GetInstanceID(), out cache))
            {
                return;
            }

            PurgeCache(cache);
        }

        private void PurgeCache(Cache cache)
        {
            GameObject.Destroy(cache.root.gameObject);

            cache.root = null;
            cache.freeStack = null;

            _cacheMap.Remove(cache.sourceInstanceID);
        }
    }
}