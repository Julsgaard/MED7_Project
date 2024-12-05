using Unity.Netcode;
using UnityEngine;

public class SharedNetworking : NetworkBehaviour
{
    // [Rpc(SendTo.Server)]
    // public void ClientToServerRpc()
    // {
    //     if (IsServer)
    //     {
    //         Debug.Log("Server received RPC.");
    //         ServerToClientRpc();
    //     }
    // }
    //
    // [Rpc(SendTo.NotServer)]
    // public void ServerToClientRpc()
    // {
    //     if (IsClient)
    //     {
    //         Debug.Log("Client received RPC.");
    //     }
    // }
}