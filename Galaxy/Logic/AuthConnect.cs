using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System;

namespace Gj.Galaxy.Logic{
    internal class AuthEvent
    {
        public const byte Auth = 1;
        public const byte Version = 2;
    }
    public interface AuthDelegate
    {

    }
    // Auth操作
    public class AuthConnect : NamespaceListener
    {
        private static Namespace n;
        public static AuthDelegate Delegate;
        private static AuthConnect listener;

        static AuthConnect()
        {
            n = PeerClient.Of(NamespaceId.Auth);
            listener = new AuthConnect();
            n.listener = listener;
        }

        public static void Auth(string userName, string password, Action<object> callback){
            n.Emit(AuthEvent.Auth, new object[] { userName, password },(object[] obj) => callback(obj[0]));
        }

        //private bool CallAuthenticate()
        //{
        //    // once encryption is availble, the client should send one (secure) authenticate. it includes the AppId (which identifies your app on the Photon Cloud)
        //    AuthenticationValues auth = this.AuthValues ?? new AuthenticationValues() { UserId = this.PlayerName };
        //    if (this.AuthMode == AuthModeOption.Auth)
        //    {
        //        return this.OpAuthenticate(this.AppId, this.AppVersion, auth, this.CloudRegion.ToString(), this.requestLobbyStatistics);
        //    }
        //    else
        //    {
        //        return this.OpAuthenticateOnce(this.AppId, this.AppVersion, auth, this.CloudRegion.ToString(), this.EncryptionMode, PhotonNetwork.PhotonServerSettings.Protocol);
        //    }
        //}
        // use the "secret" or "token" whenever we get it. doesn't really matter if it's in AuthResponse.
        //    if (operationResponse.Parameters.ContainsKey(ParameterCode.Secret))
        //    {
        //        if (this.AuthValues == null)
        //        {
        //    this.AuthValues = new AuthenticationValues();
        //    // this.DebugReturn(DebugLevel.ERROR, "Server returned secret. Created AuthValues.");
        //}

            //    this.AuthValues.Token = operationResponse [ParameterCode.Secret] as string;
            //    this.tokenCache = this.AuthValues.Token;
            //}

//        case OperationCode.Authenticate:
//                case OperationCode.AuthenticateOnce:
//                    {
//                        // ClientState oldState = this.State;

//                        if (operationResponse.ReturnCode != 0)
//                        {
//                            if (operationResponse.ReturnCode == ErrorCode.InvalidOperation)
//                            {
//                                Debug.LogError(string.Format("If you host Photon yourself, make sure to start the 'Instance LoadBalancing' " + this.ServerAddress));
//                            }
//                            else if (operationResponse.ReturnCode == ErrorCode.InvalidAuthentication)
//                            {
//                                Debug.LogError(string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account."));
//                                SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, DisconnectCause.InvalidAuthentication);
//}
//                            else if (operationResponse.ReturnCode == ErrorCode.CustomAuthenticationFailed)
//                            {
//                                Debug.LogError(string.Format("Custom Authentication failed (either due to user-input or configuration or AuthParameter string format). Calling: OnCustomAuthenticationFailed()"));
//                                SendMonoMessage(PhotonNetworkingMessage.OnCustomAuthenticationFailed, operationResponse.DebugMessage);
//                            }
//                            else
//                            {
//                                Debug.LogError(string.Format("Authentication failed: '{0}' Code: {1}", operationResponse.DebugMessage, operationResponse.ReturnCode));
//                            }

//                            this.State = ClientState.Disconnecting;
//                            this.Disconnect();

//                            if (operationResponse.ReturnCode == ErrorCode.AuthenticationTicketExpired)
//                            {
//    if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
//        Debug.LogError(string.Format("The authentication ticket expired. You need to connect (and authenticate) again. Disconnecting."));
//    SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, DisconnectCause.AuthenticationTicketExpired);
//}
//                            break;
//}
//                        else
//                        {
//                            // successful connect/auth. depending on the used server, do next steps:

//                            if (this.Server == ServerConnection.NameServer || this.Server == ServerConnection.MasterServer)
//                            {
//    if (operationResponse.Parameters.ContainsKey(ParameterCode.UserId))
//    {
//        string incomingId = (string)operationResponse.Parameters[ParameterCode.UserId];
//        if (!string.IsNullOrEmpty(incomingId))
//        {
//            if (this.AuthValues == null)
//            {
//                this.AuthValues = new AuthenticationValues();
//            }
//            this.AuthValues.UserId = incomingId;
//            PhotonNetwork.player.UserId = incomingId;

//            if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
//            {
//                this.DebugReturn(DebugLevel.INFO, string.Format("Received your UserID from server. Updating local value to: {0}", incomingId));
//            }
//        }
//    }
//    if (operationResponse.Parameters.ContainsKey(ParameterCode.NickName))
//    {
//        this.PlayerName = (string)operationResponse.Parameters[ParameterCode.NickName];
//        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
//        {
//            this.DebugReturn(DebugLevel.INFO, string.Format("Received your NickName from server. Updating local value to: {0}", this.playername));
//        }
//    }

//    if (operationResponse.Parameters.ContainsKey(ParameterCode.EncryptionData))
//    {
//        this.SetupEncryption((Dictionary<byte, object>)operationResponse.Parameters[ParameterCode.EncryptionData]);
//    }
//}

//                            if (this.Server == ServerConnection.NameServer)
//                            {
//    // on the NameServer, authenticate returns the MasterServer address for a region and we hop off to there
//    this.MasterServerAddress = operationResponse[ParameterCode.Address] as string;
//    if (PhotonNetwork.UseAlternativeUdpPorts && this.TransportProtocol == ConnectionProtocol.Udp)
//    {
//        this.MasterServerAddress = this.MasterServerAddress.Replace("5058", "27000").Replace("5055", "27001").Replace("5056", "27002");
//    }
//    this.DisconnectToReconnect();
//}
//                            else if (this.Server == ServerConnection.MasterServer)
//                            {
//    if (this.AuthMode != AuthModeOption.Auth)
//    {
//        this.OpSettings(this.requestLobbyStatistics);
//    }
//    if (PhotonNetwork.autoJoinLobby)
//    {
//        this.State = ClientState.Authenticated;
//        this.OpJoinLobby(this.lobby);
//    }
//    else
//    {
//        this.State = ClientState.ConnectedToMaster;
//        SendMonoMessage(PhotonNetworkingMessage.OnConnectedToMaster);
//    }
//}
//                            else if (this.Server == ServerConnection.GameServer)
//                            {
//    this.State = ClientState.Joining;
//    this.enterRoomParamsCache.PlayerProperties = GetLocalActorProperties();
//    this.enterRoomParamsCache.OnGameServer = true;

//    if (this.lastJoinType == JoinType.JoinRoom || this.lastJoinType == JoinType.JoinRandomRoom || this.lastJoinType == JoinType.JoinOrCreateRoom)
//    {
//        // if we just "join" the game, do so. if we wanted to "create the room on demand", we have to send this to the game server as well.
//        this.OpJoinRoom(this.enterRoomParamsCache);
//    }
//    else if (this.lastJoinType == JoinType.CreateRoom)
//    {
//        this.OpCreateGame(this.enterRoomParamsCache);
//    }
//}

//                            if (operationResponse.Parameters.ContainsKey(ParameterCode.Data))
//                            {
//    // optionally, OpAuth may return some data for the client to use. if it's available, call OnCustomAuthenticationResponse
//    Dictionary<string, object> data = (Dictionary<string, object>)operationResponse.Parameters[ParameterCode.Data];
//    if (data != null)
//    {
//        SendMonoMessage(PhotonNetworkingMessage.OnCustomAuthenticationResponse, data);
//    }
//}
//}
                    //    break;
                    //}

        //        case EventCode.AuthEvent:
        //            if (this.AuthValues == null)
        //            {
        //    this.AuthValues = new AuthenticationValues();
        //}

                    //this.AuthValues.Token = photonEvent [ParameterCode.Secret] as string;
                    //this.tokenCache = this.AuthValues.Token;
                    //break;


        public void OnDisconnect()
        {
            throw new System.NotImplementedException();
        }

        public void OnError()
        {
            throw new System.NotImplementedException();
        }

        public void OnEvent()
        {
            throw new System.NotImplementedException();
        }

        public void OnConnect(bool success)
        {
            throw new System.NotImplementedException();
        }

        public void OnReconnect(bool success)
        {
            throw new System.NotImplementedException();
        }

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            throw new System.NotImplementedException();
        }
    }
}

