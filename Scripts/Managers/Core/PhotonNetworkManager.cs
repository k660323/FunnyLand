using BackEnd;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PhotonNetworkManager
{
    public string Version { get; private set; } = "1.0.2";
    public int SerializationRate { get; private set; } = 30;
    public int SendRate { get; private set; } = 30;

    #region Ä¿½ºÅÒ Å¸ÀÔ 
    public static readonly byte[] memColor = new byte[4 * 4];
    private static short SerializeColor(StreamBuffer outStream, object customobject)
    {
        Color co = (Color)customobject;
        lock (memColor)
        {
            byte[] bytes = memColor;
            int index = 0;
            Protocol.Serialize(co.r, bytes, ref index);
            Protocol.Serialize(co.g, bytes, ref index);
            Protocol.Serialize(co.b, bytes, ref index);
            Protocol.Serialize(co.a, bytes, ref index);
            outStream.Write(bytes, 0, 4 * 4);
        }

        return 4 * 4;
    }


    private static object DeserializeColor(StreamBuffer inStream, short length)
    {
        Color co = new Color();
        lock (memColor)
        {
            inStream.Read(memColor, 0, 4 * 4);
            int index = 0;
            Protocol.Deserialize(out co.r, memColor, ref index);
            Protocol.Deserialize(out co.g, memColor, ref index);
            Protocol.Deserialize(out co.b, memColor, ref index);
            Protocol.Deserialize(out co.a, memColor, ref index);
        }

        return co;
    }
    #endregion

    public void Init()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Version;
        PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ConnectionProtocol.Udp;
        PhotonNetwork.GameVersion = Version;
        PhotonNetwork.SerializationRate = SerializationRate;
        PhotonNetwork.SendRate = SendRate;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonPeer.RegisterType(typeof(Color), 0, SerializeColor, DeserializeColor);

        InitPlayerProperties();
    }

    public void InitPlayerProperties()
    {
        Hashtable playerTable = new Hashtable();
        playerTable["Team"] = "";
        playerTable["Load"] = false;
        playerTable["Die"] = false;
        playerTable["SpawnOK"] = false;
        playerTable["Score"] = 0;
        playerTable["Kill"] = 0;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerTable);
    }

    public void SetPlayerPropertie<T>(string key, T value)
    {
        Hashtable playerTable = new Hashtable() { { key, value } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerTable);
    }

    public void InitPlayerGameProperties()
    {
        Hashtable playerTable = new Hashtable();
        playerTable["Load"] = false;
        playerTable["Die"] = false;
        playerTable["SpawnOK"] = false;
        playerTable["Kill"] = 0;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerTable);
    }

    public RoomOptions InitRoomProperties(string _RoomName, string _PW, int _CurRound, int _Round, string _MapSelect, bool _Team, bool _TeamKill, byte _MaxPlayer, bool _RoomVisible)
    {
        RoomOptions roomOptions = new RoomOptions();

        Hashtable roomTable = new Hashtable();
        roomTable.Add("RoomName", _RoomName);
        roomTable.Add("PW", _PW);
        roomTable.Add("CurRound", _CurRound);
        roomTable.Add("Round", _Round);
        roomTable.Add("MapSelect", _MapSelect);
        roomTable.Add("Team", _Team);
        roomTable.Add("TeamKill", _TeamKill);

        roomOptions.MaxPlayers = _MaxPlayer;
        roomOptions.IsVisible = _RoomVisible;

        roomOptions.CustomRoomProperties = roomTable;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "RoomName", "PW", "Round", "MapSelect", "Team", "TeamKill" };

        return roomOptions;
    }

    public Hashtable SetRoomProperties(string _RoomName, string _PW, int _Round, string _MapSelect, bool _Team, bool _TeamKill, byte _MaxPlayer, bool _RoomVisible)
    {
        if (PhotonNetwork.CurrentRoom.MaxPlayers != _MaxPlayer)
            PhotonNetwork.CurrentRoom.MaxPlayers = _MaxPlayer;
        if (PhotonNetwork.CurrentRoom.IsVisible != _RoomVisible)
            PhotonNetwork.CurrentRoom.IsVisible = _RoomVisible;

        Hashtable roomTable = new Hashtable();
        if (PhotonNetwork.CurrentRoom.CustomProperties["RoomName"].ToString() != _RoomName)
            roomTable["RoomName"] = _RoomName;
        if (PhotonNetwork.CurrentRoom.CustomProperties["PW"].ToString() != _PW)
            roomTable["PW"] = _PW;
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["Round"] != _Round)
            roomTable["Round"] = _Round;
        if (PhotonNetwork.CurrentRoom.CustomProperties["MapSelect"].ToString() != _MapSelect)
            roomTable["MapSelect"] = _MapSelect;
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["Team"] != _Team)
            roomTable["Team"] = _Team;
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamKill"] != _TeamKill)
            roomTable["TeamKill"] = _TeamKill;

        return roomTable;
    }

    public void SetRoomPropertie<T>(string key, T value)
    {
        Hashtable roomTable = new Hashtable() { { key, value } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
    }

    public int[] BoolPropertieCount(string key, bool isTrue)
    {
        int[] count = new int[2] { 0, 0 };

        count[0] = PhotonNetwork.CurrentRoom.PlayerCount;
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            if (PhotonNetwork.PlayerList[i].CustomProperties[key].GetType() != typeof(bool))
                return null;

            if ((bool)PhotonNetwork.PlayerList[i].CustomProperties[key] == isTrue)
                count[1]++;
        }

        return count;
    }

    public int[,] BoolPropertieCountTeam(string key, bool isTrue)
    {
        // 0,0 (·¹µåÆÀ true) 0,1 (·¹µåÆÀ false)
        // 1,0(ºí·çÆÀ true) 1,1 (ºí·çÆÀ false)
        int[,] teamCount = new int[2, 2] { { 0, 0 }, { 0, 0 } };

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            if (PhotonNetwork.PlayerList[i].CustomProperties[key].GetType() != typeof(bool))
                return null;

            if (PhotonNetwork.PlayerList[i].CustomProperties["Team"].ToString() == "RedTeam")
            {
                teamCount[0, 0]++;
                if ((bool)PhotonNetwork.PlayerList[i].CustomProperties[key] == isTrue)
                    teamCount[0, 1]++;
            }
            else if (PhotonNetwork.PlayerList[i].CustomProperties["Team"].ToString() == "BlueTeam")
            {
                teamCount[1, 0]++;
                if ((bool)PhotonNetwork.PlayerList[i].CustomProperties[key] == isTrue)
                    teamCount[1, 1]++;
            }

        }

        return teamCount;
    }

    public void DestoryAllPlayerObjects(bool isLocal = false)
    {
        if (!PhotonNetwork.InRoom)
            return;

        if(isLocal)
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.PlayerList[i].ActorNumber, true);
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.PlayerList[i]);
            }
        }
    }
}
