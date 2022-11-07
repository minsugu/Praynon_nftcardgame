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


    #region 로그인
    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    public override void OnDisconnected(DisconnectCause cause) => Managers.Scene.LoadScene(Define.Scene.Login);
    #endregion


    #region 로비
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


    #region 방
    public override void OnJoinedRoom() =>  Managers.Scene.LoadScene(Define.Scene.MultiGame);
    public override void OnCreateRoomFailed(short returnCode, string message) { /*CreateRoom(""); */}
    public override void OnJoinRandomFailed(short returnCode, string message) { CreateRoom(""); }
    public override void OnPlayerEnteredRoom(Player newPlayer) => OnUpdateRoom(newPlayer.NickName, true);
    public override void OnPlayerLeftRoom(Player otherPlayer) => OnUpdateRoom(otherPlayer.NickName, false);
    public void LeaveRoom()
    {
        PullRoomList(); // 방 떠나기 전에 서브 클라이언트로 룸 리스트 업데이트 하고 나가기..
        PhotonNetwork.LeaveRoom();
    }
    #endregion


    #region 서브 클라이언트
    public void PullRoomList()
    {
        RoomPuller roomPoller = gameObject.AddComponent<RoomPuller>();
        roomPoller.OnGetRoomsInfo
        (
            (roomInfos) =>
            {
                // 룸리스트를 받고나서 myList에 넣기
                myList = roomInfos;

                // 마지막엔 오브젝트 제거해주기
                Destroy(roomPoller);
            }
        );
    }
    #endregion
}
