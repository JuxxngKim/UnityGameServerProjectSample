using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YeongJ
{
    public class RendererSortingOrder : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer _renderer;
        [SerializeField] int _addSortingOrder;

        void Awake()
        {
            if (_renderer == null)
                return;

            _renderer.sortingOrder += _addSortingOrder;
        }
    }

}