using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using NetworkBase;
using UDPClient;

public class PlayerInfo
{
    public Transform LocalTransform;
    public Transform ServerTransform;
}
public class StateSyncManager : MonoBehaviour
{
    UDPClient_Demo m_UDPClient_Demo;
    public bool NewPacket;
    public int PlayerID;
    public Transform LocalPlayer;
    public Transform ServerPlayer;
    public Queue<StateSyncPacket> SyncDataQueue;
    public Dictionary<int,PlayerInfo> Players;
    public GameObject LocalPlayerPrefab;
    public GameObject ServerPlayerPrefab;
    private int times = 0;
    public int LocalX;
    public int LocalY;
    public int LocalZ;
    private void Awake()
    {
        PlayerID = -1;
        Players = new Dictionary<int, PlayerInfo>();
        NewPacket = false;
        SyncDataQueue=new Queue<StateSyncPacket>();
        m_UDPClient_Demo = new UDPClient_Demo(this);
        PacketFactoryBase factory = new StateSynchronizationFactory();
        factory.Init();
        if(!m_UDPClient_Demo.Init(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8880)))
        {
            Debug.Log("Client init fail!");
        }
    }

    void Start()
    {
        Application.targetFrameRate = 50;
        m_UDPClient_Demo.Start();
        m_UDPClient_Demo.Send(new PlayerRequestIDPacket());
    }

    private void OnApplicationQuit()
    {
        m_UDPClient_Demo.Stop();
    }

    void Update()
    {
        if(PlayerID==-1){return;}

        if (NewPacket)
        {
            UpdateOtherPlayersTarget();
        }

        UpdateLocalPlayer();

        UpdateOtherPlayersRender();
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
                m_UDPClient_Demo.Send(new PlayerInputPacket { m_X = applyX, m_Z = applyZ });
                LocalX+=applyX * 5 * 2;
                LocalZ+=applyZ * 5 * 2;
                LocalPlayer.position = new Vector3((float)LocalX/10000, (float)LocalY/10000, (float)LocalZ/10000);
            }
        }
    }
    public void UpdateOtherPlayersRender()
    {
        foreach (var VARIABLE in Players)
        {
            if (VARIABLE.Key != PlayerID)
            {
                VARIABLE.Value.LocalTransform.position = VARIABLE.Value.ServerTransform.position;
            }
        }
    }
    public void UpdateOtherPlayersTarget()
    {
        while (SyncDataQueue.Count > 0)
        {
            var data = SyncDataQueue.Dequeue();
            foreach (var player in data.Players)
            {
                
                if (player.PlayerId == PlayerID)
                {
                    CheckLocalPlayer(player);
                    continue;
                }
                PlayerInfo info=null;
                if(!Players.TryGetValue(player.PlayerId, out info))
                {
                    info = new PlayerInfo();
                    GameObject local = Instantiate(LocalPlayerPrefab);
                    GameObject server = Instantiate(ServerPlayerPrefab);
                    info.LocalTransform = local.transform;
                    info.ServerTransform = server.transform;
                    Players[player.PlayerId] = info;
                }
                info.ServerTransform.position = new Vector3(player.x, player.y, player.z);
            }
            List<int> playersToRemove=null;
            foreach (var local in Players)
            {
                var da = data.Players.Find(p => p.PlayerId == local.Key);
                if (da == null)
                {
                    if (playersToRemove == null)
                    {
                        playersToRemove=new List<int>();
                    }
                    playersToRemove.Add(local.Key);
                }
            }

            if (playersToRemove != null)
            {
                foreach (var VARIABLE in playersToRemove)
                {
                    RemovePlayer(VARIABLE);
                }
            }
        }
    }

    public void RemovePlayer(int id)
    {
        Destroy(Players[id].LocalTransform.gameObject);
        Destroy(Players[id].ServerTransform.gameObject);
        Players.Remove(id);
    }
    public void CheckLocalPlayer(PlayerState state)
    {
        if (LocalPlayer == null)
        {
            GameObject local = Instantiate(LocalPlayerPrefab);
            GameObject server = Instantiate(ServerPlayerPrefab);
            LocalPlayer = local.transform;
            ServerPlayer = server.transform;
            LocalPlayer.transform.position = new Vector3((float)state.m_X/10000, (float)state.m_Y/10000, (float)state.m_Z/10000);
            ServerPlayer.transform.position = new Vector3((float)state.m_X/10000, (float)state.m_Y/10000, (float)state.m_Z/10000);
            LocalX=state.m_X;
            LocalY=state.m_Y;
            LocalZ=state.m_Z;
        }
        else
        {
            ServerPlayer.transform.position = new Vector3(state.x, state.y, state.z);
        }
    }
    public void SetPlayerID(int id)
    {
        PlayerID = id;
    }
    
}
