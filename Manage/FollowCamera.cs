using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(Follow))]
    public class FollowCamera : BaseManage
    {
        public BaseSystem system;
        // Use this for initialization
        void Start()
        {
            GetComponent<Follow>().FollowTarget(system.player.gameObject, 5);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
