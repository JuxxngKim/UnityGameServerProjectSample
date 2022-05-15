using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YeongJ.Inagme;

namespace YeongJ.Inagme
{
    public class SkillObject : BaseActor
    {
        [SerializeField] GameObject _attackEffect;
        [SerializeField] float _heightOffset = 1f;

        public override void Init(int Id)
        {
            base.Init(Id);
        }

        public override void Remove()
        {
            base.Remove();

            if (_attackEffect == null)
                return;

            var attackEffect = GameObjectCache.Make(_attackEffect.transform, this.transform.parent);
            attackEffect.transform.position = transform.position;

            GameObjectCache.DeleteDelayed(attackEffect, delayTime: 1.1f);
        }

        protected override void UpdateHeight()
        {
            var layerMask = LayerMask.NameToLayer("Ground");

            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * _groundedRayDistance, -Vector3.up);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
            {
                var currentPosition = this.transform.position;
                currentPosition.y = hit.point.y + _heightOffset;
                this.transform.position = currentPosition;
            }
        }

        protected override void UpdateRotation(bool isLerp = true) { }
    }
}