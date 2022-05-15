using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Server.Game
{
	public class Player : BaseActor
	{
		public ClientSession Session { get; set; }

		public Player()
		{
			ObjectType = GameObjectType.Player;
		}

		protected override void ProcessSkill()
		{
			if (--_stateEndFrame > 0)
				return;

			_stateHandle = null;
			_commandHandle = UpdateCommandIdleMove;

			if(_stateEndHandle != null)
            {
				_stateEndHandle();
				_stateEndHandle = null;
			}

			PosInfo.State = ActorState.Idle;
			Room.Push(BroadcastMove);
		}

		public void UseTeleportSkill(SkillInfo skillInfo)
		{
			UseSkill(skillInfo);
			_stateEndHandle = () =>
			{
				PosInfo.Position = skillInfo.SpawnPosition;
				_position = PosInfo.Position.ToVector3();
			};
		}

		public override void OnDead(GameObject attacker)
		{
			base.OnDead(attacker);

			GameRoom room = Room;
			room.PushAfter(8000, RespawnGame, room);
		}

		protected override void RespawnGame(GameRoom room)
		{
			base.RespawnGame(room);

			_position = _spawnPosition;
			_direction = Vector3.zero;

			Stat.Hp = Stat.MaxHp;
			PosInfo.State = ActorState.Idle;
			PosInfo.Position = _position.ToFloat3();
			PosInfo.Direction = _direction.ToFloat3();
			PosInfo.LookDirection = _spawnDirection.ToFloat3();
			Info.TeamType = Info.TeamType;
			Info.PosInfo = PosInfo;

			SyncPos();
			Init(Level);

			S_Resurrection resurrectionPacket = new S_Resurrection();
			resurrectionPacket.ObjectId = Id;
			resurrectionPacket.Player = Info;

			Room?.Broadcast(resurrectionPacket);
		}
	}
}
