using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gj
{
    public class ScreenRockerInputHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private float radius;
        public float size;
        private Vector2 startPosition;
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
            startPosition = new Vector2(SimpleTools.GetX(eventData.position.x), SimpleTools.GetY(eventData.position.y));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            startPosition = Vector2.zero;
            ChangeValue(0, 0);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            ChangeValue(position.x / radius, position.y / radius);
        }

        private void ChangeValue(float x, float y)
        {
            SystemInput.sh = x;
            SystemInput.sv = y;
        }

        private float GetPositionX(float x)
        {
            return x - startPosition.x;
        }

        private float GetPositionY(float y)
        {
            return y - startPosition.y;
        }

        private bool IsOut(float x, float y)
        {
            //计算两坐标点之间的距离
            return Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) > radius;
        }

        private Vector2 GetPosition(float x, float y)
        {
            float targetX = GetPositionX(SimpleTools.GetX(x));
            float targetY = GetPositionY(SimpleTools.GetY(y));
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