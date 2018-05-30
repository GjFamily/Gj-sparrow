using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    [RequireComponent(typeof(DefensePart))]
    public class AiPart : BasePart
    {
        public ObjectAttr attr;
        public AiBrain aiBrain;
        public AiStatus aiStatus;
        public Action<byte, byte, float> command;
        public void Init(ObjectAttr objectAttr, AiBrain ai, Action<byte, byte, float> action)
        {
            aiStatus = new AiStatus(objectAttr);
            attr = objectAttr;
            aiBrain = ai;
            command = action;
        }

        public void Decision()
        {
            if (!aiStatus.IsFree()) return;
            if (command == null) return;
            AiBehavior aiBehavior = aiBrain.CheckBehavior(aiStatus);
            AiCommand aiCommand = aiBrain.FormatCommand(aiBehavior);
            command(aiCommand.type, aiCommand.category, aiCommand.value);
            aiStatus.lastBehavior = aiBehavior;
            aiStatus.lastBehaviorTime = Time.time;
        }

        public GameObject target;
        public GameObject nearestTarget;
        public Dictionary<string, float> skillTime;
        public List<GameObject> teammate;
        public List<GameObject> enemy;
        public List<GameObject> partner;
        public List<GameObject> safe;
        public List<GameObject> attack;
        public List<GameObject> beAttack;
        public float lastHitTime;

        public void CheckStatus()
        {
            if (aiBrain.GetModel().radius > 0) {
                Scanning(aiBrain.GetModel().radius);
            }
        }

        private void Scanning(float radius) {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            float distance = 0;
            float _distance = 0;
            enemy = new List<GameObject>();
            partner = new List<GameObject>();
            foreach (Collider collider in colliders)
            {
                GameObject obj = collider.gameObject;
                distance = Vector3.Distance(transform.position, obj.transform.position);
                if (_distance > 0 && distance < _distance) {
                    nearestTarget = obj;
                }
                _distance = distance;
                if (Info.IsEnemy(obj)) enemy.Add(obj);
                if (Info.IsPartner(obj)) partner.Add(obj);
            }
        }

        public void OnCure(GameObject target, Skill skill, bool extra)
        {
        }

        public void OnInjured(GameObject target, Skill skill, bool extra)
        {
            aiStatus.lastHitTime = Time.time;
        }
    }
}
