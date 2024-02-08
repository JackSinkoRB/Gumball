using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class PowerpoleLinePosition : MonoBehaviour
    {

        [SerializeField] private GameObject linePrefab;
        [SerializeField] private Material material;
        [SerializeField] private float width = 1;

        [SerializeField, ReadOnly] private Chunk chunk;
        [SerializeField, ReadOnly] private List<PowerpoleLine> currentLines = new();

        private void OnEnable()
        {
            chunk = transform.FindComponentInParents<Chunk>();
            
            //whenever the chunk is unloaded, pool the lines
            chunk.onChunkUnload += PoolLines;
        }

        private void OnDisable()
        {
            chunk.onChunkUnload -= PoolLines;
        }

        public void CreateLine(PowerpoleLinePosition otherLinePosition)
        {
            PowerpoleLine line = linePrefab.GetSpareOrCreate<PowerpoleLine>(position: transform.position);
            line.Initialise(this, otherLinePosition, material, width);
            
            TrackLine(line);
            otherLinePosition.TrackLine(line);
        }

        private void TrackLine(PowerpoleLine line)
        {
            currentLines.Add(line);
        }
        
        private void UntrackLine(PowerpoleLine line)
        {
            currentLines.Remove(line);
        }
        
        private void PoolLines()
        {
            for (int i = currentLines.Count - 1; i >= 0; i--)
            {
                PowerpoleLine line = currentLines[i];
                
                line.PositionA.UntrackLine(line);
                line.PositionB.UntrackLine(line);
                line.gameObject.Pool();
            }
        }
        
    }
}
