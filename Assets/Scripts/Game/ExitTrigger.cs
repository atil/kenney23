using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ExitTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
            {
                this.enabled = false;
                GameObject.Find("GameMain").GetComponent<GameMain>().OnExitTriggered();
            }
        }
    }
}
