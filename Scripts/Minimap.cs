using UnityEngine;
using System.Collections;

namespace Gj
{
    public class Minimap : MonoBehaviour
    {
        public GameObject player;
        public GameObject partner;
        public GameObject enemy;

        public void SetPlayer()
        {
            player.SetActive(true);
            enemy.SetActive(false);
            partner.SetActive(false);
        }

        public void SetPartner()
        {
            player.SetActive(false);
            enemy.SetActive(false);
            partner.SetActive(true);
        }

        public void SetEnemy()
        {
            player.SetActive(false);
            enemy.SetActive(true);
            partner.SetActive(false);
        }
    }
}
