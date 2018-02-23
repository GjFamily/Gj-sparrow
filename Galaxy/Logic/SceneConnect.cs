using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;
using System;

namespace Gj.Galaxy.Logic{
    internal class SceneEvent
    {
        public const byte LobbyJoin = 1;
        public const byte LobbyLeave = 2;
        public const byte LobbyExist = 3;
        public const byte TeamCreate = 4;
    }
    public interface SceneDelegate{
        void OnReady();
        void OnJoinedGame();
        void OnInvitedTeam();
    }
    public class LobbyType{
        public const string PVE = "pve";
        public const string PVP = "pvp";
    }
    // player 信息
    // lobby 信息
    // team和game接入信息
    public class SceneConnect : NamespaceListener
    {
        private static Namespace n;
        public static SceneDelegate Delegate;
        private static SceneConnect lisenter;

        public static NetworkPlayer player;

        static SceneConnect(){
            n = PeerClient.Of(NamespaceId.Scene);
            lisenter = new SceneConnect();
            n.listener = lisenter;
            player = new NetworkPlayer(true, -1, "");
        }

        public static Namespace Of(SceneRoom ns){
            return n.Of((byte)ns);
        }

        public static void Connect(Action a){
            n.Connect("");
        }

        public static void JoinLobby(string lobby, Dictionary<string, object> options)
        {
            switch (lobby)
            {
                default:
                    throw new Exception("lobby type is empty");
                case LobbyType.PVE:
                case LobbyType.PVP:
                    break;
            }
            n.Emit(SceneEvent.LobbyJoin, new object[] { lobby, options });
        }

        public static void CreateTeam(int people){
            
        }

        public static void LeaveLobby()
        {
            n.Emit(SceneEvent.LobbyLeave, new object[]{});
        }


        public static void SetPlayerCustomProperties(Hashtable customProperties)
        {
            if (customProperties == null)
            {
                customProperties = new Hashtable();
                foreach (object k in player.CustomProperties.Keys)
                {
                    customProperties[(string)k] = null;
                }
            }
            player.SetCustomProperties(customProperties);
        }
        public static void RemovePlayerCustomProperties(string[] customPropertiesToDelete)
        {
            var props = player.CustomProperties;
            for (int i = 0; i < customPropertiesToDelete.Length; i++)
            {
                string key = customPropertiesToDelete[i];
                if (props.ContainsKey(key))
                {
                    props.Remove(key);
                }
            }
            player.CustomProperties = new Hashtable();
            player.SetCustomProperties(props);
        }

        public void OnConnect(bool success)
        {
            throw new NotImplementedException();
        }

        public void OnReconnect(bool success)
        {
            throw new NotImplementedException();
        }

        public void OnEvent()
        {
            //case OperationCode.CreateGame:
                //    {
                //        if (this.Server == ServerConnection.GameServer)
                //        {
                //            this.GameEnteredOnGameServer(operationResponse);
                //        }
                //        else
                //        {
                //            if (operationResponse.ReturnCode != 0)
                //            {
                //                if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
                //                    Debug.LogWarning(string.Format("CreateRoom failed, client stays on masterserver: {0}.", operationResponse.ToStringFull()));

                //                this.State = (this.insideLobby) ? ClientState.JoinedLobby : ClientState.ConnectedToMaster;
                //                SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
                //                break;
                //            }

                //            string gameID = (string)operationResponse[ParameterCode.RoomName];
                //            if (!string.IsNullOrEmpty(gameID))
                //            {
                //                // is only sent by the server's response, if it has not been
                //                // sent with the client's request before!
                //                this.enterRoomParamsCache.RoomName = gameID;
                //            }

                //            this.GameServerAddress = (string)operationResponse[ParameterCode.Address];
                //            if (PhotonNetwork.UseAlternativeUdpPorts && this.TransportProtocol == ConnectionProtocol.Udp)
                //            {
                //                this.GameServerAddress = this.GameServerAddress.Replace("5058", "27000").Replace("5055", "27001").Replace("5056", "27002");
                //            }
                //            this.DisconnectToReconnect();
                //        }

                //        break;
                //    }

                //case OperationCode.JoinGame:
                //    {
                //        if (this.Server != ServerConnection.GameServer)
                //        {
                //            if (operationResponse.ReturnCode != 0)
                //            {
                //                if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
                //                    Debug.Log(string.Format("JoinRoom failed (room maybe closed by now). Client stays on masterserver: {0}. State: {1}", operationResponse.ToStringFull(), this.State));

                //                SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
                //                break;
                //            }

                //            this.GameServerAddress = (string)operationResponse[ParameterCode.Address];
                //            if (PhotonNetwork.UseAlternativeUdpPorts && this.TransportProtocol == ConnectionProtocol.Udp)
                //            {
                //                this.GameServerAddress = this.GameServerAddress.Replace("5058", "27000").Replace("5055", "27001").Replace("5056", "27002");
                //            }
                //            this.DisconnectToReconnect();
                //        }
                //        else
                //        {
                //            this.GameEnteredOnGameServer(operationResponse);
                //        }

                //        break;
                //    }

                //case OperationCode.JoinRandomGame:
                //    {
                //        // happens only on master. on gameserver, this is a regular join (we don't need to find a random game again)
                //        // the operation OpJoinRandom either fails (with returncode 8) or returns game-to-join information
                //        if (operationResponse.ReturnCode != 0)
                //        {
                //            if (operationResponse.ReturnCode == ErrorCode.NoRandomMatchFound)
                //            {
                //                if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
                //                    Debug.Log("JoinRandom failed: No open game. Calling: OnPhotonRandomJoinFailed() and staying on master server.");
                //            }
                //            else if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
                //            {
                //                Debug.LogWarning(string.Format("JoinRandom failed: {0}.", operationResponse.ToStringFull()));
                //            }

                //            SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed, operationResponse.ReturnCode, operationResponse.DebugMessage);
                //            break;
                //        }

                //        string roomName = (string)operationResponse[ParameterCode.RoomName];
                //        this.enterRoomParamsCache.RoomName = roomName;
                //        this.GameServerAddress = (string)operationResponse[ParameterCode.Address];
                //        if (PhotonNetwork.UseAlternativeUdpPorts && this.TransportProtocol == ConnectionProtocol.Udp)
                //        {
                //            this.GameServerAddress = this.GameServerAddress.Replace("5058", "27000").Replace("5055", "27001").Replace("5056", "27002");
                //        }
                //        this.DisconnectToReconnect();
                //        break;
                //    }

                //case OperationCode.JoinLobby:
                //    this.State = ClientState.JoinedLobby;
                //    this.insideLobby = true;
                //    SendMonoMessage(PhotonNetworkingMessage.OnJoinedLobby);

                //    // this.mListener.joinLobbyReturn();
                //    break;
                //case OperationCode.LeaveLobby:
                //    this.State = ClientState.Authenticated;
                //    this.LeftLobbyCleanup();    // will set insideLobby = false
                //    break;

                //case OperationCode.Leave:
                //    this.DisconnectToReconnect();
                //    break;

                //case OperationCode.SetProperties:
                //    // this.mListener.setPropertiesReturn(returnCode, debugMsg);
                //    break;

                //case OperationCode.GetProperties:
                    //{
                    //    Hashtable actorProperties = (Hashtable)operationResponse[ParameterCode.PlayerProperties];
                    //    Hashtable gameProperties = (Hashtable)operationResponse[ParameterCode.GameProperties];
                    //    this.ReadoutProperties(gameProperties, actorProperties, 0);

                    //    // RemoveByteTypedPropertyKeys(actorProperties, false);
                    //    // RemoveByteTypedPropertyKeys(gameProperties, false);
                    //    // this.mListener.getPropertiesReturn(gameProperties, actorProperties, returnCode, debugMsg);
                    //    break;
                    //}

                
        }

        public void OnError(string message)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnect()
        {
            throw new NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            throw new NotImplementedException();
        }
    }
}

