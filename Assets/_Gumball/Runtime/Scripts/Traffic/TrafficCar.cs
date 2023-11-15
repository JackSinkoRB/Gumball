using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class TrafficCar : MonoBehaviour
    {

        private const float timeBetweenChunkChecks = 1;

        [SerializeField, ReadOnly] private Chunk currentChunk;

        private float timeSinceLastChunkCheck;
        private readonly RaycastHit[] raycastHits = new RaycastHit[1];

        private void Update()
        {
            TryChunkCheck();
        }

        private void TryChunkCheck()
        {
            timeSinceLastChunkCheck += Time.deltaTime;

            if (timeSinceLastChunkCheck > timeBetweenChunkChecks)
            {
                timeSinceLastChunkCheck = 0;
                DoChunkCheck();
            }
        }

        private void DoChunkCheck()
        {
            //raycast down to get the chunk
            int hits = Physics.RaycastNonAlloc(transform.position,
                Vector3.down, raycastHits, Mathf.Infinity,
                GameObjectLayers.GetLayerMaskFromLayer(GameObjectLayers.Layer.Terrain));
            if (hits == 0)
            {
                gameObject.Pool();
            }
            else
            {
                currentChunk = raycastHits[0].transform.FindComponentInParents<Chunk>();
            }
        }
        
    }
}
