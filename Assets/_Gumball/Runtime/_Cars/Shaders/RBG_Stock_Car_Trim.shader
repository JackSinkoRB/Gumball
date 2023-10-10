// Made with Amplify Shader Editor v1.9.2.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/Stock_Car_Trim"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		_MainTex("_MainTex", 2D) = "white" {}
		_Gloss_Metal("Gloss_Metal", 2D) = "white" {}
		_Mask_01("Mask_01", 2D) = "white" {}
		_Mask_02("Mask_02", 2D) = "white" {}
		_Mask_03("Mask_03", 2D) = "white" {}
		_Mask_04("Mask_04", 2D) = "white" {}
		_Mask_05("Mask_05", 2D) = "white" {}
		_Mask_06("Mask_06", 2D) = "white" {}
		_Mask_07("Mask_07", 2D) = "white" {}
		_Mask_08("Mask_08", 2D) = "white" {}
		_Mask_09("Mask_09", 2D) = "white" {}
		_Rollcage_Color("Rollcage_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Rollcage_Gloss("Rollcage_Gloss", Range( 0 , 1)) = 0.7
		_Rollcage_Metal("Rollcage_Metal", Range( 0 , 1)) = 0
		_Tub_Color("Tub_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Tub_Gloss("Tub_Gloss", Range( 0 , 1)) = 0
		_Tub_Metal("Tub_Metal", Range( 0 , 1)) = 0
		_EngineBay_Color("EngineBay_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_EngineBay_Gloss("EngineBay_Gloss", Range( 0 , 1)) = 0
		_EngineBay_Metal("EngineBay_Metal", Range( 0 , 1)) = 0
		_Wheel_01_Color("Wheel_01_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Wheel_01_Gloss("Wheel_01_Gloss", Range( 0 , 1)) = 0.7
		_Wheel_01_Metal("Wheel_01_Metal", Range( 0 , 1)) = 0
		_Wheel_02_Color("Wheel_02_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Wheel_02_Gloss("Wheel_02_Gloss", Range( 0 , 1)) = 0
		_Wheel_02_Metal("Wheel_02_Metal", Range( 0 , 1)) = 0
		_Wheel_03_Color("Wheel_03_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Wheel_03_Gloss("Wheel_03_Gloss", Range( 0 , 1)) = 0
		_Wheel_03_Metal("Wheel_03_Metal", Range( 0 , 1)) = 0
		_Seat_01_Color("Seat_01_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Seat_01_Gloss("Seat_01_Gloss", Range( 0 , 1)) = 0.7
		_Seat_01_Metal("Seat_01_Metal", Range( 0 , 1)) = 0
		_Seat_02_Color("Seat_02_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Seat_02_Gloss("Seat_02_Gloss", Range( 0 , 1)) = 0
		_Seat_02_Metal("Seat_02_Metal", Range( 0 , 1)) = 0
		_Seat_03_Color("Seat_03_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Seat_03_Gloss("Seat_03_Gloss", Range( 0 , 1)) = 0
		_Seat_03_Metal("Seat_03_Metal", Range( 0 , 1)) = 0
		_E_Brake_01_Color("E_Brake_01_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_E_Brake_01_Gloss("E_Brake_01_Gloss", Range( 0 , 1)) = 0.7
		_E_Brake_01_Metal("E_Brake_01_Metal", Range( 0 , 1)) = 0
		_E_Brake_02_Color("E_Brake_02_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_E_Brake_02_Gloss("E_Brake_02_Gloss", Range( 0 , 1)) = 0
		_E_Brake_02_Metal("E_Brake_02_Metal", Range( 0 , 1)) = 0
		_Shifter_01_Color("Shifter_01_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Shifter_01_Gloss("Shifter_01_Gloss", Range( 0 , 1)) = 0
		_Shifter_01_Metal("Shifter_01_Metal", Range( 0 , 1)) = 0
		_Shifter_02_Color("Shifter_02_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Shifter_02_Gloss("Shifter_02_Gloss", Range( 0 , 1)) = 0.7
		_Shifter_02_Metal("Shifter_02_Metal", Range( 0 , 1)) = 0
		_Tacho_01_Color("Tacho_01_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Tacho_01_Gloss("Tacho_01_Gloss", Range( 0 , 1)) = 0
		_Tacho_01_Metal("Tacho_01_Metal", Range( 0 , 1)) = 0
		_Tacho_02_Color("Tacho_02_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Tacho_02_Gloss("Tacho_02_Gloss", Range( 0 , 1)) = 0
		_Tacho_02_Metal("Tacho_02_Metal", Range( 0 , 1)) = 0
		_Tacho_03_Color("Tacho_03_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Tacho_03_Gloss("Tacho_03_Gloss", Range( 0 , 1)) = 0.7
		_Tacho_03_Metal("Tacho_03_Metal", Range( 0 , 1)) = 0
		_Cosmetic_01_Color("Cosmetic_01_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Cosmetic_01_Gloss("Cosmetic_01_Gloss", Range( 0 , 1)) = 0
		_Cosmetic_01_Metal("Cosmetic_01_Metal", Range( 0 , 1)) = 0
		_Cosmetic_02_Color("Cosmetic_02_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Cosmetic_02_Gloss("Cosmetic_02_Gloss", Range( 0 , 1)) = 0
		_Cosmetic_02_Metal("Cosmetic_02_Metal", Range( 0 , 1)) = 0
		_Cosmetic_03_Color("Cosmetic_03_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Cosmetic_03_Gloss("Cosmetic_03_Gloss", Range( 0 , 1)) = 0.7
		_Cosmetic_03_Metal("Cosmetic_03_Metal", Range( 0 , 1)) = 0
		_Cosmetic_04_Color("Cosmetic_04_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Cosmetic_04_Gloss("Cosmetic_04_Gloss", Range( 0 , 1)) = 0
		_Cosmetic_04_Metal("Cosmetic_04_Metal", Range( 0 , 1)) = 0
		_Cosmetic_05_Color("Cosmetic_05_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Cosmetic_05_Gloss("Cosmetic_05_Gloss", Range( 0 , 1)) = 0
		_Cosmetic_05_Metal("Cosmetic_05_Metal", Range( 0 , 1)) = 0
		_Cosmetic_06_Color("Cosmetic_06_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Cosmetic_06_Gloss("Cosmetic_06_Gloss", Range( 0 , 1)) = 0.7
		_Cosmetic_06_Metal("Cosmetic_06_Metal", Range( 0 , 1)) = 0
		_Cosmetic_07_Color("Cosmetic_07_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Cosmetic_07_Gloss("Cosmetic_07_Gloss", Range( 0 , 1)) = 0
		_Cosmetic_07_Metal("Cosmetic_07_Metal", Range( 0 , 1)) = 0
		_Alloy_01_Color("Alloy_01_Color", Color) = (0.4622642,0.4622642,0.4622642,0)
		_Alloy_01_Gloss("Alloy_01_Gloss", Range( 0 , 1)) = 0.7
		_Alloy_01_Metal("Alloy_01_Metal", Range( 0 , 1)) = 0
		_Alloy_02_Color("Alloy_02_Color", Color) = (0.2264151,0.2264151,0.2264151,0)
		_Alloy_02_Gloss("Alloy_02_Gloss", Range( 0 , 1)) = 0
		_Alloy_02_Metal("Alloy_02_Metal", Range( 0 , 1)) = 0
		_Alloy_03_Color("Alloy_03_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Alloy_03_Gloss("Alloy_03_Gloss", Range( 0 , 1)) = 0
		_Alloy_03_Metal("Alloy_03_Metal", Range( 0 , 1)) = 0
		_Alloy_04_Color("Alloy_04_Color", Color) = (0.2735849,0.2735849,0.2735849,0)
		_Alloy_04_Gloss("Alloy_04_Gloss", Range( 0 , 1)) = 0
		_Alloy_04_Metal("Alloy_04_Metal", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector][ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[HideInInspector][ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Lit" }

		Cull Back
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#pragma multi_compile _ _FORWARD_PLUS

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_FRAG_COLOR


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 clipPos : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float4 lightmapUVOrVertexSH : TEXCOORD1;
				half4 fogFactorAndVertexLight : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _Mask_01;
			sampler2D _Mask_02;
			sampler2D _Mask_03;
			sampler2D _Mask_04;
			sampler2D _Mask_05;
			sampler2D _Mask_06;
			sampler2D _Mask_07;
			sampler2D _Mask_08;
			sampler2D _Mask_09;
			sampler2D _Gloss_Metal;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord8.xy = v.texcoord1.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#endif

				#if !defined(LIGHTMAP_ON)
					OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord.xy;
					o.lightmapUVOrVertexSH.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );

				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif

				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				o.clipPosV = positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 uv1_MainTex = IN.ase_texcoord8.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv1_Mask_01 = IN.ase_texcoord8.xy * _Mask_01_ST.xy + _Mask_01_ST.zw;
				float4 tex2DNode41 = tex2D( _Mask_01, uv1_Mask_01 );
				float4 lerpResult35 = lerp( tex2D( _MainTex, uv1_MainTex ) , _Rollcage_Color , tex2DNode41.r);
				float4 lerpResult38 = lerp( lerpResult35 , _Tub_Color , tex2DNode41.g);
				float4 lerpResult40 = lerp( lerpResult38 , _EngineBay_Color , tex2DNode41.b);
				float2 uv1_Mask_02 = IN.ase_texcoord8.xy * _Mask_02_ST.xy + _Mask_02_ST.zw;
				float4 tex2DNode49 = tex2D( _Mask_02, uv1_Mask_02 );
				float4 lerpResult43 = lerp( lerpResult40 , _Wheel_01_Color , tex2DNode49.r);
				float4 lerpResult44 = lerp( lerpResult43 , _Wheel_02_Color , tex2DNode49.g);
				float4 lerpResult46 = lerp( lerpResult44 , _Wheel_03_Color , tex2DNode49.b);
				float2 uv1_Mask_03 = IN.ase_texcoord8.xy * _Mask_03_ST.xy + _Mask_03_ST.zw;
				float4 tex2DNode56 = tex2D( _Mask_03, uv1_Mask_03 );
				float4 lerpResult51 = lerp( lerpResult46 , _Seat_01_Color , tex2DNode56.r);
				float4 lerpResult52 = lerp( lerpResult51 , _Seat_02_Color , tex2DNode56.g);
				float4 lerpResult54 = lerp( lerpResult52 , _Seat_03_Color , tex2DNode56.b);
				float2 uv1_Mask_04 = IN.ase_texcoord8.xy * _Mask_04_ST.xy + _Mask_04_ST.zw;
				float4 tex2DNode63 = tex2D( _Mask_04, uv1_Mask_04 );
				float4 lerpResult58 = lerp( lerpResult54 , _E_Brake_01_Color , tex2DNode63.r);
				float4 lerpResult59 = lerp( lerpResult58 , _E_Brake_02_Color , tex2DNode63.g);
				float4 lerpResult61 = lerp( lerpResult59 , _Shifter_01_Color , tex2DNode63.b);
				float2 uv1_Mask_05 = IN.ase_texcoord8.xy * _Mask_05_ST.xy + _Mask_05_ST.zw;
				float4 tex2DNode70 = tex2D( _Mask_05, uv1_Mask_05 );
				float4 lerpResult65 = lerp( lerpResult61 , _Shifter_02_Color , tex2DNode70.r);
				float4 lerpResult66 = lerp( lerpResult65 , _Tacho_01_Color , tex2DNode70.g);
				float4 lerpResult68 = lerp( lerpResult66 , _Tacho_02_Color , tex2DNode70.b);
				float2 uv1_Mask_06 = IN.ase_texcoord8.xy * _Mask_06_ST.xy + _Mask_06_ST.zw;
				float4 tex2DNode77 = tex2D( _Mask_06, uv1_Mask_06 );
				float4 lerpResult72 = lerp( lerpResult68 , _Tacho_03_Color , tex2DNode77.r);
				float4 lerpResult73 = lerp( lerpResult72 , _Cosmetic_01_Color , tex2DNode77.g);
				float4 lerpResult75 = lerp( lerpResult73 , _Cosmetic_02_Color , tex2DNode77.b);
				float2 uv1_Mask_07 = IN.ase_texcoord8.xy * _Mask_07_ST.xy + _Mask_07_ST.zw;
				float4 tex2DNode81 = tex2D( _Mask_07, uv1_Mask_07 );
				float4 lerpResult78 = lerp( lerpResult75 , _Cosmetic_03_Color , tex2DNode81.r);
				float4 lerpResult79 = lerp( lerpResult78 , _Cosmetic_04_Color , tex2DNode81.g);
				float4 lerpResult80 = lerp( lerpResult79 , _Cosmetic_05_Color , tex2DNode81.b);
				float2 uv1_Mask_08 = IN.ase_texcoord8.xy * _Mask_08_ST.xy + _Mask_08_ST.zw;
				float4 tex2DNode86 = tex2D( _Mask_08, uv1_Mask_08 );
				float4 lerpResult87 = lerp( lerpResult80 , _Alloy_01_Color , tex2DNode86.r);
				float4 lerpResult89 = lerp( lerpResult87 , _Alloy_02_Color , tex2DNode86.g);
				float4 lerpResult91 = lerp( lerpResult89 , _Alloy_03_Color , tex2DNode86.b);
				float2 uv1_Mask_09 = IN.ase_texcoord8.xy * _Mask_09_ST.xy + _Mask_09_ST.zw;
				float4 tex2DNode202 = tex2D( _Mask_09, uv1_Mask_09 );
				float4 lerpResult208 = lerp( lerpResult91 , _Cosmetic_06_Color , tex2DNode202.r);
				float4 lerpResult212 = lerp( lerpResult208 , _Cosmetic_07_Color , tex2DNode202.g);
				float4 lerpResult218 = lerp( lerpResult212 , _Alloy_04_Color , tex2DNode202.b);
				
				float2 uv1_Gloss_Metal = IN.ase_texcoord8.xy * _Gloss_Metal_ST.xy + _Gloss_Metal_ST.zw;
				float4 tex2DNode32 = tex2D( _Gloss_Metal, uv1_Gloss_Metal );
				float4 temp_cast_1 = (_Rollcage_Metal).xxxx;
				float4 lerpResult105 = lerp( tex2DNode32 , temp_cast_1 , tex2DNode41.r);
				float4 temp_cast_2 = (_Tub_Metal).xxxx;
				float4 lerpResult106 = lerp( lerpResult105 , temp_cast_2 , tex2DNode41.g);
				float4 temp_cast_3 = (_EngineBay_Metal).xxxx;
				float4 lerpResult107 = lerp( lerpResult106 , temp_cast_3 , tex2DNode41.b);
				float4 temp_cast_4 = (_Wheel_01_Metal).xxxx;
				float4 lerpResult113 = lerp( lerpResult107 , temp_cast_4 , tex2DNode49.r);
				float4 temp_cast_5 = (_Wheel_02_Metal).xxxx;
				float4 lerpResult114 = lerp( lerpResult113 , temp_cast_5 , tex2DNode49.g);
				float4 temp_cast_6 = (_Wheel_03_Metal).xxxx;
				float4 lerpResult115 = lerp( lerpResult114 , temp_cast_6 , tex2DNode49.b);
				float4 temp_cast_7 = (_Seat_01_Metal).xxxx;
				float4 lerpResult134 = lerp( lerpResult115 , temp_cast_7 , tex2DNode56.r);
				float4 temp_cast_8 = (_Seat_02_Metal).xxxx;
				float4 lerpResult127 = lerp( lerpResult134 , temp_cast_8 , tex2DNode56.g);
				float4 temp_cast_9 = (_Seat_03_Metal).xxxx;
				float4 lerpResult132 = lerp( lerpResult127 , temp_cast_9 , tex2DNode56.b);
				float4 temp_cast_10 = (_E_Brake_01_Metal).xxxx;
				float4 lerpResult139 = lerp( lerpResult132 , temp_cast_10 , tex2DNode63.r);
				float4 temp_cast_11 = (_E_Brake_02_Metal).xxxx;
				float4 lerpResult135 = lerp( lerpResult139 , temp_cast_11 , tex2DNode63.g);
				float4 temp_cast_12 = (_Shifter_01_Metal).xxxx;
				float4 lerpResult137 = lerp( lerpResult135 , temp_cast_12 , tex2DNode63.b);
				float4 temp_cast_13 = (_Shifter_02_Metal).xxxx;
				float4 lerpResult152 = lerp( lerpResult137 , temp_cast_13 , tex2DNode70.r);
				float4 temp_cast_14 = (_Tacho_01_Metal).xxxx;
				float4 lerpResult149 = lerp( lerpResult152 , temp_cast_14 , tex2DNode70.g);
				float4 temp_cast_15 = (_Tacho_02_Metal).xxxx;
				float4 lerpResult154 = lerp( lerpResult149 , temp_cast_15 , tex2DNode70.b);
				float4 temp_cast_16 = (_Tacho_03_Metal).xxxx;
				float4 lerpResult166 = lerp( lerpResult154 , temp_cast_16 , tex2DNode77.r);
				float4 temp_cast_17 = (_Cosmetic_01_Metal).xxxx;
				float4 lerpResult160 = lerp( lerpResult166 , temp_cast_17 , tex2DNode77.g);
				float4 temp_cast_18 = (_Cosmetic_02_Metal).xxxx;
				float4 lerpResult164 = lerp( lerpResult160 , temp_cast_18 , tex2DNode77.b);
				float4 temp_cast_19 = (_Cosmetic_03_Metal).xxxx;
				float4 lerpResult176 = lerp( lerpResult164 , temp_cast_19 , tex2DNode81.r);
				float4 temp_cast_20 = (_Cosmetic_04_Metal).xxxx;
				float4 lerpResult171 = lerp( lerpResult176 , temp_cast_20 , tex2DNode81.g);
				float4 temp_cast_21 = (_Cosmetic_05_Metal).xxxx;
				float4 lerpResult175 = lerp( lerpResult171 , temp_cast_21 , tex2DNode81.b);
				float4 temp_cast_22 = (_Alloy_01_Metal).xxxx;
				float4 lerpResult193 = lerp( lerpResult175 , temp_cast_22 , tex2DNode86.r);
				float4 temp_cast_23 = (_Alloy_02_Metal).xxxx;
				float4 lerpResult191 = lerp( lerpResult193 , temp_cast_23 , tex2DNode86.g);
				float4 temp_cast_24 = (_Alloy_03_Metal).xxxx;
				float4 lerpResult189 = lerp( lerpResult191 , temp_cast_24 , tex2DNode86.b);
				float4 temp_cast_25 = (_Cosmetic_06_Metal).xxxx;
				float4 lerpResult203 = lerp( lerpResult189 , temp_cast_25 , tex2DNode202.r);
				float4 temp_cast_26 = (_Cosmetic_07_Metal).xxxx;
				float4 lerpResult206 = lerp( lerpResult203 , temp_cast_26 , tex2DNode202.g);
				float4 temp_cast_27 = (_Alloy_04_Metal).xxxx;
				float4 lerpResult211 = lerp( lerpResult206 , temp_cast_27 , tex2DNode202.b);
				
				float4 temp_cast_29 = (_Rollcage_Gloss).xxxx;
				float4 lerpResult97 = lerp( tex2DNode32 , temp_cast_29 , tex2DNode41.r);
				float4 temp_cast_30 = (_Tub_Gloss).xxxx;
				float4 lerpResult100 = lerp( lerpResult97 , temp_cast_30 , tex2DNode41.g);
				float4 temp_cast_31 = (_EngineBay_Gloss).xxxx;
				float4 lerpResult101 = lerp( lerpResult100 , temp_cast_31 , tex2DNode41.b);
				float4 temp_cast_32 = (_Wheel_01_Gloss).xxxx;
				float4 lerpResult111 = lerp( lerpResult101 , temp_cast_32 , tex2DNode49.r);
				float4 temp_cast_33 = (_Wheel_02_Gloss).xxxx;
				float4 lerpResult112 = lerp( lerpResult111 , temp_cast_33 , tex2DNode49.g);
				float4 temp_cast_34 = (_Wheel_03_Gloss).xxxx;
				float4 lerpResult116 = lerp( lerpResult112 , temp_cast_34 , tex2DNode49.b);
				float4 temp_cast_35 = (_Seat_01_Gloss).xxxx;
				float4 lerpResult133 = lerp( lerpResult116 , temp_cast_35 , tex2DNode56.r);
				float4 temp_cast_36 = (_Seat_02_Gloss).xxxx;
				float4 lerpResult128 = lerp( lerpResult133 , temp_cast_36 , tex2DNode56.g);
				float4 temp_cast_37 = (_Seat_03_Gloss).xxxx;
				float4 lerpResult126 = lerp( lerpResult128 , temp_cast_37 , tex2DNode56.b);
				float4 temp_cast_38 = (_E_Brake_01_Gloss).xxxx;
				float4 lerpResult138 = lerp( lerpResult126 , temp_cast_38 , tex2DNode63.r);
				float4 temp_cast_39 = (_E_Brake_02_Gloss).xxxx;
				float4 lerpResult136 = lerp( lerpResult138 , temp_cast_39 , tex2DNode63.g);
				float4 temp_cast_40 = (_Shifter_01_Gloss).xxxx;
				float4 lerpResult146 = lerp( lerpResult136 , temp_cast_40 , tex2DNode63.b);
				float4 temp_cast_41 = (_Shifter_02_Gloss).xxxx;
				float4 lerpResult147 = lerp( lerpResult146 , temp_cast_41 , tex2DNode70.r);
				float4 temp_cast_42 = (_Tacho_01_Gloss).xxxx;
				float4 lerpResult151 = lerp( lerpResult147 , temp_cast_42 , tex2DNode70.g);
				float4 temp_cast_43 = (_Tacho_02_Gloss).xxxx;
				float4 lerpResult153 = lerp( lerpResult151 , temp_cast_43 , tex2DNode70.b);
				float4 temp_cast_44 = (_Tacho_03_Gloss).xxxx;
				float4 lerpResult161 = lerp( lerpResult153 , temp_cast_44 , tex2DNode77.r);
				float4 temp_cast_45 = (_Cosmetic_01_Gloss).xxxx;
				float4 lerpResult162 = lerp( lerpResult161 , temp_cast_45 , tex2DNode77.g);
				float4 temp_cast_46 = (_Cosmetic_02_Gloss).xxxx;
				float4 lerpResult163 = lerp( lerpResult162 , temp_cast_46 , tex2DNode77.b);
				float4 temp_cast_47 = (_Cosmetic_03_Gloss).xxxx;
				float4 lerpResult172 = lerp( lerpResult163 , temp_cast_47 , tex2DNode81.r);
				float4 temp_cast_48 = (_Cosmetic_04_Gloss).xxxx;
				float4 lerpResult173 = lerp( lerpResult172 , temp_cast_48 , tex2DNode81.g);
				float4 temp_cast_49 = (_Cosmetic_05_Gloss).xxxx;
				float4 lerpResult174 = lerp( lerpResult173 , temp_cast_49 , tex2DNode81.b);
				float4 temp_cast_50 = (_Alloy_01_Gloss).xxxx;
				float4 lerpResult188 = lerp( lerpResult174 , temp_cast_50 , tex2DNode86.r);
				float4 temp_cast_51 = (_Alloy_02_Gloss).xxxx;
				float4 lerpResult190 = lerp( lerpResult188 , temp_cast_51 , tex2DNode86.g);
				float4 temp_cast_52 = (_Alloy_03_Gloss).xxxx;
				float4 lerpResult192 = lerp( lerpResult190 , temp_cast_52 , tex2DNode86.b);
				float4 temp_cast_53 = (_Cosmetic_06_Gloss).xxxx;
				float4 lerpResult204 = lerp( lerpResult192 , temp_cast_53 , tex2DNode202.r);
				float4 temp_cast_54 = (_Cosmetic_07_Gloss).xxxx;
				float4 lerpResult209 = lerp( lerpResult204 , temp_cast_54 , tex2DNode202.g);
				float4 temp_cast_55 = (_Alloy_04_Gloss).xxxx;
				float4 lerpResult210 = lerp( lerpResult209 , temp_cast_55 , tex2DNode202.b);
				

				float3 BaseColor = saturate( ( lerpResult218 * ( IN.ase_color * IN.ase_color ) ) ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = lerpResult211.r;
				float Smoothness = lerpResult210.r;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.clipPos.z;
				#endif

				#ifdef _CLEARCOAT
					float CoatMask = 0;
					float CoatSmoothness = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;

				#ifdef _NORMALMAP
						#if _NORMAL_DROPOFF_TS
							inputData.normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent, WorldBiTangent, WorldNormal));
						#elif _NORMAL_DROPOFF_OS
							inputData.normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							inputData.normalWS = Normal;
						#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = ShadowCoords;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif
					inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
				#else
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = BaseColor;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;

				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.clipPos, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;

					#define SUM_LIGHT_TRANSMISSION(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 transmission = max( 0, -dot( inputData.normalWS, Light.direction ) ) * atten * Transmission;\
						color.rgb += BaseColor * transmission;

					SUM_LIGHT_TRANSMISSION( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_FORWARD_PLUS
							for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSMISSION( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSMISSION( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#define SUM_LIGHT_TRANSLUCENCY(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 lightDir = Light.direction + inputData.normalWS * normal;\
						half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );\
						half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;\
						color.rgb += BaseColor * translucency * strength;

					SUM_LIGHT_TRANSLUCENCY( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_FORWARD_PLUS
							for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSLUCENCY( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSLUCENCY( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_REFRACTION
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 clipPos : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif				
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = clipPos;
				o.clipPosV = clipPos;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(	VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.clipPos.z;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			
			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 clipPos : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				o.clipPosV = positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(	VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.clipPos.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature EDITOR_VISUALIZATION

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_FRAG_COLOR


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef EDITOR_VISUALIZATION
					float4 VizUV : TEXCOORD2;
					float4 LightCoord : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _Mask_01;
			sampler2D _Mask_02;
			sampler2D _Mask_03;
			sampler2D _Mask_04;
			sampler2D _Mask_05;
			sampler2D _Mask_06;
			sampler2D _Mask_07;
			sampler2D _Mask_08;
			sampler2D _Mask_09;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord4.xy = v.texcoord1.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );

				#ifdef EDITOR_VISUALIZATION
					float2 VizUV = 0;
					float4 LightCoord = 0;
					UnityEditorVizData(v.vertex.xyz, v.texcoord0.xy, v.texcoord1.xy, v.texcoord2.xy, VizUV, LightCoord);
					o.VizUV = float4(VizUV, 0, 0);
					o.LightCoord = LightCoord;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord0 = patch[0].texcoord0 * bary.x + patch[1].texcoord0 * bary.y + patch[2].texcoord0 * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv1_MainTex = IN.ase_texcoord4.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv1_Mask_01 = IN.ase_texcoord4.xy * _Mask_01_ST.xy + _Mask_01_ST.zw;
				float4 tex2DNode41 = tex2D( _Mask_01, uv1_Mask_01 );
				float4 lerpResult35 = lerp( tex2D( _MainTex, uv1_MainTex ) , _Rollcage_Color , tex2DNode41.r);
				float4 lerpResult38 = lerp( lerpResult35 , _Tub_Color , tex2DNode41.g);
				float4 lerpResult40 = lerp( lerpResult38 , _EngineBay_Color , tex2DNode41.b);
				float2 uv1_Mask_02 = IN.ase_texcoord4.xy * _Mask_02_ST.xy + _Mask_02_ST.zw;
				float4 tex2DNode49 = tex2D( _Mask_02, uv1_Mask_02 );
				float4 lerpResult43 = lerp( lerpResult40 , _Wheel_01_Color , tex2DNode49.r);
				float4 lerpResult44 = lerp( lerpResult43 , _Wheel_02_Color , tex2DNode49.g);
				float4 lerpResult46 = lerp( lerpResult44 , _Wheel_03_Color , tex2DNode49.b);
				float2 uv1_Mask_03 = IN.ase_texcoord4.xy * _Mask_03_ST.xy + _Mask_03_ST.zw;
				float4 tex2DNode56 = tex2D( _Mask_03, uv1_Mask_03 );
				float4 lerpResult51 = lerp( lerpResult46 , _Seat_01_Color , tex2DNode56.r);
				float4 lerpResult52 = lerp( lerpResult51 , _Seat_02_Color , tex2DNode56.g);
				float4 lerpResult54 = lerp( lerpResult52 , _Seat_03_Color , tex2DNode56.b);
				float2 uv1_Mask_04 = IN.ase_texcoord4.xy * _Mask_04_ST.xy + _Mask_04_ST.zw;
				float4 tex2DNode63 = tex2D( _Mask_04, uv1_Mask_04 );
				float4 lerpResult58 = lerp( lerpResult54 , _E_Brake_01_Color , tex2DNode63.r);
				float4 lerpResult59 = lerp( lerpResult58 , _E_Brake_02_Color , tex2DNode63.g);
				float4 lerpResult61 = lerp( lerpResult59 , _Shifter_01_Color , tex2DNode63.b);
				float2 uv1_Mask_05 = IN.ase_texcoord4.xy * _Mask_05_ST.xy + _Mask_05_ST.zw;
				float4 tex2DNode70 = tex2D( _Mask_05, uv1_Mask_05 );
				float4 lerpResult65 = lerp( lerpResult61 , _Shifter_02_Color , tex2DNode70.r);
				float4 lerpResult66 = lerp( lerpResult65 , _Tacho_01_Color , tex2DNode70.g);
				float4 lerpResult68 = lerp( lerpResult66 , _Tacho_02_Color , tex2DNode70.b);
				float2 uv1_Mask_06 = IN.ase_texcoord4.xy * _Mask_06_ST.xy + _Mask_06_ST.zw;
				float4 tex2DNode77 = tex2D( _Mask_06, uv1_Mask_06 );
				float4 lerpResult72 = lerp( lerpResult68 , _Tacho_03_Color , tex2DNode77.r);
				float4 lerpResult73 = lerp( lerpResult72 , _Cosmetic_01_Color , tex2DNode77.g);
				float4 lerpResult75 = lerp( lerpResult73 , _Cosmetic_02_Color , tex2DNode77.b);
				float2 uv1_Mask_07 = IN.ase_texcoord4.xy * _Mask_07_ST.xy + _Mask_07_ST.zw;
				float4 tex2DNode81 = tex2D( _Mask_07, uv1_Mask_07 );
				float4 lerpResult78 = lerp( lerpResult75 , _Cosmetic_03_Color , tex2DNode81.r);
				float4 lerpResult79 = lerp( lerpResult78 , _Cosmetic_04_Color , tex2DNode81.g);
				float4 lerpResult80 = lerp( lerpResult79 , _Cosmetic_05_Color , tex2DNode81.b);
				float2 uv1_Mask_08 = IN.ase_texcoord4.xy * _Mask_08_ST.xy + _Mask_08_ST.zw;
				float4 tex2DNode86 = tex2D( _Mask_08, uv1_Mask_08 );
				float4 lerpResult87 = lerp( lerpResult80 , _Alloy_01_Color , tex2DNode86.r);
				float4 lerpResult89 = lerp( lerpResult87 , _Alloy_02_Color , tex2DNode86.g);
				float4 lerpResult91 = lerp( lerpResult89 , _Alloy_03_Color , tex2DNode86.b);
				float2 uv1_Mask_09 = IN.ase_texcoord4.xy * _Mask_09_ST.xy + _Mask_09_ST.zw;
				float4 tex2DNode202 = tex2D( _Mask_09, uv1_Mask_09 );
				float4 lerpResult208 = lerp( lerpResult91 , _Cosmetic_06_Color , tex2DNode202.r);
				float4 lerpResult212 = lerp( lerpResult208 , _Cosmetic_07_Color , tex2DNode202.g);
				float4 lerpResult218 = lerp( lerpResult212 , _Alloy_04_Color , tex2DNode202.b);
				

				float3 BaseColor = saturate( ( lerpResult218 * ( IN.ase_color * IN.ase_color ) ) ).rgb;
				float3 Emission = 0;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BaseColor;
				metaInput.Emission = Emission;
				#ifdef EDITOR_VISUALIZATION
					metaInput.VizUV = IN.VizUV.xy;
					metaInput.LightCoord = IN.LightCoord;
				#endif

				return UnityMetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_FRAG_COLOR


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _Mask_01;
			sampler2D _Mask_02;
			sampler2D _Mask_03;
			sampler2D _Mask_04;
			sampler2D _Mask_05;
			sampler2D _Mask_06;
			sampler2D _Mask_07;
			sampler2D _Mask_08;
			sampler2D _Mask_09;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord2.xy = v.ase_texcoord1.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv1_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv1_Mask_01 = IN.ase_texcoord2.xy * _Mask_01_ST.xy + _Mask_01_ST.zw;
				float4 tex2DNode41 = tex2D( _Mask_01, uv1_Mask_01 );
				float4 lerpResult35 = lerp( tex2D( _MainTex, uv1_MainTex ) , _Rollcage_Color , tex2DNode41.r);
				float4 lerpResult38 = lerp( lerpResult35 , _Tub_Color , tex2DNode41.g);
				float4 lerpResult40 = lerp( lerpResult38 , _EngineBay_Color , tex2DNode41.b);
				float2 uv1_Mask_02 = IN.ase_texcoord2.xy * _Mask_02_ST.xy + _Mask_02_ST.zw;
				float4 tex2DNode49 = tex2D( _Mask_02, uv1_Mask_02 );
				float4 lerpResult43 = lerp( lerpResult40 , _Wheel_01_Color , tex2DNode49.r);
				float4 lerpResult44 = lerp( lerpResult43 , _Wheel_02_Color , tex2DNode49.g);
				float4 lerpResult46 = lerp( lerpResult44 , _Wheel_03_Color , tex2DNode49.b);
				float2 uv1_Mask_03 = IN.ase_texcoord2.xy * _Mask_03_ST.xy + _Mask_03_ST.zw;
				float4 tex2DNode56 = tex2D( _Mask_03, uv1_Mask_03 );
				float4 lerpResult51 = lerp( lerpResult46 , _Seat_01_Color , tex2DNode56.r);
				float4 lerpResult52 = lerp( lerpResult51 , _Seat_02_Color , tex2DNode56.g);
				float4 lerpResult54 = lerp( lerpResult52 , _Seat_03_Color , tex2DNode56.b);
				float2 uv1_Mask_04 = IN.ase_texcoord2.xy * _Mask_04_ST.xy + _Mask_04_ST.zw;
				float4 tex2DNode63 = tex2D( _Mask_04, uv1_Mask_04 );
				float4 lerpResult58 = lerp( lerpResult54 , _E_Brake_01_Color , tex2DNode63.r);
				float4 lerpResult59 = lerp( lerpResult58 , _E_Brake_02_Color , tex2DNode63.g);
				float4 lerpResult61 = lerp( lerpResult59 , _Shifter_01_Color , tex2DNode63.b);
				float2 uv1_Mask_05 = IN.ase_texcoord2.xy * _Mask_05_ST.xy + _Mask_05_ST.zw;
				float4 tex2DNode70 = tex2D( _Mask_05, uv1_Mask_05 );
				float4 lerpResult65 = lerp( lerpResult61 , _Shifter_02_Color , tex2DNode70.r);
				float4 lerpResult66 = lerp( lerpResult65 , _Tacho_01_Color , tex2DNode70.g);
				float4 lerpResult68 = lerp( lerpResult66 , _Tacho_02_Color , tex2DNode70.b);
				float2 uv1_Mask_06 = IN.ase_texcoord2.xy * _Mask_06_ST.xy + _Mask_06_ST.zw;
				float4 tex2DNode77 = tex2D( _Mask_06, uv1_Mask_06 );
				float4 lerpResult72 = lerp( lerpResult68 , _Tacho_03_Color , tex2DNode77.r);
				float4 lerpResult73 = lerp( lerpResult72 , _Cosmetic_01_Color , tex2DNode77.g);
				float4 lerpResult75 = lerp( lerpResult73 , _Cosmetic_02_Color , tex2DNode77.b);
				float2 uv1_Mask_07 = IN.ase_texcoord2.xy * _Mask_07_ST.xy + _Mask_07_ST.zw;
				float4 tex2DNode81 = tex2D( _Mask_07, uv1_Mask_07 );
				float4 lerpResult78 = lerp( lerpResult75 , _Cosmetic_03_Color , tex2DNode81.r);
				float4 lerpResult79 = lerp( lerpResult78 , _Cosmetic_04_Color , tex2DNode81.g);
				float4 lerpResult80 = lerp( lerpResult79 , _Cosmetic_05_Color , tex2DNode81.b);
				float2 uv1_Mask_08 = IN.ase_texcoord2.xy * _Mask_08_ST.xy + _Mask_08_ST.zw;
				float4 tex2DNode86 = tex2D( _Mask_08, uv1_Mask_08 );
				float4 lerpResult87 = lerp( lerpResult80 , _Alloy_01_Color , tex2DNode86.r);
				float4 lerpResult89 = lerp( lerpResult87 , _Alloy_02_Color , tex2DNode86.g);
				float4 lerpResult91 = lerp( lerpResult89 , _Alloy_03_Color , tex2DNode86.b);
				float2 uv1_Mask_09 = IN.ase_texcoord2.xy * _Mask_09_ST.xy + _Mask_09_ST.zw;
				float4 tex2DNode202 = tex2D( _Mask_09, uv1_Mask_09 );
				float4 lerpResult208 = lerp( lerpResult91 , _Cosmetic_06_Color , tex2DNode202.r);
				float4 lerpResult212 = lerp( lerpResult208 , _Cosmetic_07_Color , tex2DNode202.g);
				float4 lerpResult218 = lerp( lerpResult212 , _Alloy_04_Color , tex2DNode202.b);
				

				float3 BaseColor = saturate( ( lerpResult218 * ( IN.ase_color * IN.ase_color ) ) ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				half4 color = half4(BaseColor, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 clipPos : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 worldTangent : TEXCOORD2;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD3;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD4;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 tangentWS = float4(TransformObjectToWorldDir( v.ase_tangent.xyz), v.ase_tangent.w);
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;
				o.worldTangent = tangentWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				o.clipPosV = positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag(	VertexOutput IN
						, out half4 outNormalWS : SV_Target0
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float3 Normal = float3(0, 0, 1);
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.clipPos.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(WorldNormal);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float crossSign = (WorldTangent.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
							float3 bitangent = crossSign * cross(WorldNormal.xyz, WorldTangent.xyz);
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent.xyz, bitangent, WorldNormal.xyz));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = WorldNormal;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif
			
			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_FRAG_COLOR


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 clipPos : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float4 lightmapUVOrVertexSH : TEXCOORD1;
				half4 fogFactorAndVertexLight : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _Mask_01;
			sampler2D _Mask_02;
			sampler2D _Mask_03;
			sampler2D _Mask_04;
			sampler2D _Mask_05;
			sampler2D _Mask_06;
			sampler2D _Mask_07;
			sampler2D _Mask_08;
			sampler2D _Mask_09;
			sampler2D _Gloss_Metal;


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord8.xy = v.texcoord1.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
					OUTPUT_SH(normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz);
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord.xy;
					o.lightmapUVOrVertexSH.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );

				o.fogFactorAndVertexLight = half4(0, vertexLight);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				o.clipPosV = positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			FragmentOutput frag ( VertexOutput IN
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.clipPos );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#else
					ShadowCoords = float4(0, 0, 0, 0);
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 uv1_MainTex = IN.ase_texcoord8.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv1_Mask_01 = IN.ase_texcoord8.xy * _Mask_01_ST.xy + _Mask_01_ST.zw;
				float4 tex2DNode41 = tex2D( _Mask_01, uv1_Mask_01 );
				float4 lerpResult35 = lerp( tex2D( _MainTex, uv1_MainTex ) , _Rollcage_Color , tex2DNode41.r);
				float4 lerpResult38 = lerp( lerpResult35 , _Tub_Color , tex2DNode41.g);
				float4 lerpResult40 = lerp( lerpResult38 , _EngineBay_Color , tex2DNode41.b);
				float2 uv1_Mask_02 = IN.ase_texcoord8.xy * _Mask_02_ST.xy + _Mask_02_ST.zw;
				float4 tex2DNode49 = tex2D( _Mask_02, uv1_Mask_02 );
				float4 lerpResult43 = lerp( lerpResult40 , _Wheel_01_Color , tex2DNode49.r);
				float4 lerpResult44 = lerp( lerpResult43 , _Wheel_02_Color , tex2DNode49.g);
				float4 lerpResult46 = lerp( lerpResult44 , _Wheel_03_Color , tex2DNode49.b);
				float2 uv1_Mask_03 = IN.ase_texcoord8.xy * _Mask_03_ST.xy + _Mask_03_ST.zw;
				float4 tex2DNode56 = tex2D( _Mask_03, uv1_Mask_03 );
				float4 lerpResult51 = lerp( lerpResult46 , _Seat_01_Color , tex2DNode56.r);
				float4 lerpResult52 = lerp( lerpResult51 , _Seat_02_Color , tex2DNode56.g);
				float4 lerpResult54 = lerp( lerpResult52 , _Seat_03_Color , tex2DNode56.b);
				float2 uv1_Mask_04 = IN.ase_texcoord8.xy * _Mask_04_ST.xy + _Mask_04_ST.zw;
				float4 tex2DNode63 = tex2D( _Mask_04, uv1_Mask_04 );
				float4 lerpResult58 = lerp( lerpResult54 , _E_Brake_01_Color , tex2DNode63.r);
				float4 lerpResult59 = lerp( lerpResult58 , _E_Brake_02_Color , tex2DNode63.g);
				float4 lerpResult61 = lerp( lerpResult59 , _Shifter_01_Color , tex2DNode63.b);
				float2 uv1_Mask_05 = IN.ase_texcoord8.xy * _Mask_05_ST.xy + _Mask_05_ST.zw;
				float4 tex2DNode70 = tex2D( _Mask_05, uv1_Mask_05 );
				float4 lerpResult65 = lerp( lerpResult61 , _Shifter_02_Color , tex2DNode70.r);
				float4 lerpResult66 = lerp( lerpResult65 , _Tacho_01_Color , tex2DNode70.g);
				float4 lerpResult68 = lerp( lerpResult66 , _Tacho_02_Color , tex2DNode70.b);
				float2 uv1_Mask_06 = IN.ase_texcoord8.xy * _Mask_06_ST.xy + _Mask_06_ST.zw;
				float4 tex2DNode77 = tex2D( _Mask_06, uv1_Mask_06 );
				float4 lerpResult72 = lerp( lerpResult68 , _Tacho_03_Color , tex2DNode77.r);
				float4 lerpResult73 = lerp( lerpResult72 , _Cosmetic_01_Color , tex2DNode77.g);
				float4 lerpResult75 = lerp( lerpResult73 , _Cosmetic_02_Color , tex2DNode77.b);
				float2 uv1_Mask_07 = IN.ase_texcoord8.xy * _Mask_07_ST.xy + _Mask_07_ST.zw;
				float4 tex2DNode81 = tex2D( _Mask_07, uv1_Mask_07 );
				float4 lerpResult78 = lerp( lerpResult75 , _Cosmetic_03_Color , tex2DNode81.r);
				float4 lerpResult79 = lerp( lerpResult78 , _Cosmetic_04_Color , tex2DNode81.g);
				float4 lerpResult80 = lerp( lerpResult79 , _Cosmetic_05_Color , tex2DNode81.b);
				float2 uv1_Mask_08 = IN.ase_texcoord8.xy * _Mask_08_ST.xy + _Mask_08_ST.zw;
				float4 tex2DNode86 = tex2D( _Mask_08, uv1_Mask_08 );
				float4 lerpResult87 = lerp( lerpResult80 , _Alloy_01_Color , tex2DNode86.r);
				float4 lerpResult89 = lerp( lerpResult87 , _Alloy_02_Color , tex2DNode86.g);
				float4 lerpResult91 = lerp( lerpResult89 , _Alloy_03_Color , tex2DNode86.b);
				float2 uv1_Mask_09 = IN.ase_texcoord8.xy * _Mask_09_ST.xy + _Mask_09_ST.zw;
				float4 tex2DNode202 = tex2D( _Mask_09, uv1_Mask_09 );
				float4 lerpResult208 = lerp( lerpResult91 , _Cosmetic_06_Color , tex2DNode202.r);
				float4 lerpResult212 = lerp( lerpResult208 , _Cosmetic_07_Color , tex2DNode202.g);
				float4 lerpResult218 = lerp( lerpResult212 , _Alloy_04_Color , tex2DNode202.b);
				
				float2 uv1_Gloss_Metal = IN.ase_texcoord8.xy * _Gloss_Metal_ST.xy + _Gloss_Metal_ST.zw;
				float4 tex2DNode32 = tex2D( _Gloss_Metal, uv1_Gloss_Metal );
				float4 temp_cast_1 = (_Rollcage_Metal).xxxx;
				float4 lerpResult105 = lerp( tex2DNode32 , temp_cast_1 , tex2DNode41.r);
				float4 temp_cast_2 = (_Tub_Metal).xxxx;
				float4 lerpResult106 = lerp( lerpResult105 , temp_cast_2 , tex2DNode41.g);
				float4 temp_cast_3 = (_EngineBay_Metal).xxxx;
				float4 lerpResult107 = lerp( lerpResult106 , temp_cast_3 , tex2DNode41.b);
				float4 temp_cast_4 = (_Wheel_01_Metal).xxxx;
				float4 lerpResult113 = lerp( lerpResult107 , temp_cast_4 , tex2DNode49.r);
				float4 temp_cast_5 = (_Wheel_02_Metal).xxxx;
				float4 lerpResult114 = lerp( lerpResult113 , temp_cast_5 , tex2DNode49.g);
				float4 temp_cast_6 = (_Wheel_03_Metal).xxxx;
				float4 lerpResult115 = lerp( lerpResult114 , temp_cast_6 , tex2DNode49.b);
				float4 temp_cast_7 = (_Seat_01_Metal).xxxx;
				float4 lerpResult134 = lerp( lerpResult115 , temp_cast_7 , tex2DNode56.r);
				float4 temp_cast_8 = (_Seat_02_Metal).xxxx;
				float4 lerpResult127 = lerp( lerpResult134 , temp_cast_8 , tex2DNode56.g);
				float4 temp_cast_9 = (_Seat_03_Metal).xxxx;
				float4 lerpResult132 = lerp( lerpResult127 , temp_cast_9 , tex2DNode56.b);
				float4 temp_cast_10 = (_E_Brake_01_Metal).xxxx;
				float4 lerpResult139 = lerp( lerpResult132 , temp_cast_10 , tex2DNode63.r);
				float4 temp_cast_11 = (_E_Brake_02_Metal).xxxx;
				float4 lerpResult135 = lerp( lerpResult139 , temp_cast_11 , tex2DNode63.g);
				float4 temp_cast_12 = (_Shifter_01_Metal).xxxx;
				float4 lerpResult137 = lerp( lerpResult135 , temp_cast_12 , tex2DNode63.b);
				float4 temp_cast_13 = (_Shifter_02_Metal).xxxx;
				float4 lerpResult152 = lerp( lerpResult137 , temp_cast_13 , tex2DNode70.r);
				float4 temp_cast_14 = (_Tacho_01_Metal).xxxx;
				float4 lerpResult149 = lerp( lerpResult152 , temp_cast_14 , tex2DNode70.g);
				float4 temp_cast_15 = (_Tacho_02_Metal).xxxx;
				float4 lerpResult154 = lerp( lerpResult149 , temp_cast_15 , tex2DNode70.b);
				float4 temp_cast_16 = (_Tacho_03_Metal).xxxx;
				float4 lerpResult166 = lerp( lerpResult154 , temp_cast_16 , tex2DNode77.r);
				float4 temp_cast_17 = (_Cosmetic_01_Metal).xxxx;
				float4 lerpResult160 = lerp( lerpResult166 , temp_cast_17 , tex2DNode77.g);
				float4 temp_cast_18 = (_Cosmetic_02_Metal).xxxx;
				float4 lerpResult164 = lerp( lerpResult160 , temp_cast_18 , tex2DNode77.b);
				float4 temp_cast_19 = (_Cosmetic_03_Metal).xxxx;
				float4 lerpResult176 = lerp( lerpResult164 , temp_cast_19 , tex2DNode81.r);
				float4 temp_cast_20 = (_Cosmetic_04_Metal).xxxx;
				float4 lerpResult171 = lerp( lerpResult176 , temp_cast_20 , tex2DNode81.g);
				float4 temp_cast_21 = (_Cosmetic_05_Metal).xxxx;
				float4 lerpResult175 = lerp( lerpResult171 , temp_cast_21 , tex2DNode81.b);
				float4 temp_cast_22 = (_Alloy_01_Metal).xxxx;
				float4 lerpResult193 = lerp( lerpResult175 , temp_cast_22 , tex2DNode86.r);
				float4 temp_cast_23 = (_Alloy_02_Metal).xxxx;
				float4 lerpResult191 = lerp( lerpResult193 , temp_cast_23 , tex2DNode86.g);
				float4 temp_cast_24 = (_Alloy_03_Metal).xxxx;
				float4 lerpResult189 = lerp( lerpResult191 , temp_cast_24 , tex2DNode86.b);
				float4 temp_cast_25 = (_Cosmetic_06_Metal).xxxx;
				float4 lerpResult203 = lerp( lerpResult189 , temp_cast_25 , tex2DNode202.r);
				float4 temp_cast_26 = (_Cosmetic_07_Metal).xxxx;
				float4 lerpResult206 = lerp( lerpResult203 , temp_cast_26 , tex2DNode202.g);
				float4 temp_cast_27 = (_Alloy_04_Metal).xxxx;
				float4 lerpResult211 = lerp( lerpResult206 , temp_cast_27 , tex2DNode202.b);
				
				float4 temp_cast_29 = (_Rollcage_Gloss).xxxx;
				float4 lerpResult97 = lerp( tex2DNode32 , temp_cast_29 , tex2DNode41.r);
				float4 temp_cast_30 = (_Tub_Gloss).xxxx;
				float4 lerpResult100 = lerp( lerpResult97 , temp_cast_30 , tex2DNode41.g);
				float4 temp_cast_31 = (_EngineBay_Gloss).xxxx;
				float4 lerpResult101 = lerp( lerpResult100 , temp_cast_31 , tex2DNode41.b);
				float4 temp_cast_32 = (_Wheel_01_Gloss).xxxx;
				float4 lerpResult111 = lerp( lerpResult101 , temp_cast_32 , tex2DNode49.r);
				float4 temp_cast_33 = (_Wheel_02_Gloss).xxxx;
				float4 lerpResult112 = lerp( lerpResult111 , temp_cast_33 , tex2DNode49.g);
				float4 temp_cast_34 = (_Wheel_03_Gloss).xxxx;
				float4 lerpResult116 = lerp( lerpResult112 , temp_cast_34 , tex2DNode49.b);
				float4 temp_cast_35 = (_Seat_01_Gloss).xxxx;
				float4 lerpResult133 = lerp( lerpResult116 , temp_cast_35 , tex2DNode56.r);
				float4 temp_cast_36 = (_Seat_02_Gloss).xxxx;
				float4 lerpResult128 = lerp( lerpResult133 , temp_cast_36 , tex2DNode56.g);
				float4 temp_cast_37 = (_Seat_03_Gloss).xxxx;
				float4 lerpResult126 = lerp( lerpResult128 , temp_cast_37 , tex2DNode56.b);
				float4 temp_cast_38 = (_E_Brake_01_Gloss).xxxx;
				float4 lerpResult138 = lerp( lerpResult126 , temp_cast_38 , tex2DNode63.r);
				float4 temp_cast_39 = (_E_Brake_02_Gloss).xxxx;
				float4 lerpResult136 = lerp( lerpResult138 , temp_cast_39 , tex2DNode63.g);
				float4 temp_cast_40 = (_Shifter_01_Gloss).xxxx;
				float4 lerpResult146 = lerp( lerpResult136 , temp_cast_40 , tex2DNode63.b);
				float4 temp_cast_41 = (_Shifter_02_Gloss).xxxx;
				float4 lerpResult147 = lerp( lerpResult146 , temp_cast_41 , tex2DNode70.r);
				float4 temp_cast_42 = (_Tacho_01_Gloss).xxxx;
				float4 lerpResult151 = lerp( lerpResult147 , temp_cast_42 , tex2DNode70.g);
				float4 temp_cast_43 = (_Tacho_02_Gloss).xxxx;
				float4 lerpResult153 = lerp( lerpResult151 , temp_cast_43 , tex2DNode70.b);
				float4 temp_cast_44 = (_Tacho_03_Gloss).xxxx;
				float4 lerpResult161 = lerp( lerpResult153 , temp_cast_44 , tex2DNode77.r);
				float4 temp_cast_45 = (_Cosmetic_01_Gloss).xxxx;
				float4 lerpResult162 = lerp( lerpResult161 , temp_cast_45 , tex2DNode77.g);
				float4 temp_cast_46 = (_Cosmetic_02_Gloss).xxxx;
				float4 lerpResult163 = lerp( lerpResult162 , temp_cast_46 , tex2DNode77.b);
				float4 temp_cast_47 = (_Cosmetic_03_Gloss).xxxx;
				float4 lerpResult172 = lerp( lerpResult163 , temp_cast_47 , tex2DNode81.r);
				float4 temp_cast_48 = (_Cosmetic_04_Gloss).xxxx;
				float4 lerpResult173 = lerp( lerpResult172 , temp_cast_48 , tex2DNode81.g);
				float4 temp_cast_49 = (_Cosmetic_05_Gloss).xxxx;
				float4 lerpResult174 = lerp( lerpResult173 , temp_cast_49 , tex2DNode81.b);
				float4 temp_cast_50 = (_Alloy_01_Gloss).xxxx;
				float4 lerpResult188 = lerp( lerpResult174 , temp_cast_50 , tex2DNode86.r);
				float4 temp_cast_51 = (_Alloy_02_Gloss).xxxx;
				float4 lerpResult190 = lerp( lerpResult188 , temp_cast_51 , tex2DNode86.g);
				float4 temp_cast_52 = (_Alloy_03_Gloss).xxxx;
				float4 lerpResult192 = lerp( lerpResult190 , temp_cast_52 , tex2DNode86.b);
				float4 temp_cast_53 = (_Cosmetic_06_Gloss).xxxx;
				float4 lerpResult204 = lerp( lerpResult192 , temp_cast_53 , tex2DNode202.r);
				float4 temp_cast_54 = (_Cosmetic_07_Gloss).xxxx;
				float4 lerpResult209 = lerp( lerpResult204 , temp_cast_54 , tex2DNode202.g);
				float4 temp_cast_55 = (_Alloy_04_Gloss).xxxx;
				float4 lerpResult210 = lerp( lerpResult209 , temp_cast_55 , tex2DNode202.b);
				

				float3 BaseColor = saturate( ( lerpResult218 * ( IN.ase_color * IN.ase_color ) ) ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = lerpResult211.r;
				float Smoothness = lerpResult210.r;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.clipPos.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.positionCS = IN.clipPos;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
						inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
						inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
						inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = WorldNormal;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( WorldViewDirection );

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#else
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
					#else
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
					#endif
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(IN.clipPos,
						BaseColor,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData
				(BaseColor, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

			#define SCENESELECTIONPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				o.clipPos = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;

				#ifdef SCENESELECTIONPASS
					outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
					outColor = _SelectionID;
				#endif

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 140008


			#pragma vertex vert
			#pragma fragment frag

		    #define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Tacho_03_Color;
			float4 _Mask_06_ST;
			float4 _Cosmetic_01_Color;
			float4 _Cosmetic_02_Color;
			float4 _Cosmetic_03_Color;
			float4 _Mask_07_ST;
			float4 _Cosmetic_04_Color;
			float4 _Tacho_02_Color;
			float4 _Cosmetic_05_Color;
			float4 _Mask_08_ST;
			float4 _Alloy_03_Color;
			float4 _Cosmetic_06_Color;
			float4 _Mask_09_ST;
			float4 _Cosmetic_07_Color;
			float4 _Alloy_04_Color;
			float4 _Gloss_Metal_ST;
			float4 _Alloy_01_Color;
			float4 _Tacho_01_Color;
			float4 _Alloy_02_Color;
			float4 _Shifter_02_Color;
			float4 _Mask_05_ST;
			float4 _Rollcage_Color;
			float4 _Mask_01_ST;
			float4 _Tub_Color;
			float4 _Wheel_01_Color;
			float4 _Mask_02_ST;
			float4 _Wheel_02_Color;
			float4 _Wheel_03_Color;
			float4 _EngineBay_Color;
			float4 _Mask_03_ST;
			float4 _Seat_02_Color;
			float4 _Seat_03_Color;
			float4 _E_Brake_01_Color;
			float4 _Mask_04_ST;
			float4 _E_Brake_02_Color;
			float4 _Seat_01_Color;
			float4 _Shifter_01_Color;
			half _E_Brake_02_Gloss;
			half _E_Brake_01_Gloss;
			half _Seat_03_Gloss;
			half _Seat_02_Gloss;
			half _Seat_01_Gloss;
			half _Wheel_01_Gloss;
			half _Wheel_02_Gloss;
			half _EngineBay_Gloss;
			half _Shifter_01_Gloss;
			half _Tub_Gloss;
			half _Wheel_03_Gloss;
			half _Shifter_02_Gloss;
			half _Alloy_01_Gloss;
			half _Tacho_02_Gloss;
			half _Tacho_03_Gloss;
			half _Cosmetic_01_Gloss;
			half _Cosmetic_02_Gloss;
			half _Cosmetic_03_Gloss;
			half _Cosmetic_04_Gloss;
			half _Cosmetic_05_Gloss;
			half _Alloy_02_Gloss;
			half _Alloy_03_Gloss;
			half _Cosmetic_06_Gloss;
			half _Rollcage_Gloss;
			half _Tacho_01_Gloss;
			half _Alloy_04_Metal;
			half _Seat_02_Metal;
			half _Cosmetic_06_Metal;
			half _Rollcage_Metal;
			half _Tub_Metal;
			half _EngineBay_Metal;
			half _Wheel_01_Metal;
			half _Wheel_02_Metal;
			half _Wheel_03_Metal;
			half _Seat_01_Metal;
			half _Cosmetic_07_Gloss;
			half _Seat_03_Metal;
			half _E_Brake_01_Metal;
			half _E_Brake_02_Metal;
			half _Cosmetic_07_Metal;
			half _Shifter_01_Metal;
			half _Tacho_01_Metal;
			half _Tacho_02_Metal;
			half _Tacho_03_Metal;
			half _Cosmetic_01_Metal;
			half _Cosmetic_02_Metal;
			half _Cosmetic_03_Metal;
			half _Cosmetic_04_Metal;
			half _Cosmetic_05_Metal;
			half _Alloy_01_Metal;
			half _Alloy_02_Metal;
			half _Alloy_03_Metal;
			half _Shifter_02_Metal;
			half _Alloy_04_Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
						clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;

				#ifdef SCENESELECTIONPASS
					outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
					outColor = _SelectionID;
				#endif

				return outColor;
			}

			ENDHLSL
		}
		
	}
	
	CustomEditor "ASEMaterialInspector"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19201
Node;AmplifyShaderEditor.SamplerNode;41;-3252.042,-1290.175;Inherit;True;Property;_Mask_01;Mask_01;2;0;Create;True;0;0;0;False;0;False;-1;None;e95e78aaa2c53eb41bc1b9ee0f4c3755;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;32;-2370.408,-1706.742;Inherit;True;Property;_Gloss_Metal;Gloss_Metal;1;0;Create;True;0;0;0;False;0;False;-1;None;c89a93175e5462945b2facfa0ad67b80;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;105;-1799.123,-1428.358;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-3253.591,-1569.286;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;86000ee89ca204a4bb91401228093f4f;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;36;-3106.441,-941.2389;Inherit;False;Property;_Rollcage_Color;Rollcage_Color;11;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;97;-2177.842,-1309.165;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;106;-1594.031,-1306.842;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;37;-3104.849,-753.3101;Inherit;False;Property;_Tub_Color;Tub_Color;14;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;35;-2717.714,-1194.13;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;100;-1974.701,-1241.961;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;101;-1773.477,-1122.092;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;39;-3092.529,-555.8329;Inherit;False;Property;_EngineBay_Color;EngineBay_Color;17;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2641509,0.2641509,0.2641509,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;107;-1447.11,-1147.066;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;49;-2881.169,-131.7701;Inherit;True;Property;_Mask_02;Mask_02;3;0;Create;True;0;0;0;False;0;False;-1;None;9386d95a0c2e31d468ae49fb88cbfc78;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;38;-2583.17,-930.2803;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;111;-1740.02,-389.6241;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;40;-2439.918,-614.7582;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;42;-2840.341,121.0546;Inherit;False;Property;_Wheel_01_Color;Wheel_01_Color;20;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;113;-1356.1,-508.8177;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;114;-1160.109,-387.3011;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;47;-2768.756,337.8041;Inherit;False;Property;_Wheel_02_Color;Wheel_02_Color;23;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;112;-1540.779,-322.4201;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;43;-2201.107,-167.7966;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;116;-1339.555,-202.5509;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;44;-2084.613,66.46158;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;45;-2756.435,535.2797;Inherit;False;Property;_Wheel_03_Color;Wheel_03_Color;26;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2735848,0.2735848,0.2735848,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;115;-986.187,-181.5249;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;56;-1929.687,708.7496;Inherit;True;Property;_Mask_03;Mask_03;4;0;Create;True;0;0;0;False;0;False;-1;None;bb757dfa98b31894bba61e494fa53f76;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;133;-942.9877,430.7923;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;46;-1840.614,308.8588;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;50;-1869.136,931.0283;Inherit;False;Property;_Seat_01_Color;Seat_01_Color;29;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;134;-559.0684,311.5988;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;128;-743.7466,497.9963;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;127;-363.0765,433.1153;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;51;-1502.403,678.1057;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;55;-1867.544,1118.957;Inherit;False;Property;_Seat_02_Color;Seat_02_Color;32;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;63;-1224.766,1742.106;Inherit;True;Property;_Mask_04;Mask_04;5;0;Create;True;0;0;0;False;0;False;-1;None;c44e79051161b4b4ebede040bfdd72a6;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;132;-189.1556,638.8912;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;52;-1320.008,889.0809;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;126;-549.0223,625.6653;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;53;-1855.224,1316.433;Inherit;False;Property;_Seat_03_Color;Seat_03_Color;35;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2735848,0.2735848,0.2735848,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;139;360.4612,1413.684;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;54;-1190.653,1231.487;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;57;-1127.554,1977.75;Inherit;False;Property;_E_Brake_01_Color;E_Brake_01_Color;38;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;138;-23.45788,1532.877;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;135;556.4535,1535.2;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;58;-675.6572,1666.251;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;136;175.7829,1600.081;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;62;-1125.962,2165.679;Inherit;False;Property;_E_Brake_02_Color;E_Brake_02_Color;41;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;146;370.5073,1727.75;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;137;730.3748,1740.976;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;70;-572.2147,2652.499;Inherit;True;Property;_Mask_05;Mask_05;6;0;Create;True;0;0;0;False;0;False;-1;None;773e060c44c7c25449fc8f7eaa8271c3;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;59;-441.8222,1894.335;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;60;-1113.641,2363.155;Inherit;False;Property;_Shifter_01_Color;Shifter_01_Color;44;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2735848,0.2735848,0.2735848,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;147;677.0058,2390.939;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;152;1060.924,2271.747;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;61;-197.8241,2136.734;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;64;-458.1416,2868.456;Inherit;False;Property;_Shifter_02_Color;Shifter_02_Color;47;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;69;-456.5497,3056.385;Inherit;False;Property;_Tacho_01_Color;Tacho_01_Color;50;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;65;-6.250392,2556.959;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;149;1256.917,2393.262;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;151;876.2466,2458.144;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-444.2296,3253.861;Inherit;False;Property;_Tacho_02_Color;Tacho_02_Color;53;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2735848,0.2735848,0.2735848,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;153;1070.97,2585.813;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;66;227.5843,2785.041;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;77;172.8203,3617.764;Inherit;True;Property;_Mask_06;Mask_06;7;0;Create;True;0;0;0;False;0;False;-1;None;379751bfbd10f3640adf3aa5cdb05377;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;154;1430.838,2599.039;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;161;1353.24,3282.186;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;68;471.5838,3027.44;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;166;1737.158,3162.994;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;71;289.8314,3833.538;Inherit;False;Property;_Tacho_03_Color;Tacho_03_Color;56;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;162;1552.481,3349.391;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;76;291.4233,4021.469;Inherit;False;Property;_Cosmetic_01_Color;Cosmetic_01_Color;59;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;72;741.728,3522.04;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;160;1933.151,3284.509;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;163;1782.861,3487.038;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;73;975.563,3750.124;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;164;2107.07,3490.286;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;74;303.7444,4218.948;Inherit;False;Property;_Cosmetic_02_Color;Cosmetic_02_Color;62;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2735848,0.2735848,0.2735848,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;75;1318.548,4013.733;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;5563.51,6134.245;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;167;1359.076,3814.017;Half;False;Property;_Cosmetic_02_Gloss;Cosmetic_02_Gloss;63;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;168;1345.689,3729.877;Half;False;Property;_Cosmetic_01_Gloss;Cosmetic_01_Gloss;60;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;1326.153,3643.144;Half;False;Property;_Tacho_03_Gloss;Tacho_03_Gloss;57;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;1686.547,3807.476;Half;False;Property;_Cosmetic_02_Metal;Cosmetic_02_Metal;64;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;169;1686.846,3713.844;Half;False;Property;_Cosmetic_01_Metal;Cosmetic_01_Metal;61;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;159;1687.787,3625.959;Half;False;Property;_Tacho_03_Metal;Tacho_03_Metal;58;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;156;653.9184,2733.897;Half;False;Property;_Shifter_02_Gloss;Shifter_02_Gloss;48;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;669.4548,2831.63;Half;False;Property;_Tacho_01_Gloss;Tacho_01_Gloss;51;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;157;678.8418,2920.77;Half;False;Property;_Tacho_02_Gloss;Tacho_02_Gloss;54;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;155;1011.313,2916.229;Half;False;Property;_Tacho_02_Metal;Tacho_02_Metal;55;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;1008.808,2822.597;Half;False;Property;_Tacho_01_Metal;Tacho_01_Metal;52;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;1014.147,2732.908;Half;False;Property;_Shifter_02_Metal;Shifter_02_Metal;49;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;-21.62188,2062.707;Half;False;Property;_Shifter_01_Gloss;Shifter_01_Gloss;45;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-31.00891,1973.567;Half;False;Property;_E_Brake_02_Gloss;E_Brake_02_Gloss;42;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-46.54531,1875.834;Half;False;Property;_E_Brake_01_Gloss;E_Brake_01_Gloss;39;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;313.683,1876.649;Half;False;Property;_E_Brake_01_Metal;E_Brake_01_Metal;40;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;144;310.1481,1964.534;Half;False;Property;_E_Brake_02_Metal;E_Brake_02_Metal;43;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;145;312.0931,2051.944;Half;False;Property;_Shifter_01_Metal;Shifter_01_Metal;46;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;-941.1516,960.6219;Half;False;Property;_Seat_03_Gloss;Seat_03_Gloss;36;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-950.5386,871.4818;Half;False;Property;_Seat_02_Gloss;Seat_02_Gloss;33;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-966.0751,773.7488;Half;False;Property;_Seat_01_Gloss;Seat_01_Gloss;30;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-607.4365,949.8586;Half;False;Property;_Seat_03_Metal;Seat_03_Metal;37;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-609.3814,862.4489;Half;False;Property;_Seat_02_Metal;Seat_02_Metal;34;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-606.8465,774.564;Half;False;Property;_Seat_01_Metal;Seat_01_Metal;31;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-1756.262,-41.19105;Half;False;Property;_Wheel_01_Gloss;Wheel_01_Gloss;21;0;Create;True;0;0;0;False;0;False;0.7;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-1747.57,51.06554;Half;False;Property;_Wheel_02_Gloss;Wheel_02_Gloss;24;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-1738.183,140.2056;Half;False;Property;_Wheel_03_Gloss;Wheel_03_Gloss;27;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;-1406.414,42.03258;Half;False;Property;_Wheel_02_Metal;Wheel_02_Metal;25;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-1404.469,129.4422;Half;False;Property;_Wheel_03_Metal;Wheel_03_Metal;28;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-1403.879,-45.85218;Half;False;Property;_Wheel_01_Metal;Wheel_01_Metal;22;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-2172.106,-779.3355;Half;False;Property;_EngineBay_Gloss;EngineBay_Gloss;18;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-2181.493,-868.4754;Half;False;Property;_Tub_Gloss;Tub_Gloss;15;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-2190.184,-959.3156;Half;False;Property;_Rollcage_Gloss;Rollcage_Gloss;12;0;Create;True;0;0;0;False;0;False;0.7;0.623;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1838.391,-790.0988;Half;False;Property;_EngineBay_Metal;EngineBay_Metal;19;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-1840.336,-877.5084;Half;False;Property;_Tub_Metal;Tub_Metal;16;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-1837.801,-965.3932;Half;False;Property;_Rollcage_Metal;Rollcage_Metal;13;0;Create;True;0;0;0;False;0;False;0;0.664;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;8;5081.397,6350.143;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;5298.161,6363.495;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;81;1256.305,4610.479;Inherit;True;Property;_Mask_07;Mask_07;8;0;Create;True;0;0;0;False;0;False;-1;None;f6e5ce97dedc04841887627fc684967f;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;176;2711.622,4132.639;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;172;2327.703,4251.831;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;82;1368.702,4837.424;Inherit;False;Property;_Cosmetic_03_Color;Cosmetic_03_Color;65;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;171;2907.615,4254.154;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;83;1370.294,5025.356;Inherit;False;Property;_Cosmetic_04_Color;Cosmetic_04_Color;68;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;78;1820.599,4525.925;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;173;2526.944,4319.036;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;174;2721.667,4446.706;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;175;3084.387,4439.963;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;79;2054.434,4754.01;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;84;1382.615,5222.835;Inherit;False;Property;_Cosmetic_05_Color;Cosmetic_05_Color;71;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.2735848,0.2735848,0.2735848,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;86;2149.081,5539.043;Inherit;True;Property;_Mask_08;Mask_08;9;0;Create;True;0;0;0;False;0;False;-1;None;041f0867aa962e547b87243856cd5232;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;85;2236.152,5740.786;Inherit;False;Property;_Alloy_01_Color;Alloy_01_Color;80;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.6980392,0.6980392,0.6980392,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;188;2999.802,4971.406;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;193;3385.43,4871.003;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;80;2298.434,4996.409;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;191;3581.423,4992.518;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;88;2240.999,5923.28;Inherit;False;Property;_Alloy_02_Color;Alloy_02_Color;83;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.6981132,0.6981132,0.6981132,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;87;2609.469,5439.491;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;190;3200.752,5057.4;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;89;2763.017,5766.649;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;185;3353.608,5682.891;Half;False;Property;_Alloy_03_Metal;Alloy_03_Metal;88;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;3353.908,5589.259;Half;False;Property;_Alloy_02_Metal;Alloy_02_Metal;85;0;Create;True;0;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;186;3349.724,5503.087;Half;False;Property;_Alloy_01_Metal;Alloy_01_Metal;82;0;Create;True;0;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;2968.336,5652.114;Half;False;Property;_Alloy_02_Gloss;Alloy_02_Gloss;84;0;Create;True;0;0;0;False;0;False;0;0.942;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;2948.8,5565.382;Half;False;Property;_Alloy_01_Gloss;Alloy_01_Gloss;81;0;Create;True;0;0;0;False;0;False;0.7;0.9012752;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;180;2333.539,4783.663;Half;False;Property;_Cosmetic_05_Gloss;Cosmetic_05_Gloss;72;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;181;2320.152,4699.522;Half;False;Property;_Cosmetic_04_Gloss;Cosmetic_04_Gloss;69;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;182;2300.616,4612.79;Half;False;Property;_Cosmetic_03_Gloss;Cosmetic_03_Gloss;66;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;2661.01,4777.122;Half;False;Property;_Cosmetic_05_Metal;Cosmetic_05_Metal;73;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;2661.31,4683.49;Half;False;Property;_Cosmetic_04_Metal;Cosmetic_04_Metal;70;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;177;2662.25,4595.604;Half;False;Property;_Cosmetic_03_Metal;Cosmetic_03_Metal;67;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;194;2981.723,5737.255;Half;False;Property;_Alloy_03_Gloss;Alloy_03_Gloss;87;0;Create;False;0;0;0;False;0;False;0;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;204;3149.707,6738.829;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;206;3729.619,6741.152;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;209;3348.948,6806.034;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;211;3906.391,6926.961;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;212;2876.438,7241.008;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;218;3120.438,7483.407;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;208;2727.817,7006.195;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;210;3489.164,6937.338;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;189;4193.167,5684.474;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;192;3778.455,5883.974;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;91;2668.321,6267.302;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;202;2105.56,7097.477;Inherit;True;Property;_Mask_09;Mask_09;10;0;Create;True;0;0;0;False;0;False;-1;None;066d556da3986b94ba7d10b8c2981aad;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;205;2190.704,7324.422;Inherit;False;Property;_Cosmetic_06_Color;Cosmetic_06_Color;74;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.462264,0.462264,0.462264,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;207;2192.295,7512.354;Inherit;False;Property;_Cosmetic_07_Color;Cosmetic_07_Color;77;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.2264149,0.2264149,0.2264149,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;90;2240.704,6112.361;Inherit;False;Property;_Alloy_03_Color;Alloy_03_Color;86;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.6980392,0.6980392,0.6980392,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;233;3122.62,7099.788;Half;False;Property;_Cosmetic_06_Gloss;Cosmetic_06_Gloss;75;0;Create;True;0;0;0;False;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;232;3142.156,7186.52;Half;False;Property;_Cosmetic_07_Gloss;Cosmetic_07_Gloss;78;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;236;3484.254,7082.602;Half;False;Property;_Cosmetic_06_Metal;Cosmetic_06_Metal;76;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;235;3483.314,7170.488;Half;False;Property;_Cosmetic_07_Metal;Cosmetic_07_Metal;79;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;213;2204.616,7709.833;Inherit;False;Property;_Alloy_04_Color;Alloy_04_Color;89;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.6980392,0.6980392,0.6980392,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;231;3155.543,7270.661;Half;False;Property;_Alloy_04_Gloss;Alloy_04_Gloss;90;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;234;3483.014,7264.12;Half;False;Property;_Alloy_04_Metal;Alloy_04_Metal;91;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;203;3480.753,6559.715;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;27;5873.529,6113.637;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;266;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;268;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;269;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;True;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;270;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;271;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;272;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;273;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalGBuffer;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;274;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;275;6203.236,5950.534;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.CustomStandardSurface;23;5149.367,6050.443;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;267;5873.637,6440.135;Float;False;True;-1;2;ASEMaterialInspector;0;12;RBG/Stock_Car_Trim;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;20;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;41;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Forward Only;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,False,;0;  Normal Distortion;0.5,False,;0;  Scattering;2,False,;0;  Direct;0.9,False,;0;  Ambient;0.1,False,;0;  Shadow;0.5,False,;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;True;True;True;True;True;False;;False;0
WireConnection;105;0;32;0
WireConnection;105;1;108;0
WireConnection;105;2;41;1
WireConnection;97;0;32;0
WireConnection;97;1;102;0
WireConnection;97;2;41;1
WireConnection;106;0;105;0
WireConnection;106;1;109;0
WireConnection;106;2;41;2
WireConnection;35;0;5;0
WireConnection;35;1;36;0
WireConnection;35;2;41;1
WireConnection;100;0;97;0
WireConnection;100;1;103;0
WireConnection;100;2;41;2
WireConnection;101;0;100;0
WireConnection;101;1;104;0
WireConnection;101;2;41;3
WireConnection;107;0;106;0
WireConnection;107;1;110;0
WireConnection;107;2;41;3
WireConnection;38;0;35;0
WireConnection;38;1;37;0
WireConnection;38;2;41;2
WireConnection;111;0;101;0
WireConnection;111;1;119;0
WireConnection;111;2;49;1
WireConnection;40;0;38;0
WireConnection;40;1;39;0
WireConnection;40;2;41;3
WireConnection;113;0;107;0
WireConnection;113;1;120;0
WireConnection;113;2;49;1
WireConnection;114;0;113;0
WireConnection;114;1;121;0
WireConnection;114;2;49;2
WireConnection;112;0;111;0
WireConnection;112;1;117;0
WireConnection;112;2;49;2
WireConnection;43;0;40;0
WireConnection;43;1;42;0
WireConnection;43;2;49;1
WireConnection;116;0;112;0
WireConnection;116;1;118;0
WireConnection;116;2;49;3
WireConnection;44;0;43;0
WireConnection;44;1;47;0
WireConnection;44;2;49;2
WireConnection;115;0;114;0
WireConnection;115;1;122;0
WireConnection;115;2;49;3
WireConnection;133;0;116;0
WireConnection;133;1;124;0
WireConnection;133;2;56;1
WireConnection;46;0;44;0
WireConnection;46;1;45;0
WireConnection;46;2;49;3
WireConnection;134;0;115;0
WireConnection;134;1;131;0
WireConnection;134;2;56;1
WireConnection;128;0;133;0
WireConnection;128;1;125;0
WireConnection;128;2;56;2
WireConnection;127;0;134;0
WireConnection;127;1;130;0
WireConnection;127;2;56;2
WireConnection;51;0;46;0
WireConnection;51;1;50;0
WireConnection;51;2;56;1
WireConnection;132;0;127;0
WireConnection;132;1;129;0
WireConnection;132;2;56;3
WireConnection;52;0;51;0
WireConnection;52;1;55;0
WireConnection;52;2;56;2
WireConnection;126;0;128;0
WireConnection;126;1;123;0
WireConnection;126;2;56;3
WireConnection;139;0;132;0
WireConnection;139;1;143;0
WireConnection;139;2;63;1
WireConnection;54;0;52;0
WireConnection;54;1;53;0
WireConnection;54;2;56;3
WireConnection;138;0;126;0
WireConnection;138;1;142;0
WireConnection;138;2;63;1
WireConnection;135;0;139;0
WireConnection;135;1;144;0
WireConnection;135;2;63;2
WireConnection;58;0;54;0
WireConnection;58;1;57;0
WireConnection;58;2;63;1
WireConnection;136;0;138;0
WireConnection;136;1;141;0
WireConnection;136;2;63;2
WireConnection;146;0;136;0
WireConnection;146;1;140;0
WireConnection;146;2;63;3
WireConnection;137;0;135;0
WireConnection;137;1;145;0
WireConnection;137;2;63;3
WireConnection;59;0;58;0
WireConnection;59;1;62;0
WireConnection;59;2;63;2
WireConnection;147;0;146;0
WireConnection;147;1;156;0
WireConnection;147;2;70;1
WireConnection;152;0;137;0
WireConnection;152;1;148;0
WireConnection;152;2;70;1
WireConnection;61;0;59;0
WireConnection;61;1;60;0
WireConnection;61;2;63;3
WireConnection;65;0;61;0
WireConnection;65;1;64;0
WireConnection;65;2;70;1
WireConnection;149;0;152;0
WireConnection;149;1;158;0
WireConnection;149;2;70;2
WireConnection;151;0;147;0
WireConnection;151;1;150;0
WireConnection;151;2;70;2
WireConnection;153;0;151;0
WireConnection;153;1;157;0
WireConnection;153;2;70;3
WireConnection;66;0;65;0
WireConnection;66;1;69;0
WireConnection;66;2;70;2
WireConnection;154;0;149;0
WireConnection;154;1;155;0
WireConnection;154;2;70;3
WireConnection;161;0;153;0
WireConnection;161;1;165;0
WireConnection;161;2;77;1
WireConnection;68;0;66;0
WireConnection;68;1;67;0
WireConnection;68;2;70;3
WireConnection;166;0;154;0
WireConnection;166;1;159;0
WireConnection;166;2;77;1
WireConnection;162;0;161;0
WireConnection;162;1;168;0
WireConnection;162;2;77;2
WireConnection;72;0;68;0
WireConnection;72;1;71;0
WireConnection;72;2;77;1
WireConnection;160;0;166;0
WireConnection;160;1;169;0
WireConnection;160;2;77;2
WireConnection;163;0;162;0
WireConnection;163;1;167;0
WireConnection;163;2;77;3
WireConnection;73;0;72;0
WireConnection;73;1;76;0
WireConnection;73;2;77;2
WireConnection;164;0;160;0
WireConnection;164;1;170;0
WireConnection;164;2;77;3
WireConnection;75;0;73;0
WireConnection;75;1;74;0
WireConnection;75;2;77;3
WireConnection;26;0;218;0
WireConnection;26;1;201;0
WireConnection;201;0;8;0
WireConnection;201;1;8;0
WireConnection;176;0;164;0
WireConnection;176;1;177;0
WireConnection;176;2;81;1
WireConnection;172;0;163;0
WireConnection;172;1;182;0
WireConnection;172;2;81;1
WireConnection;171;0;176;0
WireConnection;171;1;179;0
WireConnection;171;2;81;2
WireConnection;78;0;75;0
WireConnection;78;1;82;0
WireConnection;78;2;81;1
WireConnection;173;0;172;0
WireConnection;173;1;181;0
WireConnection;173;2;81;2
WireConnection;174;0;173;0
WireConnection;174;1;180;0
WireConnection;174;2;81;3
WireConnection;175;0;171;0
WireConnection;175;1;178;0
WireConnection;175;2;81;3
WireConnection;79;0;78;0
WireConnection;79;1;83;0
WireConnection;79;2;81;2
WireConnection;188;0;174;0
WireConnection;188;1;187;0
WireConnection;188;2;86;1
WireConnection;193;0;175;0
WireConnection;193;1;186;0
WireConnection;193;2;86;1
WireConnection;80;0;79;0
WireConnection;80;1;84;0
WireConnection;80;2;81;3
WireConnection;191;0;193;0
WireConnection;191;1;183;0
WireConnection;191;2;86;2
WireConnection;87;0;80;0
WireConnection;87;1;85;0
WireConnection;87;2;86;1
WireConnection;190;0;188;0
WireConnection;190;1;184;0
WireConnection;190;2;86;2
WireConnection;89;0;87;0
WireConnection;89;1;88;0
WireConnection;89;2;86;2
WireConnection;204;0;192;0
WireConnection;204;1;233;0
WireConnection;204;2;202;1
WireConnection;206;0;203;0
WireConnection;206;1;235;0
WireConnection;206;2;202;2
WireConnection;209;0;204;0
WireConnection;209;1;232;0
WireConnection;209;2;202;2
WireConnection;211;0;206;0
WireConnection;211;1;234;0
WireConnection;211;2;202;3
WireConnection;212;0;208;0
WireConnection;212;1;207;0
WireConnection;212;2;202;2
WireConnection;218;0;212;0
WireConnection;218;1;213;0
WireConnection;218;2;202;3
WireConnection;208;0;91;0
WireConnection;208;1;205;0
WireConnection;208;2;202;1
WireConnection;210;0;209;0
WireConnection;210;1;231;0
WireConnection;210;2;202;3
WireConnection;189;0;191;0
WireConnection;189;1;185;0
WireConnection;189;2;86;3
WireConnection;192;0;190;0
WireConnection;192;1;194;0
WireConnection;192;2;86;3
WireConnection;91;0;89;0
WireConnection;91;1;90;0
WireConnection;91;2;86;3
WireConnection;203;0;189;0
WireConnection;203;1;236;0
WireConnection;203;2;202;1
WireConnection;27;0;26;0
WireConnection;267;0;27;0
WireConnection;267;3;211;0
WireConnection;267;4;210;0
ASEEND*/
//CHKSM=564F57233CD803288DD7B86AC3977E37E8D5DF91