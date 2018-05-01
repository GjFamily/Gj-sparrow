using UnityEngine;
using System.Collections;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Scripts;

namespace Gj
{
    public class AiControl : BaseControl
    {
		public override void Init()
		{
            Info.attr.auto = true;
            base.Init();
		}

        protected override void Command(byte type, byte category, float value)
        {
            OnCommand(type, category, value);
        }

        public override void InitSync(NetworkEsse esse)
        {
            esse.synchronization = Synchronization.Reliable;
            esse.ownershipTransfer = OwnershipOption.Request;
        }
	}
}
