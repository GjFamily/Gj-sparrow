using System;

namespace Gj.Galaxy.Logic{
    public enum NamespaceId
    {
        Auth = 0,
        Scene = 1,
        Chat = 2
    }

    public enum SceneRoom
    {
        Game = 1,
        Team = 2
    }

    public enum SyncTargets
    {
        /// <summary>Sends the RPC to everyone else and executes it immediately on this client. Player who join later will not execute this RPC.</summary>
        All,
        /// <summary>Sends the RPC to everyone else. This client does not execute the RPC. Player who join later will not execute this RPC.</summary>
        Others,
        /// <summary>Sends the RPC to everyone else and executes it immediately on this client. New players get the RPC when they join as it's buffered (until this client leaves).</summary>
        AllBuffered,
        /// <summary>Sends the RPC to everyone. This client does not execute the RPC. New players get the RPC when they join as it's buffered (until this client leaves).</summary>
        OthersBuffered,
        /// <summary>Sends the RPC to everyone (including this client) through the server.</summary>
        /// <remarks>
        /// This client executes the RPC like any other when it received it from the server.
        /// Benefit: The server's order of sending the RPCs is the same on all clients.
        /// </remarks>
        AllViaServer,
        /// <summary>Sends the RPC to everyone (including this client) through the server and buffers it for players joining later.</summary>
        /// <remarks>
        /// This client executes the RPC like any other when it received it from the server.
        /// Benefit: The server's order of sending the RPCs is the same on all clients.
        /// </remarks>
        AllBufferedViaServer,
        MasterClient
    }

    public static class EncryptionDataParameters
    {
        /// <summary>
        /// Key for encryption mode
        /// </summary>
        public const byte Mode = 0;
        /// <summary>
        /// Key for first secret
        /// </summary>
        public const byte Secret1 = 1;
        /// <summary>
        /// Key for second secret
        /// </summary>
        public const byte Secret2 = 2;
    }

    public enum EntitySynchronization:byte { Off, Reliable, Unreliable, UnreliableOnChange }

    /// <summary>
    /// Options to define how Ownership Transfer is handled per PhotonView.
    /// </summary>
    /// <remarks>
    /// This setting affects how RequestOwnership and TransferOwnership work at runtime.
    /// </remarks>
    public enum OwnershipOption
    {
        /// <summary>
        /// Ownership is fixed. Instantiated objects stick with their creator, scene objects always belong to the Master Client.
        /// </summary>
        Fixed,
        /// <summary>
        /// Ownership can be taken away from the current owner who can't object.
        /// </summary>
        Takeover,
        /// <summary>
        /// Ownership can be requested with PhotonView.RequestOwnership but the current owner has to agree to give up ownership.
        /// </summary>
        /// <remarks>The current owner has to implement IPunCallbacks.OnOwnershipRequest to react to the ownership request.</remarks>
        Request
    }
}
