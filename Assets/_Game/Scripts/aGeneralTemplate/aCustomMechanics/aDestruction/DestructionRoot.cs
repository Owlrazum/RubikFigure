using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomMechanics.Destruction
{
    public class DestructionRoot : MonoBehaviour
    {
        private List<DestructionShattered> shards;
        private bool isDestructed;

        private void Start()
        {
            var shardsArray = GetComponentsInChildren<DestructionShattered>();
            shards = new List<DestructionShattered>(shardsArray);
            isDestructed = false;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (isDestructed)
                {
                    gameObject.SetActive(false);
                    return;
                }
                foreach (DestructionShattered shard in shards)
                {
                    shard.StartBreaking();
                }
                isDestructed = true;
                //GameManager.Singleton.TargetObtained();
            }
        }
    }
}
