using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dreamteck.Splines;
using JBooth.VertexPainterPro;
using MyBox;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using System.IO;
using Gumball.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Gumball
{
    [ExecuteAlways]
    public class ChunkEditorTools : MonoBehaviour
    {

#if UNITY_EDITOR
        public static void OnDuplicateChunkAsset(string oldID, string newChunkPath, ChunkEditorTools newChunk)
        {
            //find the old chunk
            string directory = Path.GetDirectoryName(newChunkPath);
            UniqueIDAssigner idAssigner = UniqueIDAssigner.FindAssignerWithIDInDirectory(oldID, directory);

            if (idAssigner != null)
            {
                ChunkEditorTools oldChunk = idAssigner.GetComponent<ChunkEditorTools>();
                if (oldChunk != null)
                {
                    //rebuild the runtime chunk
                    ChunkUtils.CreateRuntimeChunk(oldChunk.gameObject, saveAssetsOnComplete: false);
                }
            }
            
            AssetDatabase.SaveAssets();
        }
        
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
            if (!prefabName.Equals(gameObject.name))
                return;
            
            CheckToAssignSplineMeshIDs();
        }
        
        private void OnEnable()
        {
            chunk.SplineComputer.onRebuild += CheckToUpdateMeshesImmediately;
            SaveEditorAssetsEvents.onSavePrefab += OnSavePrefab;

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
            
            EditorApplication.delayCall -= CheckToUnbakeMeshes;
            EditorApplication.delayCall += CheckToUnbakeMeshes;
        }

        private void CheckToUnbakeMeshes()
        {
            try
            {
                bool isPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() != null && PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot == gameObject;
                if (isPrefabMode)
                    UnbakeSplineMeshes();
            }
            catch (MissingReferenceException)
            {
                //safely ignore - prefab may have closed
            }
        }

        /// <summary>
        /// Iterates over the chunk objects and ensures the colliders to flatten to are on the raycastable layer.
        /// </summary>
        public void EnsureChunkObjectsAreOnRaycastLayer()
        {
            foreach (ChunkObject chunkObject in chunk.transform.GetComponentsInAllChildren<ChunkObject>())
            {
                if (chunkObject.CanFlattenTerrain && chunkObject.ColliderToFlattenTo != null)
                    chunkObject.ColliderToFlattenTo.gameObject.layer = (int) LayersAndTags.Layer.ChunkObject;
                
                if (chunkObject.CanColourTerrain && chunkObject.ColorModifier.ColliderToColourAround != null)
                    chunkObject.ColorModifier.ColliderToColourAround.gameObject.layer = (int) LayersAndTags.Layer.ChunkObject;
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

        #region COMBINE_MESHES
        [Header("Combine meshes")]
        [SerializeField] private MeshFilter meshFilter1;
        [SerializeField] private MeshFilter meshFilter2;

        [SerializeField, ReadOnly] private Vector3[] vertices1Before;
        [SerializeField, ReadOnly] private Vector3[] vertices2Before;

        [SerializeField, ReadOnly] private Vector3[] verticesCombined;
        
        [ButtonMethod]
        public void CombineMeshes()
        {
            vertices1Before = meshFilter1.sharedMesh.vertices;
            vertices2Before = meshFilter2.sharedMesh.vertices;
            
            var combine = new CombineInstance[2];
            combine[0].mesh = meshFilter1.sharedMesh;
            combine[0].transform = meshFilter1.transform.localToWorldMatrix;
            combine[1].mesh = meshFilter2.sharedMesh;
            combine[1].transform = meshFilter2.transform.localToWorldMatrix;

            MeshFilter combinedMeshFilter = new GameObject("TEMP").AddComponent<MeshFilter>();
            combinedMeshFilter.gameObject.AddComponent<MeshRenderer>();
            
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine);
            
            combinedMesh.SetUVs(0, ChunkUtils.GetTriplanarUVs(combinedMesh.vertices, combinedMeshFilter.transform));

            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();
            combinedMesh.RecalculateTangents();

            combinedMeshFilter.sharedMesh = combinedMesh;

            verticesCombined = combinedMesh.vertices;
            
            Debug.DrawRay(meshFilter1.transform.TransformPoint(vertices1Before[2]), Vector3.up * 100, Color.red, 60);
            Debug.DrawRay(meshFilter2.transform.TransformPoint(vertices2Before[2]), Vector3.up * 100, Color.blue, 60);
            
            //green should be at the same position as red
            Debug.DrawRay(combinedMeshFilter.transform.TransformPoint(verticesCombined[2]).OffsetY(100), Vector3.up * 100, Color.green, 60);

            //black should be at same position as blue
            Debug.DrawRay(combinedMeshFilter.transform.TransformPoint(verticesCombined[vertices1Before.Length + 2]).OffsetY(100), Vector3.up * 100, Color.black, 60);
        }
        #endregion

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
        public void DrawAllTangents()
        {
            for (int vertexIndex = 0; vertexIndex < chunk.ChunkMeshData.Vertices.Length; vertexIndex++)
            {
                DrawTangent(vertexIndex, Color.magenta);
            }
        }
        
        private void DrawTangent(int vertexIndex, Color color)
        {
            const float distance = 5;
            const float duration = 120;
            Debug.DrawRay(chunk.ChunkMeshData.GetCurrentVertexWorldPosition(vertexIndex), chunk.ChunkMeshData.Mesh.tangents[vertexIndex] * distance, color, duration);
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

        public void CheckToAssignSplineMeshIDs()
        {
            bool needsUpdating = false;
            SplineMesh[] splineMeshes = transform.GetComponentsInAllChildren<SplineMesh>().ToArray();
            
            //get spline meshes with duplicate IDs
            HashSet<string> duplicateIDs = new HashSet<string>();
            foreach (SplineMesh splineMesh in splineMeshes)
            {
                foreach (SplineMesh other in splineMeshes)
                {
                    if (other == splineMesh)
                        continue;

                    UniqueIDAssigner idAssigner = splineMesh.GetComponent<UniqueIDAssigner>();
                    UniqueIDAssigner otherIdAssigner = other.GetComponent<UniqueIDAssigner>();
                    if (idAssigner != null && otherIdAssigner != null && otherIdAssigner.UniqueID.Equals(idAssigner.UniqueID))
                    {
                        duplicateIDs.Add(splineMesh.GetComponent<UniqueIDAssigner>().UniqueID);
                        break;
                    }
                }
            }
            
            foreach (SplineMesh splineMesh in splineMeshes)
            {
                UniqueIDAssigner idAssigner = splineMesh.GetComponent<UniqueIDAssigner>();
                if (idAssigner == null
                    || idAssigner.UniqueID.IsNullOrEmpty()
                    || duplicateIDs.Contains(idAssigner.UniqueID)
                    || splineMesh.GetComponents<UniqueIDAssigner>().Length > 1)
                {
                    needsUpdating = true;
                    break;
                }
            }
            
            if (needsUpdating)
            {
                string assetPath = GameObjectUtils.GetPathToPrefabAsset(gameObject);
                
                bool isPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() != null && PrefabStageUtility.GetCurrentPrefabStage().assetPath.Equals(assetPath);
                GameObject prefabInstance = isPrefabMode ? PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot : PrefabUtility.LoadPrefabContents(assetPath);
                
                SplineMesh[] splineMeshesInPrefab = prefabInstance.transform.GetComponentsInAllChildren<SplineMesh>().ToArray();
                foreach (SplineMesh splineMesh in splineMeshesInPrefab)
                {
                    UniqueIDAssigner idAssigner = splineMesh.GetComponent<UniqueIDAssigner>();
                    if (idAssigner == null
                        || idAssigner.UniqueID.IsNullOrEmpty()
                        || duplicateIDs.Contains(idAssigner.UniqueID)
                        || splineMesh.GetComponents<UniqueIDAssigner>().Length > 1)
                    {
                        //remove all existing
                        foreach (UniqueIDAssigner uniqueIDAssigner in splineMesh.gameObject.GetComponents<UniqueIDAssigner>())
                            DestroyImmediate(uniqueIDAssigner);
                        
                        idAssigner = splineMesh.gameObject.AddComponent<UniqueIDAssigner>();
                        idAssigner.Initialise();
                        
                        Debug.Log($"Updated ID for spline mesh {splineMesh.name}");
                    }
                }

                if (!isPrefabMode)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }
            }
        }
        
        /// <summary>
        /// Force the terrain to be recreated.
        /// </summary>
        public void RecreateTerrain()
        {
            DisconnectAll();
            
            GlobalLoggers.ChunkLogger.Log($"Recreating terrain for '{chunk.name}'");
            
            //check if there's additional vertex color data
            VertexInstanceStream vertexInstanceStream = chunk.TerrainHighLOD.GetComponent<VertexInstanceStream>();
            GenericDictionary<int, List<VertexInstanceStream.PaintData>> paintData = null;
            if (vertexInstanceStream != null)
                paintData = vertexInstanceStream.paintedVertices;

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
            //destroy old terrain
            DestroyImmediate(chunk.TerrainHighLOD);
            DestroyImmediate(chunk.TerrainLowLOD);
            
            Dictionary<Chunk.ChunkLOD, GameObject> newTerrain = terrainData.Create(chunk);
            foreach (Chunk.ChunkLOD key in newTerrain.Keys)
            {
                GameObject terrain = newTerrain[key];
                chunk.SetTerrain(key, terrain);
            }
            
            chunk.SwitchTerrainLOD(Chunk.ChunkLOD.HIGH);
        }

        private void UnbakeSplineMeshes()
        {
            if (isRuntimeChunk)
                return;
            
            SplineMesh[] splineMeshes = transform.GetComponentsInAllChildren<SplineMesh>().ToArray();
            foreach (SplineMesh splineMesh in splineMeshes)
            {
                if (!splineMesh.gameObject.activeSelf)
                    continue;

                if (splineMesh.baked)
                {
                    splineMesh.Unbake();
                    EditorUtility.SetDirty(gameObject);
                }
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

        [Header("Chunk map search")]
        [SerializeField] private GumballEvent[] eventsToSearchIn;
        [SerializeField, ReadOnly] private ChunkMap[] mapsUsingChunk;
        
        [ButtonMethod]
        public void FindMapsUsingChunk()
        {
            if (eventsToSearchIn == null || eventsToSearchIn.Length == 0)
                throw new InvalidOperationException($"Add an event to search for in the {nameof(eventsToSearchIn)} field.");
                
            HashSet<ChunkMap> chunkMaps = new();
            //find all the chunks maps
            foreach (GumballEvent eventToSearchIn in eventsToSearchIn)
            {
                if (eventToSearchIn == null)
                    continue;
                
                foreach (AssetReferenceGameObject mapReference in eventToSearchIn.Maps)
                {
                    GameSessionMap map = mapReference.editorAsset.GetComponent<GameSessionMap>();
                    foreach (GameSessionNode node in map.Nodes)
                    {
                        ChunkMap chunkMap = node.GameSession.ChunkMapAssetReference.editorAsset;
                        if (chunkMap == null)
                            continue;
                        foreach (AssetReferenceGameObject chunkReference in chunkMap.ChunkReferences)
                        {
                            Chunk chunkInMap = chunkReference.editorAsset.GetComponent<Chunk>();
                            if (chunkInMap.UniqueID.Equals(chunk.UniqueID))
                            {
                                chunkMaps.Add(chunkMap);
                                break;
                            }
                        }
                    }
                }
            }

            mapsUsingChunk = chunkMaps.ToArray();
            Debug.Log($"Found {mapsUsingChunk.Length} chunk maps using chunk {gameObject.name}.");
        }

        [ButtonMethod]
        public void RebuildMapsUsingChunk()
        {
            FindMapsUsingChunk();
            
            if (Application.isPlaying)
                throw new InvalidOperationException("Cannot rebuild during play mode.");

            Stopwatch stopwatch = Stopwatch.StartNew();

            //rebuild the data (but only recreate runtime chunks once)
            foreach (ChunkMap chunkMap in mapsUsingChunk)
            {
                if (chunkMap == null)
                    continue;
                
                chunkMap.RebuildData(false);
            }
            
            //reset the runtime chunk creation tracking
            ChunkMap.ClearRuntimeChunksCreatedTracking();

            Debug.Log($"Rebuild process took {stopwatch.Elapsed.ToPrettyString()}");
        }
#endif

    }
}