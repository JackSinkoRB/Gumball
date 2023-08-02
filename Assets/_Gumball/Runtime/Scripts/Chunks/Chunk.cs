using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(ChunkEditorTools))]
    public class Chunk : MonoBehaviour
    {

        [SerializeField] private SplineComputer splineComputer;

        private Node firstConnector;
        private Node lastConnector;

        public int LastPointIndex => splineComputer.pointCount - 1;
        public SplineComputer SplineComputer => splineComputer;

        /// <summary>
        /// Get or create a connection node at the last point of the spline.
        /// </summary>
        public Node Connector
        {
            get
            {
                if (firstConnector == null)
                    firstConnector = CreateConnector();
                return firstConnector;
            }
        }

        /// <summary>
        /// Puts the chunk at the end of an existing chunk.
        /// </summary>
        public void Connect(Chunk chunkToAppendTo)
        {
            ChunkUtils.ConnectChunks(chunkToAppendTo, this);
        }
        
        public Vector3 GetCenterOfSpline()
        {
            float splineLength = splineComputer.CalculateLength();
            double travel = splineComputer.Travel(0.0, splineLength / 2f, Spline.Direction.Forward);
            Vector3 middle = splineComputer.EvaluatePosition(travel);
            return middle;
        }

        public SplineSample GetClosestPointOnSpline(Vector3 fromPoint)
        {
            SplineSample sample = new SplineSample();
            splineComputer.Project(fromPoint, ref sample);
            return sample;
        }

        private Node CreateConnector()
        {
            GameObject connector = new GameObject("Chunk Connector");
            connector.transform.SetParent(transform);
            Node node = connector.AddComponent<Node>();
            node.type = Node.Type.Free;
            return node;
        }
        
    }
}
