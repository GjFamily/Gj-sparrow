using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BaseBehaviour : MonoBehaviour
    {
        private Info _info;
        protected Info Info
        {
            get
            {
                if (_info == null)
                {
                    _info = GetComponent<Info>();
                }
                return _info;
            }
        }

        private Rigidbody _rigidbody;
        protected Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }
                return _rigidbody;
            }
        }

        protected bool init = false;
        protected bool velocity = true;

        void FixedUpdate()
        {
            // 去掉物理速度防止反弹
            if (init && !velocity && Rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
            }
        }
    }
}
