using UnityEngine;
using System.Collections;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Scripts;
using SimpleJSON;

namespace Gj
{
    public class AiControl : BaseControl
    {
        public Ai ai;

        public override void Init()
        {
            Info.attr.auto = true;
            base.Init();
        }

        public override void FormatAi(JSONObject json)
        {
            ai = new Ai(json, Info.attr);
        }

        public override void InitSync(NetworkEsse esse)
        {
            esse.serializeStatus = Synchronization.Fixed;
            esse.ownershipTransfer = OwnershipOption.Request;
        }
    }
}
