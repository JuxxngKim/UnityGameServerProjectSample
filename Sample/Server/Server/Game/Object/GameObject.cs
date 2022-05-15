using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}
		public bool IsAlive
		{
			get
			{
				return Info?.StatInfo?.Hp > 0;
			}
		}

		public GameRoom Room { get; set; }

		public ObjectInfo Info { get; set; } = new ObjectInfo();
		public PositionInfo PosInfo { get; private set; } = new PositionInfo();
		public StatInfo Stat { get; private set; } = new StatInfo();
		public Vector3 Position => _position;
		public float Radius => Stat.Radius;

		protected Vector3 _position;
		protected Vector3 _direction;

		protected Vector3 _spawnPosition;
		protected Vector3 _spawnDirection;

		protected float _timeStamp = 0.1f;

		public GameObject()
		{
			Info.PosInfo = PosInfo;
			Info.StatInfo = Stat;

			Info.PosInfo.Position = Vector3.zero.ToFloat3();
			Info.PosInfo.Direction = Vector3.zero.ToFloat3();
			Info.PosInfo.LookDirection = Vector3.back.ToFloat3();
		}

		public virtual void Update()
		{
			if (Room == null)
				return;

			Room?.PushAfter((int)(_timeStamp * 1000), Update);
		}

		public virtual void OnDamaged(BaseActor attacker, int damage)
        {
            if (Room == null || !IsAlive)
                return;

			S_Hit hitPacket = new S_Hit();
			hitPacket.AttackerId = attacker.Id;
			hitPacket.DefenderId = Id;
			hitPacket.Damage = damage;
			Room?.Broadcast(hitPacket);

			Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = Stat.Hp;
			Room?.Broadcast(changePacket);

			if (Stat.Hp <= 0)
			{
				OnDead(attacker);
			}
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;
		}

		public virtual void SyncPos()
		{
			_position = PosInfo.Position.ToVector3();
			_direction = PosInfo.Direction.ToVector3();

			_spawnPosition = _position;
			_spawnDirection = PosInfo.LookDirection.ToVector3();
		}

		public virtual void Remove()
        {

        }
	}
}
