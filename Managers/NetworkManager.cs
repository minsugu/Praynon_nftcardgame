using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks 
{
    static NetworkManager s_instance;
    public static NetworkManager Inst { get { Init(); return s_instance; } } 

    public static Action OnUpdateRoomList;
    public static Action<string, bool> OnUpdateRoom;

    public List<RoomInfo> myList = new List<RoomInfo>();

    void Start()
    {
        Init();
    }

    void Update()
    {

    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@NetworkManager");
            if (go == null)
            {
                go = new GameObject { name = "@NetworkManager" };
                go.AddComponent<NetworkManager>();
            }

            if (go != null)
                DontDestroyOnLoad(go);

            s_instance = go.GetComponent<NetworkManager>();
        }  

    }


    #region �α���
    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    public override void OnDisconnected(DisconnectCause cause) => Managers.Scene.LoadScene(Define.Scene.Login);
    #endregion


    #region �κ�
    public override void OnJoinedLobby() => Managers.Scene.LoadScene(Define.Scene.Lobby);
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        OnUpdateRoomList?.Invoke();
    }
    public void SinglePlay() => PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.NickName + "'s Single Room ", new RoomOptions { MaxPlayers = 1 });
    public void CreateRoom(string roomName) => PhotonNetwork.CreateRoom(roomName == "" ? PhotonNetwork.LocalPlayer.NickName + "'s Room " + UnityEngine.Random.Range(1, 11).ToString() : roomName, new RoomOptions { MaxPlayers = 2 });
    public void JoinRoom(string roomName) => PhotonNetwork.JoinRoom(roomName);
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    #endregion


    #region ��
    public override void OnJoinedRoom() =>  Managers.Scene.LoadScene(Define.Scene.MultiGame);
    public override void OnCreateRoomFailed(short returnCode, string message) { /*CreateRoom(""); */}
    public override void OnJoinRandomFailed(short returnCode, string message) { CreateRoom(""); }
    public override void OnPlayerEnteredRoom(Player newPlayer) => OnUpdateRoom(newPlayer.NickName, true);
    public override void OnPlayerLeftRoom(Player otherPlayer) => OnUpdateRoom(otherPlayer.NickName, false);
    public void LeaveRoom()
    {
        PullRoomList(); // �� ������ ���� ���� Ŭ���̾�Ʈ�� �� ����Ʈ ������Ʈ �ϰ� ������..
        PhotonNetwork.LeaveRoom();
    }
    #endregion


    #region ���� Ŭ���̾�Ʈ
    public void PullRoomList()
    {
        RoomPuller roomPoller = gameObject.AddComponent<RoomPuller>();
        roomPoller.OnGetRoomsInfo
        (
            (roomInfos) =>
            {
                // �븮��Ʈ�� �ް��� myList�� �ֱ�
                myList = roomInfos;

                // �������� ������Ʈ �������ֱ�
                Destroy(roomPoller);
            }
        );
    }
    #endregion
}
