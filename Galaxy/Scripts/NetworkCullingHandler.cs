using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Logic;

namespace Gj.Galaxy.Scripts
{
    [RequireComponent(typeof(NetworkEsse))]
    public class NetworkCullingHandler : MonoBehaviour, GameObservable
    {
        #region VARIABLES

        private int orderIndex;

        private CullMap cull;

        private List<string> previousActiveCells, activeCells;

        private NetworkEsse esse;

        private Vector3 lastPosition, currentPosition;

        #endregion

        #region UNITY_FUNCTIONS

        /// <summary>
        ///     Gets references to the PhotonView component and the cull area game object.
        /// </summary>
        private void OnEnable()
        {
            if (this.esse == null)
            {
                this.esse = GetComponent<NetworkEsse>();

                if (!this.esse.isMine)
                {
                    return;
                }
            }

            if (this.cull == null)
            {
                this.cull = FindObjectOfType<CullMap>();
            }

            this.previousActiveCells = new List<string>(0);
            this.activeCells = new List<string>(0);

            this.currentPosition = this.lastPosition = transform.position;
        }

        /// <summary>
        ///     Initializes the right interest group or prepares the permanent change of the interest group of the PhotonView component.
        /// </summary>
        private void Start()
        {
            if (!this.esse.isMine)
            {
                return;
            }

            if (this.esse)
            {
                if (this.cull.NumberOfSubdivisions == 0)
                {
                    this.esse.group = this.cull.FIRST_GROUP_ID.ToString();

                    AreaConnect.SetGroups(new string[] { this.cull.FIRST_GROUP_ID.ToString() }, null);
                }
                else
                {
                    // This is used to continuously update the active group.
                    this.esse.ObservedComponents.Add(this);
                }
            }
        }

        /// <summary>
        ///     Checks if the player has moved previously and updates the interest groups if necessary.
        /// </summary>
        private void Update()
        {
            if (!this.esse.isMine)
            {
                return;
            }

            this.lastPosition = this.currentPosition;
            this.currentPosition = transform.position;

            // This is a simple position comparison of the current and the previous position. 
            // When using Network Culling in a bigger project keep in mind that there might
            // be more transform-related options, e.g. the rotation, or other options to check.
            if (this.currentPosition != this.lastPosition)
            {
                if (this.HaveActiveCellsChanged())
                {
                    this.UpdateInterestGroups();
                }
            }
        }

        /// <summary>
        ///     Drawing informations.
        /// </summary>
        private void OnGUI()
        {
            if (!this.esse.isMine)
            {
                return;
            }

            string subscribedAndActiveCells = "Inside cells:\n";
            string subscribedCells = "Subscribed cells:\n";

            for (int index = 0; index < this.activeCells.Count; ++index)
            {
                if (index <= this.cull.NumberOfSubdivisions)
                {
                    subscribedAndActiveCells += this.activeCells[index] + " | ";
                }

                subscribedCells += this.activeCells[index] + " | ";
            }
            GUI.Label(new Rect(20.0f, Screen.height - 120.0f, 200.0f, 40.0f), "<color=white>PhotonView Group: " + this.esse.group + "</color>", new GUIStyle() { alignment = TextAnchor.UpperLeft, fontSize = 16 });
            GUI.Label(new Rect(20.0f, Screen.height - 100.0f, 200.0f, 40.0f), "<color=white>" + subscribedAndActiveCells + "</color>", new GUIStyle() { alignment = TextAnchor.UpperLeft, fontSize = 16 });
            GUI.Label(new Rect(20.0f, Screen.height - 60.0f, 200.0f, 40.0f), "<color=white>" + subscribedCells + "</color>", new GUIStyle() { alignment = TextAnchor.UpperLeft, fontSize = 16 });
        }

        #endregion

        /// <summary>
        ///     Checks if the previously active cells have changed.
        /// </summary>
        /// <returns>True if the previously active cells have changed and false otherwise.</returns>
        private bool HaveActiveCellsChanged()
        {
            if (this.cull.NumberOfSubdivisions == 0)
            {
                return false;
            }

            this.previousActiveCells = new List<string>(this.activeCells);
            this.activeCells = new List<string>(this.activeCells); // this.cull.GetActiveCells(transform.position);

            // If the player leaves the area we insert the whole area itself as an active cell.
            // This can be removed if it is sure that the player is not able to leave the area.
            while (this.activeCells.Count <= this.cull.NumberOfSubdivisions)
            {
                this.activeCells.Add(this.cull.FIRST_GROUP_ID.ToString());
            }

            if (this.activeCells.Count != this.previousActiveCells.Count)
            {
                return true;
            }

            if (this.activeCells[this.cull.NumberOfSubdivisions] != this.previousActiveCells[this.cull.NumberOfSubdivisions])
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Unsubscribes from old and subscribes to new interest groups.
        /// </summary>
        private void UpdateInterestGroups()
        {
            List<string> disable = new List<string>(0);

            foreach (string groupId in this.previousActiveCells)
            {
                if (!this.activeCells.Contains(groupId))
                {
                    disable.Add(groupId);
                }
            }
            AreaConnect.SetGroups(disable.ToArray(), this.activeCells.ToArray());
        }

        #region GameObservable implementation

        public void OnSerialize(StreamBuffer stream, MessageInfo info)
        {
            // If the player leaves the area we insert the whole area itself as an active cell.
            // This can be removed if it is sure that the player is not able to leave the area.
            while (this.activeCells.Count <= this.cull.NumberOfSubdivisions)
            {
                this.activeCells.Add(this.cull.FIRST_GROUP_ID.ToString());
            }

            if (this.cull.NumberOfSubdivisions == 1)
            {
                this.orderIndex = (++this.orderIndex % this.cull.SUBDIVISION_FIRST_LEVEL_ORDER.Length);
                this.esse.group = this.activeCells[this.cull.SUBDIVISION_FIRST_LEVEL_ORDER[this.orderIndex]];
            }
            else if (this.cull.NumberOfSubdivisions == 2)
            {
                this.orderIndex = (++this.orderIndex % this.cull.SUBDIVISION_SECOND_LEVEL_ORDER.Length);
                this.esse.group = this.activeCells[this.cull.SUBDIVISION_SECOND_LEVEL_ORDER[this.orderIndex]];
            }
            else if (this.cull.NumberOfSubdivisions == 3)
            {
                this.orderIndex = (++this.orderIndex % this.cull.SUBDIVISION_THIRD_LEVEL_ORDER.Length);
                this.esse.group = this.activeCells[this.cull.SUBDIVISION_THIRD_LEVEL_ORDER[this.orderIndex]];
            }
        }

        public void OnDeserialize(StreamBuffer stream, MessageInfo info)
        {

        }

        public void Bind(NetworkEsse esse)
        {
            throw new System.NotImplementedException();
        }


        #endregion
    }
}
