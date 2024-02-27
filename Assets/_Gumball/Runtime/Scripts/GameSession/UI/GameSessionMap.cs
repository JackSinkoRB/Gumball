using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class GameSessionMap : MonoBehaviour
    {

        [SerializeField] private Transform cameraMovementPlane;

        private GameSessionNode[] nodesCached;

        public Transform CameraMovementPlane => cameraMovementPlane;
        
        public GameSessionNode[] Nodes
        {
            get
            {
                if (nodesCached == null || nodesCached.Length == 0)
                    nodesCached = transform.GetComponentsInAllChildren<GameSessionNode>().ToArray();
                
                return nodesCached;
            }
        }

    }
}
