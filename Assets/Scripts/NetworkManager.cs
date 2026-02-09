using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
       ConnectToServer(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void ConnectToServer(){
		PhotonNetwork.ConnectUsingSettings();
		Debug.Log("Yo!!! Try Connect to Server");
		
	}
	public override void OnConnectedToMaster()
	{
		Debug.Log("I am connected to Server");
		base.OnConnectedToMaster();
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 10;
		roomOptions.IsVisible = true;
		roomOptions.PublishUserId = true;
		roomOptions.IsOpen = true;
		PhotonNetwork.JoinOrCreateRoom("RoomMrMcLaughlin",roomOptions,TypedLobby.Default);
	}
	public override void OnJoinedRoom(){
		Debug.Log("Yeah we are in a room! Welcome player ");
		base.OnJoinedRoom();
		
	}
	
	
	
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
			Debug.Log("A new player is in the room now");
			base.OnPlayerEnteredRoom(newPlayer);
	}
	
	public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon. Cause: " + cause.ToString());

        // Handle what happens after disconnection
    }
	
}
