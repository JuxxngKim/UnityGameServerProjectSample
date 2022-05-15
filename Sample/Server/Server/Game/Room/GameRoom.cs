using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Object;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }
        public ObjModel Level { get; private set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, SkillObject> _skills = new Dictionary<int, SkillObject>();

        public void Init(int mapId)
        {
            Level = new ObjModel($"NavMesh{mapId:D2}.obj");
            Level.InitMonsterFile($"MonsterSpawn{mapId:D2}.obj");

            // TODO : Monster Load
            //path = $"Monster{mapId:D2}.obj";

            for(int i = 0; i < Level.MonsterPositions.Count; ++i)
            {
                Vector3 spawnPos = Level.MonsterPositions[i];
                Vector3 spawnAngle = Level.MonsterAngles[i];
                
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Info.Name = $"Monster_{monster.Info.ObjectId}";
                monster.Info.PosInfo.State = ActorState.Idle;
                monster.Info.PosInfo.Position = spawnPos.ToFloat3();
                monster.Info.PosInfo.LookDirection = spawnAngle.ToFloat3();
                monster.Info.PosInfo.Direction = Vector3.zero.ToFloat3();
                monster.Info.TeamType = Const.MonsterTeamType;
                StatInfo stat = DataPresets.MakeChuChuStat(level: 1);
                monster.Stat.MergeFrom(stat);

                monster.SyncPos();
                monster.Init(Level);

                Push(EnterGame, monster, TeamType.Opponent);
            }
        }

        // 누군가 주기적으로 호출해줘야 한다
        public void Update()
        {
            Flush();
        }

        public void EnterGame(GameObject gameObject, TeamType teamType)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (var monster in _monsters.Values)
                    {
                        spawnPacket.Objects.Add(monster.Info);
                    }

                    player.Session.Send(spawnPacket);
                }

                player.Update();
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);

                monster.Room = this;
                monster.Update();
            }
            else if (type == GameObjectType.Skill)
            {
                SkillObject skillObject = gameObject as SkillObject;
                _skills.Add(gameObject.Id, skillObject);

                skillObject.Room = this;
                skillObject.Update();
            }

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                if (!_players.Remove(objectId, out var player))
                    return;

                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                if (!_monsters.Remove(objectId, out var monster))
                    return;

                monster.Room = null;
            }
            else if (type == GameObjectType.Skill)
            {
                if (!_skills.Remove(objectId, out var skillObject))
                    return;

                skillObject.Room = null;
            }

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        public void HandleDance(Player player, C_Dance dancePacket)
        {
            if (player.PosInfo.State != ActorState.Idle)
                return;

            S_Dance sendPacket = new S_Dance();
            sendPacket.ObjectId = player.Id;
            sendPacket.DanceId = dancePacket.DanceId;

            Broadcast(sendPacket);
        }

        public void HandleChangeTeamType(Player player)
        {
            S_ChangeTeam sendPacket = new S_ChangeTeam();
            sendPacket.ObjectId = player.Id;
            sendPacket.TeamType = player.Info.TeamType == TeamType.Friendly ? TeamType.War : TeamType.Friendly;

            player.Info.TeamType = sendPacket.TeamType;

            Broadcast(sendPacket);
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.Position = movePosInfo.Position;
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State == ActorState.Attack)
                return;

            SkillObject skillObject = null;
            int skillId = skillPacket.Info.SkillId;
            if (skillId == -1)
            {
                player.UseTeleportSkill(skillPacket.Info);
                return;
            }

            switch (skillId)
            {
                case -1: player.UseTeleportSkill(skillPacket.Info); return;
                case 1: skillObject = ObjectManager.Instance.Add<Projectile>(); break;
                default: skillObject = ObjectManager.Instance.Add<AreaSkill>(); break;
            }

            skillObject.Init(Level, player, skillPacket.Info);
            player.UseSkill(skillPacket.Info);

            PushAfter(skillObject.SpawnDelay, EnterGame, skillObject, info.TeamType);
        }

        public List<BaseActor> IsCollisition(int thisId, TeamType teamType, Vector3 position, float radius)
        {
            List<BaseActor> targets = new List<BaseActor>();

            if (teamType == TeamType.Friendly)
            {
                var d_enum = _monsters.GetEnumerator();
                while (d_enum.MoveNext())
                {
                    var monster = d_enum.Current.Value;
                    if (!monster.IsAlive)
                        continue;

                    var targetPosition = monster.Position;
                    var dir = targetPosition - position;
                    if (dir.magnitude <= radius + monster.Radius)
                    {
                        targets.Add(monster);
                    }
                }
            }
            else if (teamType == TeamType.Opponent)
            {

            }
            else if (teamType == TeamType.War)
            {
                foreach (Monster m in _monsters.Values)
                {
                    if (!m.IsAlive)
                        continue;

                    var targetPosition = m.Position;
                    var dir = targetPosition - position;
                    if (dir.magnitude <= radius + m.Radius)
                    {
                        targets.Add(m);
                    }
                }

                foreach (Player p in _players.Values)
                {
                    if (!p.IsAlive || p.Id == thisId)
                        continue;

                    var targetPosition = p.Position;
                    var dir = targetPosition - position;
                    if (dir.magnitude <= radius + p.Radius)
                    {
                        targets.Add(p);
                    }
                }
            }

            return targets;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}
