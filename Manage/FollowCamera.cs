using UnityEngine;
using System.Collections;

namespace Gj
{
    [AddPart(typeof(Follow))]
    public class FollowCamera : BaseManage
    {
        public Main system;
        // Use this for initialization
        void Start()
        {
            GetComponent<Follow>().FollowTarget(system.play, 5);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
