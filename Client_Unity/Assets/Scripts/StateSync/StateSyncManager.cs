using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using NetworkBase;
using UDPClient;

public class PlayerInfo
{
    public Transform LocalTransform;
    public Transform ServerTransform;
    public float totalTime;
    public float simTime;
}
public class StateSyncManager : MonoBehaviour
{
    UDPClient_StateSyncDemo m_UDPClientStateSyncDemo;
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
    private StateSyncPacket m_LastPacket;
    public bool ShowServer;
    private DateTime m_LastInputTime;
    
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
        Players = new Dictionary<int, PlayerInfo>();
        NewPacket = false;
        SyncDataQueue=new Queue<StateSyncPacket>();
        m_UDPClientStateSyncDemo = new UDPClient_StateSyncDemo(this);
        PacketFactoryBase factory = new StateSynchronizationFactory();
        factory.Init();
        if(!m_UDPClientStateSyncDemo.Init(new IPEndPoint(IPAddress.Parse(IP), 8880)))
        {
            Debug.Log("Client init fail!");
        }
    }

    void Start()
    {
        m_UDPClientStateSyncDemo.Start();
        m_UDPClientStateSyncDemo.Send(new PlayerRequestIDPacket());
    }

    private void OnApplicationQuit()
    {
        m_UDPClientStateSyncDemo.Stop();
    }

    void FixedUpdate()
    {
        if(PlayerID==-1){return;}

        if (NewPacket)
        {
            UpdateOtherPlayersTarget();
        }

        UpdateLocalPlayer();

        UpdateOtherPlayersRender();
        CheckLocalPlayer();
    }
    public void CheckLocalPlayer()
    {
        if (m_LastPacket == null)
        {
            return;
        }
        PlayerState target=m_LastPacket.Players.Find(p=>p.PlayerId==PlayerID);
        if ( Vector3Int.Distance(new Vector3Int(LocalX,LocalY,LocalZ),new Vector3Int(target.m_X,target.m_Y,target.m_Z))>100&&(DateTime.Now - m_LastPacket.TimeStamp).TotalMilliseconds > 1000&&(DateTime.Now - m_LastInputTime).TotalMilliseconds > 1000)
        {
            LocalX = target.m_X;
            LocalZ = target.m_Z;
            LocalY = target.m_Y;
            LocalPlayer.position = new Vector3((float)LocalX/10000, (float)LocalY/10000, (float)LocalZ/10000);
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
                m_LastInputTime=DateTime.Now;
                int applyX=(int)(inputX * 100);
                int applyZ = (int)(inputY * 100);
                m_UDPClientStateSyncDemo.Send(new PlayerInputPacket { m_X = applyX, m_Z = applyZ });
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
                VARIABLE.Value.simTime+=Time.fixedDeltaTime;
                if (VARIABLE.Value.simTime > VARIABLE.Value.totalTime)
                {
                    VARIABLE.Value.simTime = 0;
                    VARIABLE.Value.totalTime = 0;
                }
                if (VARIABLE.Value.totalTime == 0)
                {
                    VARIABLE.Value.LocalTransform.position = VARIABLE.Value.ServerTransform.position;
                }
                else
                {
                    VARIABLE.Value.LocalTransform.position = Vector3.Lerp(VARIABLE.Value.LocalTransform.position,
                        VARIABLE.Value.ServerTransform.position, VARIABLE.Value.simTime / VARIABLE.Value.totalTime);
                }
            }
        }
    }
    public void UpdateOtherPlayersTarget()
    {
        while (SyncDataQueue.Count > 0)
        {
            var data = SyncDataQueue.Dequeue();
            m_LastPacket = data;
            foreach (var player in data.Players)
            {
                
                if (player.PlayerId == PlayerID)
                {
                    UpdateLocalPlayerTargete(player);
                    continue;
                }
                PlayerInfo info=null;
                if(!Players.TryGetValue(player.PlayerId, out info))
                {
                    info = new PlayerInfo();
                    GameObject local = Instantiate(LocalPlayerPrefab);
                    GameObject server = Instantiate(ServerPlayerPrefab);
                    if (!ShowServer)
                    {
                        server.GetComponent<MeshRenderer>().enabled = false;
                    }
                    info.LocalTransform = local.transform;
                    info.ServerTransform = server.transform;
                    info.totalTime = 0;
                    info.simTime = 0;
                    Players[player.PlayerId] = info;
                }
                info.ServerTransform.position = new Vector3(player.x, player.y, player.z);
                info.totalTime += 0.2f;
                
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
    public void UpdateLocalPlayerTargete(PlayerState state)
    {
        if (LocalPlayer == null)
        {
            GameObject local = Instantiate(LocalPlayerPrefab);
            GameObject server = Instantiate(ServerPlayerPrefab);
            if (!ShowServer)
            {
                server.GetComponent<MeshRenderer>().enabled = false;
            }
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
