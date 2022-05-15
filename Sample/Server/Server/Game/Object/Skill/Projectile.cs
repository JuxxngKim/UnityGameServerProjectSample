using Google.Protobuf.Protocol;
using Server.Data;
using UnityEngine;

namespace Server.Game.Object
{
    class Projectile : SkillObject
    {
        public override void Init(ObjModel level, BaseActor owner, SkillInfo skillInfo)
        {
            base.Init(level, owner, skillInfo);

            _commandHandle = UpdateCommandProjectile;
        }

        protected virtual void UpdateCommandProjectile()
        {
            var targetPos = _position + _direction;
            var nextPos = Vector3.MoveTowards(_position, targetPos, _timeStamp * Stat.Speed);

            var targets = Room?.IsCollisition(Owener.Id, Info.TeamType, _position, Stat.Radius);
            if (targets == null || targets.Count <= 0)
            {
                _position = nextPos;
                _postProcessHandles.Add(BroadcastMove);
                return;
            }

            _stateEndFrame = 0;
            var target = targets[0];
            target.OnDamaged(this, Stat.Attack);

            _commandHandle = null;
            _stateHandle = null;

            var room = Room;
            room.LeaveGame(Id);
        }

        protected override void BroadcastMove()
        {
            // 다른 플레이어한테도 알려준다
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = Util.Vector3ToPosInfo(_position, _direction);
            movePacket.PosInfo.State = ActorState.Moving;
            Room?.Broadcast(movePacket);
        }
    }
}
