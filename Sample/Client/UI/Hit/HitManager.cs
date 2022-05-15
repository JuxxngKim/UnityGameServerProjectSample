using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YeongJ.Inagme;

namespace YeongJ.UI
{
    public class HitManager : UISingleton<HitManager>
    {
        [SerializeField] DamageFont _templateDamageFont;

        public void AddDamageFont(int objectId, int damage)
        {
            var ownerActor = GetActor(objectId);
            if (ownerActor == null)
                return;

            var worldPosition = ownerActor.UIRoot.transform.position;
            var newDamageFont = GameObjectCache.Make(_templateDamageFont, transform);
            newDamageFont.Init(worldPosition, damage, lifeTime: 0.7f);

            GameObjectCache.DeleteDelayed(newDamageFont.transform, 0.7f);
        }

        public void AddHitEffect(int attackerId, int defenderId)
        {
            var attacker = GetActor(attackerId);
            if (attacker == null)
                return;

            var defender = GetActor(defenderId);
            if (defender == null)
                return;

            var effectRoot = defender.ActorRoot;
            var hitEffect = attacker.GetComponent<BaseActor>()?.HitEffect;
            if (effectRoot == null || hitEffect == null)
                return;
            
            var attackEffect = GameObjectCache.Make(hitEffect.transform, defender.transform.parent);
            attackEffect.transform.position = effectRoot.transform.position;

            GameObjectCache.DeleteDelayed(attackEffect, delayTime: 1.1f);
        }

        private BaseActor GetActor(int objectId)
        {
            return Managers.Object.FindById(objectId)?.GetComponent<BaseActor>();
        }
    }
}