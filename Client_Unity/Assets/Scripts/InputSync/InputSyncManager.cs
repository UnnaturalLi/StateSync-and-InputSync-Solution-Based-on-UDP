using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using NetworkBase;
using UDPClient;

public class InputSyncManager : MonoBehaviour
{
    public UDPClient_InputSyncDemo m_UDPClient_InputSyncDemo;
    public bool NewPacket;
    public int PlayerID;
    public Transform LocalPlayer;
    public Queue<InputSyncPacket> SyncDataQueue;
    public Queue<StateSyncPacket> StateDataQueue;
    public Queue<DropClientPacket> DropClientQueue;
    public Dictionary<int,Transform> Players;
    public GameObject LocalPlayerPrefab;
    public int LocalX;
    public int LocalY;
    public int LocalZ;
    private void Awake()
    {
        string IP="";
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ipConfig.txt");
        if (File.Exists(configPath))
        {
            IP = File.ReadAllText(configPath);
        }
        else
        {
            File.Create(configPath);
            Debug.Log(configPath);
        }
        PlayerID = -1;
        m_UDPClient_InputSyncDemo = new UDPClient_InputSyncDemo(this);
        StateDataQueue = new Queue<StateSyncPacket>();
        Players = new Dictionary<int, Transform>();
        NewPacket = false;
        SyncDataQueue=new Queue<InputSyncPacket>();
        DropClientQueue=new Queue<DropClientPacket>();
        PacketFactoryBase factory = new InputSynchronizationFactory();
        factory.Init();
        if(!m_UDPClient_InputSyncDemo.Init(new IPEndPoint(IPAddress.Parse(IP), 8880)))
        {
            Debug.Log("Client init fail!");
        }
    }

    void Start()
    {
        m_UDPClient_InputSyncDemo.Start();
        m_UDPClient_InputSyncDemo.Send(new PlayerRequestIDPacket());
    }

    private void OnApplicationQuit()
    {
        m_UDPClient_InputSyncDemo.Stop();
    }

    void FixedUpdate()
    {
        if(PlayerID==-1){return;}
        if (NewPacket)
        {
            UpdateOtherPlayers();
        }
        UpdateLocalPlayer();
    }

    public void UpdateOtherPlayers()
    {
        while (StateDataQueue.Count > 0)
        {
            var data= StateDataQueue.Dequeue();
            foreach (var player in data.Players)
            {
                if (!Players.ContainsKey(player.PlayerId)&& player.PlayerId!=PlayerID)
                {
                    Players[player.PlayerId] = Instantiate(LocalPlayerPrefab,new Vector3(player.x,player.y,player.z),Quaternion.identity).transform;
                }
            }
        }
        while (SyncDataQueue.Count > 0)
        {
            var data= SyncDataQueue.Dequeue();
            foreach (var InputPacket in data.PlayersInput)
            {
                if (InputPacket.PlayerId == PlayerID)
                {
                    continue;
                }
                if (!Players.ContainsKey(InputPacket.PlayerId))
                {
                    Players[InputPacket.PlayerId] = Instantiate(LocalPlayerPrefab,new Vector3(0,0,0),Quaternion.identity).transform;
                }
                Players[InputPacket.PlayerId].position += new Vector3(InputPacket.m_X*0.001f, 0,InputPacket.m_Z*0.001f);
            }
        }
        while (DropClientQueue.Count > 0)
        {
            var DropPacket = DropClientQueue.Dequeue();
            foreach (var id in DropPacket.idList)
            {
                RemovePlayer(id);
            }
        }
    }
    public void UpdateLocalPlayer()
    {
        if (LocalPlayer != null)
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical"); 
            inputX = (float)Math.Round(inputX, 2);
            inputY = (float)Math.Round(inputY, 2);
            if (Mathf.Abs(inputX) > float.Epsilon || Mathf.Abs(inputY) > float.Epsilon)
            {
                int applyX=(int)(inputX * 100);
                int applyZ = (int)(inputY * 100);
                m_UDPClient_InputSyncDemo.Send(new PlayerInputPacket { m_X = applyX, m_Z = applyZ ,PlayerId = PlayerID});
                LocalX+=applyX * 5 * 2;
                LocalZ+=applyZ * 5 * 2;
                LocalPlayer.position = new Vector3((float)LocalX/10000, (float)LocalY/10000, (float)LocalZ/10000);
            }
        }
        else
        {
            LocalPlayer=Instantiate(LocalPlayerPrefab,new Vector3(0,0,0),Quaternion.identity).transform;
        }
    }
    public void RemovePlayer(int id)
    {
        Destroy(Players[id].gameObject);
        Players.Remove(id);
    }
    public void SetPlayerID(int id)
    {
        PlayerID = id;
    }

    public void RegisterNewClient(RegisterNewClientPacket packet)
    {
        
    }
}
