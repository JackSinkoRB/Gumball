using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using JBooth.VertexPainterPro;
using MyBox;
#if UNITY_EDITOR
using Gumball.Editor;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    [ExecuteAlways]
    public class ChunkEditorTools : MonoBehaviour
    {

#if UNITY_EDITOR
        private Chunk chunk => GetComponent<Chunk>();
        private float timeSinceUnityUpdated => Time.realtimeSinceStartup - timeWhenUnityLastUpdated;

        [ReadOnly, SerializeField] private Chunk chunkBefore;
        [ReadOnly, SerializeField] private Chunk chunkAfter;

        [SerializeField, HideInInspector] private bool hasChunkConnected;

        public bool HasChunkConnected => hasChunkConnected;
        
        private GameObject previousSelection;
        private float timeWhenUnityLastUpdated;

        private bool isRuntimeChunk => name.Contains(ChunkUtils.RuntimeChunkSuffix);
        
        private void OnSavePrefab(string prefabName, string path)
        {
            if (prefabName.Equals(gameObject.name) && !isRuntimeChunk)
            {
                ChunkUtils.BakeMeshes(chunk);
                chunk.FindSplineMeshes();
            }
        }
        
        private void OnEnable()
        {
            SaveEditorAssetsEvents.onSavePrefab += OnSavePrefab;
            
            chunk.SplineComputer.onRebuild += CheckToUpdateMeshesImmediately;
            chunk.UpdateSplineSampleData();
        }

        private void OnDisable()
        {
            chunk.SplineComputer.onRebuild -= CheckToUpdateMeshesImmediately;

            SaveEditorAssetsEvents.onSavePrefab -= OnSavePrefab;
            
            Tools.hidden = false;
        }

        private void OnDrawGizmos()
        {
            currentGrid?.OnDrawGizmos();
            CheckToShowVertices();
        }

        private void Update()
        {
            CheckIfJustDeselected();
            
            previousSelection = Selection.activeGameObject;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                timeWhenUnityLastUpdated = Time.realtimeSinceStartup;
        }
        
        private void LateUpdate()
        {
            CheckToDisableTools();
        }
        
        private void OnValidate()
        {
            CheckToUpdateMeshesImmediately();
            CheckIfTerrainIsRaycastable();
        }

        /// <summary>
        /// Iterates over the chunk objects and ensures the colliders to flatten to are on the raycastable layer.
        /// </summary>
        public void EnsureChunkObjectsAreOnRaycastLayer()
        {
            foreach (ChunkObject chunkObject in chunk.transform.GetComponentsInAllChildren<ChunkObject>())
            {
                if (chunkObject.CanFlattenTerrain)
                    chunkObject.ColliderToFlattenTo.gameObject.layer = (int) LayersAndTags.Layer.ChunkObject;
            }
        }
        
        private void CheckIfTerrainIsRaycastable()
        {
            if (chunk.TerrainHighLOD == null)
                return;

            chunk.TerrainHighLOD.layer = (int)LayersAndTags.Layer.Terrain;
            chunk.TerrainHighLOD.GetComponent<MeshCollider>(true);
        }
        
        #region Show terrain vertices
        private const float timeToShowVertices = 15;
        
        private float timeLastClickedShowTerrainVertices = -10000f;
        
        private float timeSinceClickedShowTerrainVertices => Time.realtimeSinceStartup - timeLastClickedShowTerrainVertices;
        
        [ButtonMethod]
        public void ShowTerrainVertices()
        {
            timeLastClickedShowTerrainVertices = Time.realtimeSinceStartup;
        }
        
        private void CheckToShowVertices()
        {
            if (timeSinceClickedShowTerrainVertices > timeToShowVertices)
                return;

            MeshFilter meshFilter = chunk.TerrainHighLOD.GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;

            if (meshFilter.sharedMesh == null)
                return;
            
            Vector3[] vertices = meshFilter.sharedMesh.vertices;
            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = vertices[vertexIndex];
                Vector3 vertexPositionWorld = meshFilter.transform.TransformPoint(vertexPosition);
                
                Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 20, Color.blue);
                Handles.Label(vertexPositionWorld, vertexIndex.ToString());
            }
        }
        #endregion
        
        #region Generate terrain
        
        [Tooltip("If enabled, the terrain will update whenever a value is changed. Otherwise the CreateTerrain button will need to be used.")]
        [SerializeField] private bool updateImmediately = true;
        
        [ReadOnly(nameof(hasChunkConnected)), SerializeField]
        private ChunkTerrainData terrainData = new();

        private ChunkGrid currentGrid;
        
        private static bool subscribedToPlayModeStateChanged;
        private static PlayModeStateChange playModeState;

        public ChunkTerrainData TerrainData => terrainData;
        
        [ButtonMethod]
        public void ShowTerrainGrid()
        {
            currentGrid = new ChunkGrid(chunk, terrainData.Resolution, terrainData.WidthAroundRoad);
        }
        
        public void ShowEndsOfSpline()
        {
            chunk.SplineComputer.RebuildImmediate();
            chunk.UpdateSplineSampleData();
            Debug.DrawRay(chunk.FirstSample.position, chunk.FirstSample.forward * 15, Color.blue, 15);
            Debug.DrawRay(chunk.FirstSample.position, chunk.FirstSample.up * 15, Color.green, 15);
            Debug.DrawRay(chunk.FirstSample.position, chunk.FirstSample.right * 15, Color.red, 15);
            
            Debug.DrawRay(chunk.LastSample.position, chunk.LastSample.forward * 15, Color.blue, 15);
            Debug.DrawRay(chunk.LastSample.position, chunk.LastSample.up * 15, Color.green, 15);
            Debug.DrawRay(chunk.LastSample.position, chunk.LastSample.right * 15, Color.red, 15);
        }
        
        [ButtonMethod]
        public void DrawMeshEdgeNormals()
        {
            foreach (int vertexIndex in chunk.ChunkMeshData.FirstEndVertices)
            {
                DrawNormal(vertexIndex, Color.red);
            }
            
            foreach (int vertexIndex in chunk.ChunkMeshData.LastEndVertices)
            {
                DrawNormal(vertexIndex, Color.blue);
            }
        }
        
        private void DrawNormal(int vertexIndex, Color color)
        {
            const float distance = 5;
            const float duration = 120;
            Debug.DrawRay(chunk.ChunkMeshData.GetCurrentVertexWorldPosition(vertexIndex), chunk.ChunkMeshData.Mesh.normals[vertexIndex] * distance, color, duration);
        }
        
        [ButtonMethod]
        public void DrawAllNormals()
        {
            for (int vertexIndex = 0; vertexIndex < chunk.ChunkMeshData.Vertices.Length; vertexIndex++)
            {
                DrawNormal(vertexIndex, Color.red);
            }
        }
        
        [ButtonMethod]
        public void CreateTerrain()
        {
            RecreateTerrainLODs();
            currentGrid = terrainData.Grid;

            Selection.SetActiveObjectWithContext(chunk.TerrainHighLOD, chunk);
        }
        
        [InitializeOnLoadMethod]
        private static void SubscribeToPlayModeStateChanged()
        {
            if (!subscribedToPlayModeStateChanged)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                subscribedToPlayModeStateChanged = true;
            }
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            playModeState = state;
        }

        private void CheckToUpdateMeshesImmediately()
        {
            if (!updateImmediately)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                timeWhenUnityLastUpdated = Time.realtimeSinceStartup;

            if (timeSinceUnityUpdated < 1) //likely recompiling
                return;

            if (chunk.IsAutomaticTerrainRecreationDisabled || hasChunkConnected)
                return;
            
            bool justSelected = previousSelection != gameObject && Selection.activeGameObject == gameObject;
            if (justSelected)
                return;

            bool justDeselected = previousSelection == gameObject && Selection.activeGameObject != gameObject;
            if (justDeselected)
                return;

            bool isSelected = Selection.activeGameObject == gameObject;
            if (!isSelected)
                return;
            
            if (playModeState is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode
                || (Application.isPlaying && !GameLoaderSceneManager.HasLoaded))
                return;

            if (chunk.TerrainHighLOD == null)
                return;
            
            EditorApplication.delayCall -= RecreateTerrain;
            EditorApplication.delayCall += RecreateTerrain;
        }
        
        /// <summary>
        /// Force the terrain to be recreated.
        /// </summary>
        public void RecreateTerrain()
        {
            DisconnectAll();
            
            GlobalLoggers.ChunkLogger.Log($"Recreating terrain for '{chunk.name}'");
            Material[] previousMaterials = chunk.TerrainHighLOD.GetComponent<MeshRenderer>().sharedMaterials;

            //check if there's additional vertex color data
            VertexInstanceStream vertexInstanceStream = chunk.TerrainHighLOD.GetComponent<VertexInstanceStream>();
            GenericDictionary<int, List<VertexInstanceStream.PaintData>> paintData = null;
            if (vertexInstanceStream != null)
                paintData = vertexInstanceStream.paintedVertices;

            DestroyImmediate(chunk.TerrainHighLOD);
            DestroyImmediate(chunk.TerrainLowLOD);

            chunk.SplineComputer.RebuildImmediate();

            RecreateTerrainLODs();

            if (paintData != null)
            {
                chunk.TerrainHighLOD.GetOrAddComponent<VertexInstanceStream>().SetPaintData(paintData);
                
                //TODO: generate vertex color data for the low LOD
            }
            
            if (chunkBefore != null)
                ChunkUtils.ConnectChunks(chunkBefore, chunk, ChunkUtils.LoadDirection.AFTER, new ChunkBlendData(chunkBefore, chunk));
            if (chunkAfter != null)
                ChunkUtils.ConnectChunks(chunk, chunkAfter, ChunkUtils.LoadDirection.AFTER, new ChunkBlendData(chunk, chunkAfter));

            UnbakeSplineMeshes();
        }

        private void RecreateTerrainLODs()
        {
            Dictionary<Chunk.TerrainLOD, GameObject> newTerrain = terrainData.Create(chunk);
            foreach (Chunk.TerrainLOD key in newTerrain.Keys)
            {
                GameObject terrain = newTerrain[key];
                chunk.SetTerrain(key, terrain);
            }
            
            chunk.SwitchTerrainLOD(Chunk.TerrainLOD.HIGH);
        }

        private void UnbakeSplineMeshes()
        {
            foreach (SplineMesh splineMesh in chunk.SplinesMeshes)
            {
                if (!splineMesh.gameObject.activeSelf)
                    continue;
                splineMesh.Unbake();
            }
        }
        
        #endregion
        
        #region Connect to a chunk
        [Header("Connect")]
        [SerializeField] private Chunk chunkToConnectWith;

        public void OnConnectChunkBefore(Chunk chunk)
        {
            OnConnectChunk();
            chunkBefore = chunk;
        }
        
        public void OnConnectChunkAfter(Chunk chunk)
        {
            OnConnectChunk();
            chunkAfter = chunk;
        }

        private void OnConnectChunk()
        {
            
        }

        private void OnDisconnectChunk()
        {
            if (!hasChunkConnected)
                transform.rotation = Quaternion.Euler(Vector3.zero); //reset rotation
        }
        
        
        public void DisconnectAll(bool canUndo = false)
        {
            if (canUndo)
            {
                List<Object> objectsToRecord = new List<Object>();

                objectsToRecord.Add(transform);
                objectsToRecord.Add(chunk.TerrainHighLOD.GetComponent<MeshFilter>());

                if (chunkAfter != null)
                {
                    objectsToRecord.Add(chunkAfter);
                    if (chunkAfter.GetComponent<ChunkEditorTools>().chunkBefore != null)
                        objectsToRecord.Add(chunkAfter.GetComponent<ChunkEditorTools>().chunkBefore);
                }

                if (chunkBefore != null)
                {
                    objectsToRecord.Add(chunkBefore);
                    if (chunkAfter.GetComponent<ChunkEditorTools>().chunkAfter != null)
                        objectsToRecord.Add(chunkAfter.GetComponent<ChunkEditorTools>().chunkAfter);
                }
                
                Undo.RecordObjects(objectsToRecord.ToArray(), "Disconnect Chunk");
            }

            OnDisconnectChunkAfter();
            OnDisconnectChunkBefore();
        }

        public void OnDisconnectChunkBefore()
        {
            if (chunkBefore == null)
                return;

            Chunk previousChunk = chunkBefore;
            chunkBefore = null;
            previousChunk.GetComponent<ChunkEditorTools>().OnDisconnectChunkAfter();
            OnDisconnectChunk();
        }

        public void OnDisconnectChunkAfter()
        {
            if (chunkAfter == null)
                return;
            
            Chunk previousChunk = chunkAfter;
            chunkAfter = null;
            previousChunk.GetComponent<ChunkEditorTools>().OnDisconnectChunkBefore();
            OnDisconnectChunk();
        }

        /// <summary>
        /// Connect the chunk with the specified chunk.
        /// </summary>
        [ButtonMethod]
        public void Connect()
        {
            if (chunkToConnectWith == null)
                throw new NullReferenceException($"There is no '{nameof(chunkToConnectWith)}' value set in the inspector.");
            
            if (hasChunkConnected)
                throw new InvalidOperationException("This chunk is already connected. Disconnect the chunk first.");

            ChunkUtils.ConnectChunksWithNewBlendData(chunkToConnectWith, chunk, ChunkUtils.LoadDirection.AFTER, true);
        }
        
        [ButtonMethod]
        public void Disconnect()
        {
            if (!hasChunkConnected)
            {
                Debug.Log("This chunk is already disconnected.");
                return;
            }

            Chunk[] affectedChunks = { chunk, chunkAfter, chunkBefore};
            DisconnectAll(true);
            foreach (Chunk affectedChunk in affectedChunks)
            {
                if (affectedChunk != null)
                    affectedChunk.GetComponent<ChunkEditorTools>().RecreateTerrain();
            }
        }

        private void CheckIfJustDeselected()
        {
            bool justDeselected = previousSelection == gameObject && Selection.activeGameObject != gameObject;
            if (justDeselected)
                Tools.hidden = false;
        }

        private void CheckToDisableTools()
        {
            if (Selection.activeGameObject != gameObject)
                return;
            
            Tools.hidden = hasChunkConnected;
        }
        
        #endregion

        [SerializeField] private bool showDebugLines;

        public bool ShowDebugLines => showDebugLines;
        
#endif
    }
}