using UnityEngine;
using System.Collections;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Scripts;
using SimpleJSON;

namespace Gj
{
    [RequireComponent(typeof(AiPart))]
    public class AiControl : BaseControl
    {
        public AiBrain aiBrain;
        private AiPart aiPart;
        public AiPart AiPart {
            get {
                if (aiPart == null) {
                    aiPart = GetComponent<AiPart>();
                }
                return aiPart;
            }
        }

        public override void Init()
        {
            base.Init();
            Info.attr.auto = true;
            FormatAi(Info.attr.ai);
        }

        public void FormatAi(JSONObject json)
        {
            aiBrain = new AiBrain(json);
        }

        public override void OnMaster()
        {
            AiPart.Init(Info.attr, aiBrain, Command);
        }

        public override void InitSync(NetworkEsse esse)
        {
            esse.serializeStatus = Synchronization.Fixed;
            esse.ownershipTransfer = OwnershipOption.Request;
        }
    }
}
