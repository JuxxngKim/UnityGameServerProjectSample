using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using UnityEngine;
using YeongJ.Inagme;
using YeongJ.UI;

class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
        HpBarManager.Instance.AddMyHpBar(enterGamePacket.Player.ObjectId);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
        Managers.Object.Clear();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
            HpBarManager.Instance.AddHpBar(obj.ObjectId);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
            HpBarManager.Instance.RemoveHpBar(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        var baseActor = go?.GetComponent<BaseActor>();
        baseActor?.SetServerPos(movePacket.PosInfo);
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;

        GameObject go = Managers.Object.FindById(changePacket.ObjectId);
        if (go == null)
            return;

        BaseActor baseActor = go.GetComponent<BaseActor>();
        if (baseActor == null)
            return;

        baseActor.Stat.Hp = changePacket.Hp;
        HpBarManager.Instance.ChangeHpBar(changePacket.ObjectId, changePacket.Hp);
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        BaseActor baseActor = go?.GetComponent<BaseActor>();
        if (baseActor == null)
            return;

        baseActor.OnDead();
    }

    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        S_Ping pingPacket = packet as S_Ping;
        var latencyTicks = DateTime.UtcNow.Ticks - pingPacket.Time;
        var timeSpan = TimeSpan.FromTicks(latencyTicks);
        float latency = timeSpan.Milliseconds / 1000f;

        var myPlayer = Managers.Object?.MyPlayer;
        if (myPlayer == null)
            return;

        Managers.Network.SetLatency(latency);
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;

        GameObject go = Managers.Object?.FindById(skillPacket.ObjectId);
        BaseActor baseActor = go.GetComponent<BaseActor>();
        baseActor.UseSkill(skillPacket.Info);
    }

    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        if (chatPacket == null)
            return;

        ChatManager.Instance?.AddChat(chatPacket.ObjectId, chatPacket.UserName, chatPacket.Chat);
    }

    public static void S_HitHandler(PacketSession session, IMessage packet)
    {
        S_Hit hitPacket = packet as S_Hit;
        HitManager.Instance.AddDamageFont(hitPacket.DefenderId, hitPacket.Damage);
        HitManager.Instance.AddHitEffect(hitPacket.AttackerId, hitPacket.DefenderId);
    }

    public static void S_DanceHandler(PacketSession session, IMessage packet)
    {
        S_Dance dancePacket = packet as S_Dance;
        GameObject go = Managers.Object?.FindById(dancePacket.ObjectId);
        go?.GetComponent<BaseActor>()?.OnDance();
    }


    public static void S_ResurrectionHandler(PacketSession session, IMessage packet)
    {
        S_Resurrection resurrectionPacekt = packet as S_Resurrection;
        GameObject go = Managers.Object?.FindById(resurrectionPacekt.ObjectId);
        BaseActor baseActor = go?.GetComponent<BaseActor>();

        if (baseActor == null)
            return;

        baseActor.gameObject.SetActive(false);
        baseActor.gameObject.SetActive(true);
        baseActor.SetServerPos(resurrectionPacekt.Player.PosInfo);
        baseActor.SyncPos();
        baseActor.OnResurrection();
        baseActor.Stat.Hp = resurrectionPacekt.Player.StatInfo.Hp;

        HpBarManager.Instance.ChangeHpBar(resurrectionPacekt.ObjectId, resurrectionPacekt.Player.StatInfo.Hp);
    }

    public static void S_ChangeTeamHandler(PacketSession session, IMessage packet)
    {
        S_ChangeTeam changePacket = packet as S_ChangeTeam;

        GameObject go = Managers.Object?.FindById(changePacket.ObjectId);
        BaseActor baseActor = go?.GetComponent<BaseActor>();
        if (baseActor == null)
            return;

        baseActor.SetTeamType(changePacket.TeamType);
        if (baseActor is MyPlayer)
        {
            HpBarManager.Instance.SetPlayerTeamType(changePacket.TeamType);
            OthersUI.Instance.RefreshTeamType(changePacket.TeamType);
        }
    }
}