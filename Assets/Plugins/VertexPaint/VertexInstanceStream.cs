using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/* Holds streams of data to override the colors or UVs on a mesh without making the mesh unique. This is more
 * memory efficient than burning the color data into many copies of a mesh, and much easier to manage. 
 * 
*/
namespace JBooth.VertexPainterPro
{
   [ExecuteInEditMode]
   public class VertexInstanceStream : MonoBehaviour
   {
      
      public bool keepRuntimeData;

      public VertexInstanceStreamData Data = new();
      
#if UNITY_EDITOR
      [Serializable]
      public struct PaintData
      {
         public Color color;
         public float strength;

         public PaintData(Color color, float strength)
         {
            this.color = color;
            this.strength = strength;
         }
      }

      public GenericDictionary<int, List<PaintData>> paintedVertices = new();
      
      public void TrackPaintData(int index, Color color, float strength)
      {
         if (!paintedVertices.ContainsKey(index))
            paintedVertices[index] = new List<PaintData>();
         
         paintedVertices[index].Add(new PaintData(color, strength));
      }
      
      public void SetPaintData(GenericDictionary<int, List<PaintData>> paintedVertices)
      {
         this.paintedVertices = paintedVertices;
         
         Color[] meshColors = GetComponent<MeshFilter>().sharedMesh.colors;
         if (Data.Colors == null || Data.Colors.Length != meshColors.Length)
            Data.Colors = meshColors;
         
         foreach (int index in paintedVertices.Keys)
         {
            List<PaintData> dataCollection = paintedVertices[index];
            foreach (PaintData data in dataCollection)
            {
               Data.Colors[index] = Color.Lerp(Data.Colors[index], data.color, data.strength);
            }
         }

         Apply();
      }
#endif
      
      public void SetData(VertexInstanceStreamData data)
      {
         Data = data;
      }
      
      public Color[] colors 
      { 
         get 
         { 
            return Data.Colors; 
         }
         set
         {
            enforcedColorChannels = (! (Data.Colors == null || (value != null && Data.Colors.Length != value.Length)));
            Data.Colors = value;
            Apply();
         }
      }

      public List<Vector4> uv0 { get { return Data.Uv0; } set { Data.Uv0 = value; Apply(); } }
      public List<Vector4> uv1 { get { return Data.Uv1; } set { Data.Uv1 = value; Apply(); } }
      public List<Vector4> uv2 { get { return Data.Uv2; } set { Data.Uv2 = value; Apply(); } }
      public List<Vector4> uv3 { get { return Data.Uv3; } set { Data.Uv3 = value; Apply(); } }
      public Vector3[] positions { get { return Data.Positions; } set { Data.Positions = value; Apply(); } }
      public Vector3[] normals { get { return Data.Normals; } set { Data.Normals = value; Apply(); } }
      public Vector4[] tangents { get { return Data.Tangents; } set { Data.Tangents = value; Apply(); } }

      #if UNITY_EDITOR
      Vector3[] cachedPositions;
      public Vector3 GetSafePosition(int index)
      {
         if (Data.Positions != null && index < Data.Positions.Length)
         {
            return Data.Positions[index];
         }
         if (cachedPositions == null)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedPositions = mf.sharedMesh.vertices;
         }
         if (index < cachedPositions.Length)
         {
            return cachedPositions[index];
         }
         return Vector3.zero;
      }

      Vector3[] cachedNormals;
      public Vector3 GetSafeNormal(int index)
      {
         if (Data.Normals != null && index < Data.Normals.Length)
         {
            return Data.Normals[index];
         }
         if (cachedPositions == null)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedNormals = mf.sharedMesh.normals;
         }
         if (cachedNormals != null && index < cachedNormals.Length)
         {
            return cachedNormals[index];
         }
         return new Vector3(0, 0, 1);
      }

      Vector4[] cachedTangents;
      public Vector4 GetSafeTangent(int index)
      {
         if (Data.Tangents != null && index < Data.Tangents.Length)
         {
            return Data.Tangents[index];
         }
         if (cachedTangents == null)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedTangents = mf.sharedMesh.tangents;
         }
         if (cachedTangents != null && index < cachedTangents.Length)
         {
            return cachedTangents[index];
         }
         return new Vector4(0, 1, 0, 1);
      }

      #endif

      #if UNITY_EDITOR
      // Stored here to make copy/save behaviour work better - basically, if you copy a mesh around, you want to also
      // clone the original material otherwise it may have the preview material stuck on it forever. 
      [HideInInspector]
      public Material[]       originalMaterial;
      public static Material  vertexShaderMat;


      void Awake()
      {
         // restore original material if we got saved with the preview material. 
         // I tried to do this in a number of ways; using the pre/post serialization callbacks seemed
         // like the best, but is actually not possible because they don't always both get called. In editor,
         // sometimes only the pre-serialization callback gets called. WTF..

         MeshRenderer mr = GetComponent<MeshRenderer>();
         if (mr != null)
         {
            if (mr.sharedMaterials != null && mr.sharedMaterial == vertexShaderMat && originalMaterial != null
               && originalMaterial.Length == mr.sharedMaterials.Length && originalMaterial.Length > 1)
            {
               Material[] mats = new Material[mr.sharedMaterials.Length];
               for (int i = 0; i < mr.sharedMaterials.Length; ++i)
               {
                  if (originalMaterial[i] != null)
                  {
                     mats[i] = originalMaterial[i];
                  }
               }
               mr.sharedMaterials = mats;
            }
            else if (originalMaterial != null && originalMaterial.Length > 0)
            {
               if (originalMaterial[0] != null)
               {
                  mr.sharedMaterial = originalMaterial[0];
               }
            }
         }
      }
      #endif

   	void Start()
      {
         Apply(!keepRuntimeData);
         if (keepRuntimeData)
         {
            var mf = GetComponent<MeshFilter>();
            Data.Positions = mf.sharedMesh.vertices;
         }
      }

      void OnDestroy()
      {
         if (!Application.isPlaying)
         {
            MeshRenderer mr = GetComponent<MeshRenderer> ();
            if ( mr != null )
               mr.additionalVertexStreams = null;
         }
      }

      bool enforcedColorChannels = false;
      void EnforceOriginalMeshHasColors(Mesh stream)
      {
         if (enforcedColorChannels == true)
            return;
         enforcedColorChannels = true;
         MeshFilter mf = GetComponent<MeshFilter>();
         Color[] origColors = mf.sharedMesh.colors;
         if (stream != null && stream.colors.Length > 0 && (origColors == null || origColors.Length == 0))
         {
            // workaround for unity bug; dispite docs claim, color channels must exist on the original mesh
            // for the additionalVertexStream to work. Which is, sad...
            mf.sharedMesh.colors = stream.colors;
         }
      }

      #if UNITY_EDITOR
      public void SetColor(Color c, int count) { Data.Colors = new Color[count]; for (int i = 0; i < count; ++i) { Data.Colors[i] = c; } Apply(); }
      public void SetUV0(Vector4 uv, int count) { Data.Uv0 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { Data.Uv0.Add(uv); } Apply(); }
      public void SetUV1(Vector4 uv, int count) { Data.Uv1 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { Data.Uv1.Add(uv); } Apply(); }
      public void SetUV2(Vector4 uv, int count) { Data.Uv2 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { Data.Uv2.Add(uv); } Apply(); }
      public void SetUV3(Vector4 uv, int count) { Data.Uv3 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { Data.Uv3.Add(uv); } Apply(); }

      public void SetUV0_XY(Vector2 uv, int count)
      {
         if (Data.Uv0 == null || Data.Uv0.Count != count)
         {
            Data.Uv0 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv0[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv0[i];
            v.x = uv.x;
            v.y = uv.y;
            Data.Uv0[i] = v;
         }
         Apply();
      }

      public void SetUV0_ZW(Vector2 uv, int count)
      {
         if (Data.Uv0 == null || Data.Uv0.Count != count)
         {
            Data.Uv0 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv0[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv0[i];
            v.z = uv.x;
            v.w = uv.y;
            Data.Uv0[i] = v;
         }
         Apply();
      }

      public void SetUV1_XY(Vector2 uv, int count)
      {
         if (Data.Uv1 == null || Data.Uv1.Count != count)
         {
            Data.Uv1 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv1[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv1[i];
            v.x = uv.x;
            v.y = uv.y;
            Data.Uv1[i] = v;
         }
         Apply();
      }

      public void SetUV1_ZW(Vector2 uv, int count)
      {
         if (Data.Uv1 == null || Data.Uv1.Count != count)
         {
            Data.Uv1 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv1[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv1[i];
            v.z = uv.x;
            v.w = uv.y;
            Data.Uv1[i] = v;
         }
         Apply();
      }

      public void SetUV2_XY(Vector2 uv, int count)
      {
         if (Data.Uv2 == null || Data.Uv2.Count != count)
         {
            Data.Uv2 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv2[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv2[i];
            v.x = uv.x;
            v.y = uv.y;
            Data.Uv2[i] = v;
         }
         Apply();
      }

      public void SetUV2_ZW(Vector2 uv, int count)
      {
         if (Data.Uv2 == null || Data.Uv2.Count != count)
         {
            Data.Uv2 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv2[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv2[i];
            v.z = uv.x;
            v.w = uv.y;
            Data.Uv2[i] = v;
         }
         Apply();
      }

      public void SetUV3_XY(Vector2 uv, int count)
      {
         if (Data.Uv3 == null || Data.Uv3.Count != count)
         {
            Data.Uv3 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv3[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv3[i];
            v.x = uv.x;
            v.y = uv.y;
            Data.Uv3[i] = v;
         }
         Apply();
      }

      public void SetUV3_ZW(Vector2 uv, int count)
      {
         if (Data.Uv3 == null || Data.Uv3.Count != count)
         {
            Data.Uv3 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               Data.Uv3[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = Data.Uv3[i];
            v.z = uv.x;
            v.w = uv.y;
            Data.Uv3[i] = v;
         }
         Apply();
      }

      public void SetColorRG(Vector2 rg, int count) 
      { 
         if (Data.Colors == null || Data.Colors.Length != count)
         {
            Data.Colors = new Color[count];
            enforcedColorChannels = false;
         }
         for (int i = 0; i < count; ++i)
         {
            Data.Colors[i].r = rg.x;
            Data.Colors[i].g = rg.y;
         }
         Apply();
      }

      public void SetColorBA(Vector2 ba, int count) 
      { 
         if (Data.Colors == null || Data.Colors.Length != count)
         {
            Data.Colors = new Color[count];
            enforcedColorChannels = false;
         }
         for (int i = 0; i < count; ++i)
         {
            Data.Colors[i].r = ba.x;
            Data.Colors[i].g = ba.y;
         }
         Apply();
      }
      #endif

      public Mesh Apply(bool markNoLongerReadable = true)
      {
         MeshRenderer mr = GetComponent<MeshRenderer>();
         MeshFilter mf = GetComponent<MeshFilter>();

         if (mr != null && mf != null && mf.sharedMesh != null)
         {
            int vertexCount = mf.sharedMesh.vertexCount;
            Mesh stream = meshStream;
            if (stream == null || vertexCount != stream.vertexCount)
            {
               if (stream != null)
               {
                  DestroyImmediate(stream);
               }
               stream = new Mesh();

               // even though the docs say you don't need to set the positions on your avs, you do.. 
               stream.vertices = new Vector3[mf.sharedMesh.vertexCount];

               // wtf, copy won't work?
               // so, originally I did a copyTo here, but with a unity patch release the behavior changed and
               // the verticies would all become 0. This seems a funny thing to change in a patch release, but
               // since getting the data from the C++ side creates a new array anyway, we don't really need
               // to copy them anyway since they are a unique copy already.
               stream.vertices = mf.sharedMesh.vertices;
               // another Unity bug, when in editor, the paint job will just disapear sometimes. So we have to re-assign
               // it every update (even though this doesn't get called each frame, it appears to loose the data during
               // the editor update call, which only happens occationaly. 
               stream.MarkDynamic();
               stream.triangles = mf.sharedMesh.triangles;
               meshStream = stream;

               stream.hideFlags = HideFlags.HideAndDontSave;
            }
            if (Data.Positions != null && Data.Positions.Length == vertexCount) { stream.vertices = Data.Positions; }
            if (Data.Normals != null && Data.Normals.Length == vertexCount) { stream.normals = Data.Normals; } else { stream.normals = null; }
            if (Data.Tangents != null && Data.Tangents.Length == vertexCount) { stream.tangents = Data.Tangents; } else { stream.tangents = null; }
            if (Data.Colors != null && Data.Colors.Length == vertexCount) { stream.colors = Data.Colors; } else { stream.colors = null; }
            if (Data.Uv0 != null && Data.Uv0.Count == vertexCount) { stream.SetUVs(0, Data.Uv0);  }  else { stream.uv = null; }
            if (Data.Uv1 != null && Data.Uv1.Count == vertexCount) { stream.SetUVs(1, Data.Uv1); }  else { stream.uv2 = null; }
            if (Data.Uv2 != null && Data.Uv2.Count == vertexCount) { stream.SetUVs(2, Data.Uv2); }  else { stream.uv3 = null; }
            if (Data.Uv3 != null && Data.Uv3.Count == vertexCount) { stream.SetUVs(3, Data.Uv3); }  else { stream.uv4 = null; }

            EnforceOriginalMeshHasColors(stream);
 
            if (!Application.isPlaying || Application.isEditor)
            {
               // only mark no longer readable in game..
               markNoLongerReadable = false;
            }

            stream.UploadMeshData(markNoLongerReadable);
            mr.additionalVertexStreams = stream;
            return stream;
         }
         return null;
      }

      // keep this around for updates..
      private Mesh meshStream;
      // In various versions of unity, you have to set .additionalVertexStreams every frame. 
      // This was broken in 5.3, then broken again in 5.5..

   #if UNITY_EDITOR

      public Mesh GetModifierMesh() { return meshStream; }
      private MeshRenderer meshRend = null;
      void Update()
      {
         // turns out this happens in play mode as well.. grrr..
         if (meshRend == null)
         {
            meshRend = GetComponent<MeshRenderer>();
         }
         //if (!Application.isPlaying)
         {
            meshRend.additionalVertexStreams = meshStream;
         }
      }
   #endif
   }
}
