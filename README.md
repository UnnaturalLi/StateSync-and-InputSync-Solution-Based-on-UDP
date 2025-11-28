# 🎮 StateSync-and-InputSync-Solution-Based-on-UDP
A robust multiplayer synchronization framework for Unity, built on top of a custom **Reliable UDP** transport layer. This project demonstrates two core synchronization paradigms—**State Synchronization** and **Input Synchronization**—implementing industry-standard techniques such as client-side prediction, server reconciliation, and entity interpolation to ensure smooth gameplay under latency.

> **Motivation:** To move beyond high-level "black box" networking engines (like Mirror/Photon) and implement core multiplayer architecture algorithms from scratch to understand the underlying mechanics of lag compensation and state replication.

## ✨ Core Features

### 1. State Synchronization (`StateSyncManager.cs`)
The server simulates the world and broadcasts authoritative state snapshots. The client predicts local movement and smoothly interpolates remote entities.

* **Server Authority:** The server dictates the true state of the world to prevent cheating.
* **Client-Side Prediction:** The local player moves immediately upon input (`UpdateLocalPlayer`) without waiting for the server's round-trip time (RTT), ensuring responsive controls.
* **Server Reconciliation:** To correct prediction errors, the client constantly checks its local state against the server's authoritative state. If the deviation exceeds a threshold, the client creates a "snap" correction (Rubber Banding) to resync.
    * *Implementation:* See `CheckLocalPlayer()` logic.
* **Entity Interpolation:** Remote players are updated via a `SyncDataQueue`. The renderer smoothly interpolates positions between the previous snapshot and the current target using `Vector3.Lerp`, masking network jitter.
* **Visual Debugging:** Includes a "Ghost" visualization (`ServerPlayerPrefab`) that renders the authoritative server position alongside the predicted client position for real-time debugging of prediction errors.

### 2. Input Synchronization (`InputSyncManager.cs`)
Clients send raw inputs rather than positions, and the simulation runs deterministically on all clients (Lockstep-style approach).

* **Input Broadcasting:** Captures `Input.GetAxis` and wraps them into packets.
* **Fixed-Point Math:** Inputs are converted to integers (e.g., `(int)(inputX * 100)`) before transmission to avoid floating-point non-determinism across different CPU architectures.
* **Queue-Based Processing:** Incoming packets (`InputSyncPacket`) are buffered in queues to handle out-of-order delivery and ensure sequential execution of game logic.

## 🛠️ Technical Architecture

### Logic Flow (State Sync)
1.  **Input:** Client captures input in `FixedUpdate`.
2.  **Prediction:** Client updates `LocalTransform` immediately.
3.  **Transport:** Input sent to server via custom `UDPClient`.
4.  **Authoritative Simulation:** Server processes input and broadcasts `StateSyncPacket`.
5.  **Reconciliation:** Client compares local `LocalX/Y/Z` with received `PlayerState`. If `Distance > Threshold`, force override local state.

### Code Highlight: Server Reconciliation
This snippet from `StateSyncManager.cs` demonstrates how the client corrects its position if the prediction drifts too far from the server's truth:

```csharp
public void CheckLocalPlayer()
{
    if (m_LastPacket == null) return;

    // Find the authoritative state for this player
    PlayerState target = m_LastPacket.Players.Find(p => p.PlayerId == PlayerID);
    
    // Calculate deviation between Local Prediction and Server Authority
    // Threshold set to 100 (fixed-point units)
    if (Vector3Int.Distance(new Vector3Int(LocalX, LocalY, LocalZ), 
                            new Vector3Int(target.m_X, target.m_Y, target.m_Z)) > 100 
        && (DateTime.Now - m_LastPacket.TimeStamp).TotalMilliseconds > 1000)
    {
        // Snap to authoritative position
        LocalX = target.m_X;
        LocalZ = target.m_Z;
        LocalY = target.m_Y;
        
        // Update Render Transform
        LocalPlayer.position = new Vector3(LocalX / 10000f, LocalY / 10000f, LocalZ / 10000f);
    }
}
```
## 🚀 Getting Started

### Prerequisites

- **Base Networking Module:** This project depends on my [TCP/UDP Extensible Networking Framework](https://github.com/UnnaturalLi/TCPUDP-Extensible-ReliableUDP-Demo) for the `NetworkBase` and `UDPClient` implementation.

### Configuration

The project automatically generates an `ipConfig.txt` file in the build directory to facilitate connecting to different servers without rebuilding.

1. Build the project or run in Editor.
2. Locate `ipConfig.txt` in `AppDomain.CurrentDomain.BaseDirectory`.
3. Enter the Server IP address (default is `127.0.0.1`).

### Running the Demo

1. **Start Server:** Launch the dedicated server application (Console App).
2. **Start Clients:**
    - Open the `StateSyncScene` to test prediction and reconciliation.
    - Open the `InputSyncScene` to test input broadcasting.
3. **Controls:** Use `WASD` or Arrow Keys to move the player. Observe the `ServerPlayer` (Ghost) vs `LocalPlayer` to see prediction in action.

## 📜 License)
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
