using Google.Protobuf.Protocol;
using Server.Data;
using System.Collections.Generic;

namespace Server.Game.Object
{
    public class SkillObject : BaseActor
    {
        public BaseActor Owener { get; private set; }
        public int SpawnDelay => _spawnDelay;

        protected List<BaseActor> _alreadyAttackTargets;
        protected SkillInfo _skillInfo;
        protected SkillData _skillData;
        protected int _spawnDelay;

        public SkillObject()
        {
            this.ObjectType = GameObjectType.Skill;
            _alreadyAttackTargets = new List<BaseActor>();
        }

        public virtual void Init(ObjModel level, BaseActor owner, SkillInfo skillInfo)
        {
            base.Init(level);
            
            Owener = owner;

            _commandHandle = null;
            _stateHandle = ProcessSkill;
            _skillInfo = skillInfo;

            _position = skillInfo.SpawnPosition.ToVector3();
            _direction = skillInfo.SkillDirection.ToVector3();
            _direction.Normalize();

            PosInfo.Position = _position.ToFloat3();
            PosInfo.Direction = _direction.ToFloat3();

            DataPresets.SkillDatas.TryGetValue(skillInfo.SkillId, out _skillData);
            if (_skillData == null)
                return;

            Info.StatInfo.Attack = _skillData.Damage;
            Info.StatInfo.Speed = _skillData.MoveSpeed;
            Info.StatInfo.Radius = _skillData.Range;
            Info.Name = _skillData.Name;
            Info.TeamType = Owener.Info.TeamType;

            _spawnDelay = _skillData.SpawnDelayTick;
            _stateEndFrame = _skillData.LifeFrame;
        }

        public override void Remove()
        {
            base.Remove();

            _alreadyAttackTargets = null;
        }

        protected override void ProcessSkill()
        {
            if (--_stateEndFrame > 0)
                return;

            _commandHandle = null;
            _stateHandle = null;

            var room = Room;
            room.LeaveGame(Id);
        }
    }
}