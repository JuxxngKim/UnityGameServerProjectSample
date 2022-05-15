using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YeongJ
{
    using YeongJ.UI;

    public class MainControl : MonoBehaviour
    {
        [SerializeField] RectTransform _uiRoot;

        public static MainControl Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            UISingletonBase.InitSingletons(_uiRoot);
        }
    }
}