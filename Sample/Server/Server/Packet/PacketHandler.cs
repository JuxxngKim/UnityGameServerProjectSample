using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_PongHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandlePong();
	}

	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null || player.PosInfo.State == ActorState.Attack)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

        room.Push(room.HandleMove, player, movePacket);
    }

	public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null || player.PosInfo.State == ActorState.Attack)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleSkill, player, skillPacket);
    }

    public static void C_ChatHandler(PacketSession session, IMessage packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession?.MyPlayer;
        GameRoom room = player?.Room;
        if (room == null)
            return;

        S_Chat sendPacket = new S_Chat();
        sendPacket.ObjectId = player?.Id ?? 0;
        sendPacket.UserName = player?.Info.Name ?? string.Empty;
        sendPacket.Chat = chatPacket.Chat;

        if(player != null)
        {
            Console.WriteLine($"{player.Id}: {chatPacket.Chat}");
        }

        room?.Push(room.Broadcast, sendPacket);
    }

    public static void C_DanceHandler(PacketSession session, IMessage packet)
    {
        C_Dance dancePacket = packet as C_Dance;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession?.MyPlayer;
        GameRoom room = player?.Room;
        if (room == null)
            return;

        room.Push(room.HandleDance, player, dancePacket);
    }

    public static void C_ChangeTeamHandler(PacketSession session, IMessage packet)
    {
        C_ChangeTeam changePacket = packet as C_ChangeTeam;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession?.MyPlayer;
        GameRoom room = player?.Room;
        if (room == null)
            return;

        room.Push(room.HandleChangeTeamType, player);
    }

}
