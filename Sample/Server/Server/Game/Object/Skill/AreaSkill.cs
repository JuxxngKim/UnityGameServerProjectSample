using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game.Object
{
    class AreaSkill : SkillObject
    {
        int _hitDelayFrame;
        int _loopTimeFrame;
        bool _isLoopAttack;

        public override void Init(ObjModel level, BaseActor owner, SkillInfo skillInfo)
        {
            base.Init(level, owner, skillInfo);

            _direction = UnityEngine.Vector3.zero;
            PosInfo.Direction = _direction.ToFloat3();

            _commandHandle = UpdateCommandMeteo;
            _hitDelayFrame = _skillData.HitDelayFrame;
            _loopTimeFrame = _skillData.IsLoopSkill ? _skillData.LoopTimeFrame : 0;
        }

        protected virtual void UpdateCommandMeteo()
        {
            if (--_hitDelayFrame > 0)
                return;

            var targets = Room?.IsCollisition(Owener?.Id ?? 0, Info?.TeamType ?? TeamType.Friendly, _position, Stat?.Radius ?? 0.0f) ?? null;
            if (targets == null || targets.Count <= 0)
                return;

            _commandHandle = null;

            foreach (var target in targets)
            {
                if (!target.IsAlive)
                    continue;

                int damage = _isLoopAttack ? _skillData.LoopDamage : Stat.Attack;
                target.OnDamaged(this, damage);
            }

            if (--_loopTimeFrame > 0)
            {
                _commandHandle = UpdateCommandMeteo;
                _isLoopAttack = true;
                _hitDelayFrame = _skillData.LoopHitDelayFrame;
            }
        }
    }
}
