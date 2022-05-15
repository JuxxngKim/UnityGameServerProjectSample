using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace YeongJ.UI
{
    public abstract class UISingletonBase : MonoBehaviour
    {
        abstract public void InitSingleton();
        abstract public void InitUserData();

        public static bool Initialized = false;

        public static void InitSingletons(Transform root)
        {
            UISingletonBase[] ui_list = root.GetComponentsInChildren<UISingletonBase>(true);
            for (int i = 0; i < ui_list.Length; ++i)
            {
                ui_list[i].InitSingleton();
            }

            Initialized = true;
        }
    }

    public static class UISingletonFactory
    {
        public static List<UISingletonBase> SingletonList = new List<UISingletonBase>();

        public static void forEach(UnityAction<UISingletonBase> function)
        {
            for (int i = 0; i < SingletonList.Count; ++i)
            {
                function(SingletonList[i]);
            }
        }
    }

    public class UISingleton<typeT> : UISingletonBase where typeT : UISingleton<typeT>
    {
        private static typeT _instance;

        public static typeT Instance
        {
            get
            {
                return _instance;
            }
        }

        public override void InitSingleton()
        {
            _instance = (typeT)this;
            UISingletonFactory.SingletonList.Add(this);
        }

        public override void InitUserData() { }

        public virtual void Show() { this.gameObject.SetActive(true); }
        public virtual void Hide() { this.gameObject.SetActive(false); }
    }
}