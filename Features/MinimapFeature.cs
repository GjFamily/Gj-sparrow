using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MinimapFeature : BaseFeature
    {
        private bool followDirection;
        private MinimapScript _minimapScript;
        private MinimapScript MinimapScript
        {
            get
            {
                if (_minimapScript == null)
                {
                    _minimapScript = GetFeatureComponent<MinimapScript>();
                }
                return _minimapScript;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (followDirection)
            {
                Model.transform.rotation = Quaternion.Euler(Model.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Model.transform.rotation.eulerAngles.z);
            }
        }

        public void SetPlayer()
        {
            followDirection = true;
            MinimapScript.player.SetActive(true);
            MinimapScript.enemy.SetActive(false);
            MinimapScript.partner.SetActive(false);
        }

        public void SetPartner()
        {
            followDirection = false;
            MinimapScript.player.SetActive(false);
            MinimapScript.enemy.SetActive(false);
            MinimapScript.partner.SetActive(true);
        }

        public void SetEnemy()
        {
            followDirection = false;
            MinimapScript.player.SetActive(false);
            MinimapScript.enemy.SetActive(true);
            MinimapScript.partner.SetActive(false);
        }
    }
}
