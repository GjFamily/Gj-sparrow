using UnityEngine;
using System.Collections;

namespace Gj
{
    public class AiControl : BaseControl
    {
		public override void Init()
		{
            Info.attr.auto = true;
            base.Init();
		}
	}
}
