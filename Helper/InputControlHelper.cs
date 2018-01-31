using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gj
{
    public class InputControlHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private float radius;
        public Image item;
        public float size;
        public bool left;
        // Use this for initialization
        void Start()
        {
            radius = size / 2;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            item.rectTransform.localPosition = GetPosition(eventData.position.x, eventData.position.y);
            ChangeValue(position.x / radius, position.y / radius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            item.rectTransform.localPosition = new Vector2(0, 0);
            ChangeValue(0, 0);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            item.rectTransform.localPosition = GetPosition(eventData.position.x, eventData.position.y);
            ChangeValue(position.x / radius, position.y / radius);
        }

        private void ChangeValue(float x, float y) {
            if (left) {
                SystemInput.px = x;
                SystemInput.py = y;
            } else {
                SystemInput.rx = x;
                SystemInput.ry = y;
            }
        }

        private float GetPositionX(float x)
        {
            return x - radius;
        }

        private float GetPositionY(float y)
        {
            return y - radius;
        }

        private bool IsOut(float x, float y)
        {
            //计算两坐标点之间的距离
            return Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) > radius;
        }

        private Vector2 GetPosition(float x, float y)
        {
            float targetX = GetPositionX(x);
            float targetY = GetPositionY(y);
            if (IsOut(targetX, targetY))
            {
                //获取x，y坐标点弧度
                float radians = Mathf.Atan2(targetX, targetY);
                //sin和cos对应的是弧度不是角度
                return new Vector2(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
            }
            else
            {
                return new Vector2(targetX, targetY);
            }
        }
    }

}