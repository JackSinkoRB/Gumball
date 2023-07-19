// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/RBG_Car_Reflection"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Main_Color", Color) = (0,0,0,0)
		_OuterCol("Outer_Color", Color) = (1,1,1,0)
		_InnerCol("Inner_Color", Color) = (0,0,0,0)
		_SpecularCol("Specular_Color", Color) = (0.4245283,0.1381719,0.1381719,0)
		_Metal("Metal", Range( 0 , 1)) = 0
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_CoatStr("Coat Strength", Range( 0 , 15)) = 1
		_CoatGloss("Coat Gloss", Range( 0 , 0.98)) = 0.5
		_Transition("Transition", Range( 0 , 10)) = 4
		_SpecularPower("Specular_Power", Range( 0.04 , 4.9)) = 4
		_Reflection("Reflection", 2D) = "white" {}
		_ReflectionIntensity("Reflection Intensity", Float) = 1
		_ReflectionCamIntensity("ReflectionCamIntensity", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			float3 worldRefl;
			INTERNAL_DATA
			float2 uv2_texcoord2;
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _Reflection;
		uniform float _ReflectionCamIntensity;
		uniform float _ReflectionIntensity;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform float4 _OuterCol;
		uniform float4 _InnerCol;
		uniform float _Transition;
		uniform float4 _SpecularCol;
		uniform float _SpecularPower;
		uniform float _Metal;
		uniform float _Gloss;
		uniform float _CoatGloss;
		uniform float _CoatStr;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s31 = (SurfaceOutputStandard ) 0;
			float2 uv1_Albedo = i.uv2_texcoord2 * _Albedo_ST.xy + _Albedo_ST.zw;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult9 = dot( ase_worldNormal , ase_worldViewDir );
			float4 lerpResult4 = lerp( _OuterCol , _InnerCol , pow( saturate( dotResult9 ) , _Transition ));
			float4 temp_output_25_0 = ( _Color * lerpResult4 );
			float fresnelNdotV12 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode12 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV12, 5.0 ) );
			float temp_output_13_0 = pow( fresnelNode12 , ( 5.0 - _SpecularPower ) );
			float4 lerpResult26 = lerp( temp_output_25_0 , _SpecularCol , temp_output_13_0);
			s31.Albedo = ( tex2D( _Albedo, uv1_Albedo ) * lerpResult26 ).rgb;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			s31.Normal = ase_normWorldNormal;
			s31.Emission = float3( 0,0,0 );
			s31.Metallic = _Metal;
			s31.Smoothness = _Gloss;
			s31.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi31 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g31 = UnityGlossyEnvironmentSetup( s31.Smoothness, data.worldViewDir, s31.Normal, float3(0,0,0));
			gi31 = UnityGlobalIllumination( data, s31.Occlusion, s31.Normal, g31 );
			#endif

			float3 surfResult31 = LightingStandard ( s31, viewDir, gi31 ).rgb;
			surfResult31 += s31.Emission;

			#ifdef UNITY_PASS_FORWARDADD//31
			surfResult31 -= s31.Emission;
			#endif//31
			SurfaceOutputStandard s35 = (SurfaceOutputStandard ) 0;
			s35.Albedo = float4(1,1,1,1).rgb;
			s35.Normal = ase_normWorldNormal;
			s35.Emission = float3( 0,0,0 );
			s35.Metallic = 0.5;
			s35.Smoothness = _CoatGloss;
			s35.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi35 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g35 = UnityGlossyEnvironmentSetup( s35.Smoothness, data.worldViewDir, s35.Normal, float3(0,0,0));
			gi35 = UnityGlobalIllumination( data, s35.Occlusion, s35.Normal, g35 );
			#endif

			float3 surfResult35 = LightingStandard ( s35, viewDir, gi35 ).rgb;
			surfResult35 += s35.Emission;

			#ifdef UNITY_PASS_FORWARDADD//35
			surfResult35 -= s35.Emission;
			#endif//35
			float3 temp_cast_5 = (_CoatStr).xxx;
			float3 lerpResult32 = lerp( surfResult31 , pow( surfResult35 , temp_cast_5 ) , saturate( fresnelNode12 ));
			c.rgb = saturate( ( float4( lerpResult32 , 0.0 ) * ( i.vertexColor * i.vertexColor ) ) ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float3 ase_worldNormal = i.worldNormal;
			float4 screenColor59 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,ase_worldNormal.xy);
			float3 ase_worldReflection = i.worldRefl;
			float4 lerpResult80 = lerp( screenColor59 , tex2D( _Reflection, ase_worldReflection.xy ) , _ReflectionCamIntensity);
			o.Emission = ( lerpResult80 * _ReflectionIntensity ).rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv2_texcoord2;
				o.customPack1.xy = v.texcoord1;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv2_texcoord2 = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.worldRefl = -worldViewDir;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1;-970.8838,578.4561;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-772.6243,409.4855;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;3;-1248.119,596.2166;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;4;-2504.674,-1550.03;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;5;-3029.482,-1895.926;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;6;-2835.071,-1631.825;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2718.164,1098.774;Float;False;Constant;_Fres_Scale;Fres_Scale;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-2717.433,1232.812;Float;False;Constant;_Fres_Power;Fres_Power;6;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;12;-2455.171,1065.856;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;13;-2207.094,-77.00192;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-2855.497,-1152.033;Inherit;False;Property;_OuterCol;Outer_Color;2;0;Create;False;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-1938.004,172.3035;Float;False;Property;_Metal;Metal;5;0;Create;True;0;0;0;False;0;False;0;0.108;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1916.583,267.0038;Float;False;Property;_Gloss;Gloss;6;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-3194.078,-1543.621;Inherit;False;Property;_Transition;Transition;9;0;Create;True;0;0;0;False;0;False;4;1.82;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2670.521,844.4451;Float;True;Constant;_Fres_Bias;Fres_Bias;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-2660.067,-161.6164;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;20;-2430.067,-119.6164;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;21;-3610.673,-1343.272;Float;False;Property;_InnerCol;Inner_Color;3;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.6698113,0.6698113,0.6698113,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;22;-2738.807,-384.8417;Inherit;False;Property;_SpecularCol;Specular_Color;4;0;Create;False;0;0;0;False;0;False;0.4245283,0.1381719,0.1381719,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;23;-470.8176,375.2959;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-1977.807,-512.0751;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-2084.782,-1335.3;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;26;-1711.466,-249.2943;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1296.848,-782.7033;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;28;-1734.857,-990.8837;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;29;-1825.645,777.861;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2746.799,-62.33839;Inherit;False;Property;_SpecularPower;Specular_Power;10;0;Create;False;0;0;0;False;0;False;4;2.47;0.04;4.9;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;31;-1332.114,-13.70096;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;32;-1061.718,300.0509;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-2990.229,566.1204;Float;False;Property;_CoatGloss;Coat Gloss;8;0;Create;False;0;0;0;False;0;False;0.5;0.638;0;0.98;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;34;-2977.851,214.4253;Float;False;Constant;_CoatAlbedo;CoatAlbedo;5;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomStandardSurface;35;-2574.161,404.0595;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-3008.551,451.38;Float;False;Constant;_CoatMetallic;CoatMetallic;6;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;37;-2043.655,485.6344;Inherit;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-2536.26,701.7632;Float;False;Property;_CoatStr;Coat Strength;7;0;Create;False;0;0;0;False;0;False;1;1.56;0;15;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;39;-2489.496,-1094.033;Float;False;Property;_Color;Main_Color;1;0;Create;False;0;0;0;False;0;False;0,0,0,0;1,0.1254902,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-402.2375,-81.20773;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-758.9112,-37.19311;Inherit;False;Property;_ReflectionIntensity;Reflection Intensity;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;7;-3728.588,-2210.242;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;8;-3722.617,-2005.244;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;9;-3354.583,-2126.915;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;59;-884.1982,-367.7059;Inherit;False;Global;_GrabScreen0;Grab Screen 0;15;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;55;-1083.275,-345.406;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;80;-557.3672,-276.7958;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;42;-825.8691,-624.0799;Inherit;True;Property;_Reflection;Reflection;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;-1089.565,-172.3041;Inherit;False;Property;_ReflectionCamIntensity;ReflectionCamIntensity;13;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;RBG/RBG_Car_Reflection;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.WorldReflectionVector;82;-1062.886,-641.7302;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
WireConnection;1;0;3;0
WireConnection;1;1;3;0
WireConnection;2;0;32;0
WireConnection;2;1;1;0
WireConnection;4;0;14;0
WireConnection;4;1;21;0
WireConnection;4;2;6;0
WireConnection;5;0;9;0
WireConnection;6;0;5;0
WireConnection;6;1;17;0
WireConnection;12;1;18;0
WireConnection;12;2;10;0
WireConnection;12;3;11;0
WireConnection;13;0;12;0
WireConnection;13;1;20;0
WireConnection;20;0;19;0
WireConnection;20;1;30;0
WireConnection;23;0;2;0
WireConnection;24;0;22;0
WireConnection;24;1;25;0
WireConnection;24;2;13;0
WireConnection;25;0;39;0
WireConnection;25;1;4;0
WireConnection;26;0;25;0
WireConnection;26;1;22;0
WireConnection;26;2;13;0
WireConnection;27;0;28;0
WireConnection;27;1;26;0
WireConnection;29;0;12;0
WireConnection;31;0;27;0
WireConnection;31;3;15;0
WireConnection;31;4;16;0
WireConnection;32;0;31;0
WireConnection;32;1;37;0
WireConnection;32;2;29;0
WireConnection;35;0;34;0
WireConnection;35;3;36;0
WireConnection;35;4;33;0
WireConnection;37;0;35;0
WireConnection;37;1;38;0
WireConnection;46;0;80;0
WireConnection;46;1;47;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;59;0;55;0
WireConnection;80;0;59;0
WireConnection;80;1;42;0
WireConnection;80;2;56;0
WireConnection;42;1;82;0
WireConnection;0;2;46;0
WireConnection;0;13;23;0
ASEEND*/
//CHKSM=E4F8679DDE6DDE466AE9A91317105F11A952C49D