using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MyBox;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gumball
{
    [RequireComponent(typeof(Camera))]
    public class EditorScreenshotCamera : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private string fileName = "Screenshot";
        [SerializeField] private string screenshotFolder = "Assets/_Gumball/Runtime/Sprites/Screenshots";
        [SerializeField] private int imageSize;

        [ButtonMethod]
        public void TakeScreenshot()
        {
            // Capture screenshot to render tex.
            var cam = GetComponent<Camera>();
            var prevTarget = cam.targetTexture;
            var tex = new RenderTexture(imageSize, imageSize, 32);
            cam.targetTexture = tex;
            cam.Render();

            // Convert to Texture2D to encode as png.
            var tex2D = ToTexture2D(tex);
            var bytes = tex2D.EncodeToPNG();

            // Create or replace output file.
            var path = PrepareFilePath();
            File.WriteAllBytes(path, bytes);

            // Cleanup
            cam.targetTexture = prevTarget;
            DestroyImmediate(tex);
            DestroyImmediate(tex2D);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private string PrepareFilePath()
        {
            var path = Path.Combine(screenshotFolder, fileName);
            if (!path.EndsWith(".png"))
                path = $"{path}.png";

            if (!Directory.Exists(screenshotFolder))
                throw new NullReferenceException($"Directory doesn't exist: {screenshotFolder}");

            if (File.Exists(path))
            {
                Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                throw new InvalidOperationException($"File already exists: {screenshotFolder}/{fileName}");
            }

            return path;
        }

        private Texture2D ToTexture2D(RenderTexture source)
        {
            var texture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false)
            {
                name = source.name + "_Tex2D"
            };

            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.RTToTexture)
            {
                Graphics.CopyTexture(source, texture);
            }
            else
            {
                var prevActive = RenderTexture.active;
                RenderTexture.active = source;

                texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                texture.Apply();

                RenderTexture.active = prevActive;
            }

            return texture;
        }
#endif
    }
}