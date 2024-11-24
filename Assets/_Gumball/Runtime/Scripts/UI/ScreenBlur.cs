using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gumball
{
    public class ScreenBlur : ScriptableRendererFeature
    {

        private static ScreenBlur instanceCached;
        public static ScreenBlur Instance
        {
            get
            {
                if (instanceCached == null)
                {
                    ScriptableRenderer renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).scriptableRenderer;
                    PropertyInfo property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

                    List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

                    foreach (var feature in features)
                    {
                        if (feature.GetType() == typeof(ScreenBlur))
                            instanceCached = feature as ScreenBlur;
                    }
                }
                
                return instanceCached;
            }
        }

        private static Tween currentTween;
        private static float currentRadius;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitialise()
        {
            //start disabled
            currentTween?.Kill();
            currentRadius = 1;
            SetRadius(1);
            Instance.SetActive(false);
        }

        public static void Show(bool show)
        {
            const int radius = 10;
            const float showDuration = 0.1f;
            const float hideDuration = 0.2f;
            
            if (show)
                Instance.SetActive(true);
            
            currentTween?.Kill();
            currentTween = DOTween.To(() => currentRadius, x => currentRadius = x, show ? radius : 0, show ? showDuration : hideDuration)
                .SetUpdate(UpdateType.Normal, true)
                .SetEase(Ease.InOutSine)
                .OnUpdate(() =>
                {
                    int desiredRadius = Mathf.CeilToInt(currentRadius);
                    SetRadius(desiredRadius);
                });
            
            Debug.Log($"{(show ? "Showing" : "Hiding")} screen blur.");
        }

        private static void SetRadius(int radius)
        {
            Instance.SetActive(radius > 0);

            CoroutineHelper.Instance.PerformAfterTrue(
                () => Instance.pass != null, 
                () => Instance.pass.blurSettings.Radius = radius);
            
#if UNITY_EDITOR
            CoroutineHelper.Instance.PerformAfterTrue(
                () => Instance.sceneview_pass != null, 
                () => Instance.sceneview_pass.blurSettings.Radius = radius);
#endif
        }

        internal abstract class BlurPass : ScriptableRenderPass, IDisposable
        {
            public FastBlurSettings blurSettings { get; set; } = new();
            public RenderTargetIdentifier colorSource { get; set; }

            public virtual void Dispose()
            {
            }
        }

        internal class BlurPassStandard : BlurPass
        {
            private Material blurMat => blurSettings.BlurMat;

            private RenderTargetHandle tempTexture;
            private RenderTexture BlurTexture;

            private int blurIterations => (int)blurSettings.Radius;
            private RenderTextureDescriptor renderTextureDescriptor1;
            private Resolution downScaledResolution;
            private bool isScene = false;

            public BlurPassStandard(RenderTextureDescriptor renderTextureDescriptor, bool isScene = false)
            {
                Init(renderTextureDescriptor);
                this.isScene = isScene;
                if (isScene)
                    tempTexture.Init("_tempTexture_scene");
                else
                    tempTexture.Init("_tempTexture");
            }

            public override void Dispose()
            {
                if (BlurTexture != null)
                    BlurTexture.Release();
            }

            public void Init(RenderTextureDescriptor renderTextureDescriptor)
            {
                renderTextureDescriptor.mipCount = 0;
                renderTextureDescriptor.useMipMap = false;
                renderTextureDescriptor.depthBufferBits = 0;
                renderTextureDescriptor.colorFormat = RenderTextureFormat.RGB111110Float;
                renderTextureDescriptor1 = renderTextureDescriptor;

                Dispose();

                const float baseMpx = 1920 * 1080;
                const float maxMpx = 8294400;
                float mpx = renderTextureDescriptor.width * renderTextureDescriptor.height;
                float t = Mathf.InverseLerp(baseMpx, maxMpx, mpx);
                t = Mathf.Clamp01(t);
                float scale = Mathf.Lerp(1, 0.5f, t);
                downScaledResolution = new Resolution();
                downScaledResolution.height = Mathf.FloorToInt(renderTextureDescriptor.height * scale);
                downScaledResolution.width = Mathf.FloorToInt(renderTextureDescriptor.width * scale);
                renderTextureDescriptor.width = downScaledResolution.width;
                renderTextureDescriptor.height = downScaledResolution.height;
                BlurTexture = new RenderTexture(renderTextureDescriptor.width, renderTextureDescriptor.height, 0, renderTextureDescriptor.colorFormat, 0);
                BlurTexture.filterMode = FilterMode.Bilinear;
                BlurTexture.name = isScene ? "_CameraBlurTexture_scene" : "_CameraBlurTexture";
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (renderingData.cameraData.cameraTargetDescriptor.width != renderTextureDescriptor1.width
                    || renderingData.cameraData.cameraTargetDescriptor.height != renderTextureDescriptor1.height)
                {
                    Init(renderingData.cameraData.cameraTargetDescriptor);
                }

                if (BlurTexture == null)
                {
                    Init(renderingData.cameraData.cameraTargetDescriptor);
                }

                Shader.SetGlobalTexture(BlurTexture.name, BlurTexture);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get("Fast Blur");
                cmd.GetTemporaryRT(tempTexture.id, downScaledResolution.width, downScaledResolution.height, 0, FilterMode.Bilinear, renderTextureDescriptor1.colorFormat);

                float offset = 0.5f;

                //first iteration of blur
                cmd.SetGlobalFloat("_Offset", offset);
                if (blurIterations == 1)
                {
                    cmd.Blit(colorSource, BlurTexture, blurMat, 2);
                }
                else
                {
                    cmd.Blit(colorSource, tempTexture.id, blurMat, 2);
                }

                offset = 0.5f;
                //rest of the iteration
                int iterationCount = 1;
                int maxIter = blurIterations;
                while (iterationCount < maxIter)
                {
                    cmd.SetGlobalFloat("_Offset", offset + (float)iterationCount);
                    cmd.Blit(tempTexture.id, BlurTexture, blurMat, 2);
                    iterationCount++;

                    if (iterationCount < blurIterations)
                    {
                        cmd.SetGlobalFloat("_Offset", offset + (float)iterationCount);
                        cmd.Blit(BlurTexture, tempTexture.id, blurMat, 2);
                        iterationCount++;

                        if (iterationCount >= blurIterations)
                        {
                            cmd.Blit(tempTexture.id, BlurTexture);
                        }
                    }
                }

                if (blurSettings.ShowBlurredTexture)
                {
                    cmd.Blit(BlurTexture, colorSource);
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(tempTexture.id);
            }
        }

        private void CreateMat()
        {
            Shader shader = Shader.Find("hidden/FastBlur");
            if (shader == null)
            {
                Debug.LogWarning("Cannot find hidden/FastBlur shader!");
                return;
            }

            Settings.BlurMat = CoreUtils.CreateEngineMaterial(shader);
        }

        private BlurPass pass = null;
#if UNITY_EDITOR
        private BlurPass sceneview_pass = null;
#endif
        public FastBlurSettings Settings = new()
        {
            BlurMat = null,
            Radius = 32,
            RenderQueue = RenderPassEvent.AfterRenderingTransparents,
            QueueOffset = 0,
            ShowBlurredTexture = false,
        };

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            Settings.Radius = Mathf.Min(Mathf.Max(Settings.Radius, 1), 128);
#if UNITY_EDITOR
            BlurPass currentPass = renderingData.cameraData.isSceneViewCamera ? sceneview_pass : pass;
#else
            BlurPass currentPass = pass;
#endif
            if (currentPass == null)
            {
#if UNITY_EDITOR
                if (renderingData.cameraData.isSceneViewCamera)
                {
                    currentPass = new BlurPassStandard(renderingData.cameraData.cameraTargetDescriptor, true);
                    sceneview_pass = currentPass;
                }
                else
                {
                    currentPass = new BlurPassStandard(renderingData.cameraData.cameraTargetDescriptor, false);
                    pass = currentPass;
                }
#else
                currentPass = new BlurPassStandard(renderingData.cameraData.cameraTargetDescriptor, false);
                pass = currentPass;
#endif
                CreateMat();
            }

            currentPass.blurSettings = Settings;

            if (Settings.BlurMat == null)
            {
                CreateMat();
                return;
            }

            currentPass.renderPassEvent = (Settings.RenderQueue + Settings.QueueOffset);
            renderer.EnqueuePass(currentPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
#if UNITY_EDITOR
            BlurPass currentPass = renderingData.cameraData.isSceneViewCamera ? sceneview_pass : pass;
#else
            BlurPass currentPass = pass;
#endif
            currentPass.colorSource = renderer.cameraColorTarget;
        }

        private void OnDestroy()
        {
            Dispose(true);
        }
        
        private void OnValidate()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (pass != null)
                pass.Dispose();

#if UNITY_EDITOR
            if (sceneview_pass != null)
                sceneview_pass.Dispose();
#endif
        }

        public override void Create()
        {
            pass = null;
            //nothing, creating pass on the fly
        }
    }

    [Serializable]
    public class FastBlurSettings
    {
        [Tooltip("Approximate blur radius")]
        [Range(1, 128)]
        public int Radius;

        [Tooltip("When to do blurring")]
        public RenderPassEvent RenderQueue;

        [Tooltip("When to do blurring + offset")]
        public int QueueOffset;

        [Tooltip("Render blurred texture to screen")]
        public bool ShowBlurredTexture;

        internal Material BlurMat { get; set; }
    }
}