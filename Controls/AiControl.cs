using UnityEngine;
using System.Collections;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Scripts;
using SimpleJSON;

namespace Gj
{
    public class AiControl : BaseControl
    {
        public override void Init()
        {
            Info.attr.auto = true;
            base.Init();
        }

        public override void FormatExtend(JSONObject json)
        {

        }

        public override void InitSync(NetworkEsse esse)
        {
            esse.synchronization = Synchronization.Reliable;
            esse.ownershipTransfer = OwnershipOption.Request;
        }

        public void ChangeMode()
        {

        }

        private void CheckStatus()
        {

        }
    }
}
