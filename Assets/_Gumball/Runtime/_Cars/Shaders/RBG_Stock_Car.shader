// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/Stock_Car"
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
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldNormal;
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
			SurfaceOutputStandard s23 = (SurfaceOutputStandard ) 0;
			float2 uv1_Albedo = i.uv2_texcoord2 * _Albedo_ST.xy + _Albedo_ST.zw;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult73 = dot( ase_worldNormal , ase_worldViewDir );
			float4 lerpResult74 = lerp( _OuterCol , _InnerCol , pow( saturate( dotResult73 ) , _Transition ));
			float4 temp_output_78_0 = ( _Color * lerpResult74 );
			float fresnelNdotV10 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode10 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV10, 5.0 ) );
			float temp_output_82_0 = pow( fresnelNode10 , ( 5.0 - _SpecularPower ) );
			float4 lerpResult81 = lerp( temp_output_78_0 , _SpecularCol , temp_output_82_0);
			s23.Albedo = ( tex2D( _Albedo, uv1_Albedo ) * lerpResult81 ).rgb;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			s23.Normal = ase_normWorldNormal;
			s23.Emission = float3( 0,0,0 );
			s23.Metallic = _Metal;
			s23.Smoothness = _Gloss;
			s23.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi23 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g23 = UnityGlossyEnvironmentSetup( s23.Smoothness, data.worldViewDir, s23.Normal, float3(0,0,0));
			gi23 = UnityGlobalIllumination( data, s23.Occlusion, s23.Normal, g23 );
			#endif

			float3 surfResult23 = LightingStandard ( s23, viewDir, gi23 ).rgb;
			surfResult23 += s23.Emission;

			#ifdef UNITY_PASS_FORWARDADD//23
			surfResult23 -= s23.Emission;
			#endif//23
			SurfaceOutputStandard s11 = (SurfaceOutputStandard ) 0;
			s11.Albedo = float4(1,1,1,1).rgb;
			s11.Normal = ase_normWorldNormal;
			s11.Emission = float3( 0,0,0 );
			s11.Metallic = 0.5;
			s11.Smoothness = _CoatGloss;
			s11.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi11 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g11 = UnityGlossyEnvironmentSetup( s11.Smoothness, data.worldViewDir, s11.Normal, float3(0,0,0));
			gi11 = UnityGlobalIllumination( data, s11.Occlusion, s11.Normal, g11 );
			#endif

			float3 surfResult11 = LightingStandard ( s11, viewDir, gi11 ).rgb;
			surfResult11 += s11.Emission;

			#ifdef UNITY_PASS_FORWARDADD//11
			surfResult11 -= s11.Emission;
			#endif//11
			float3 temp_cast_2 = (_CoatStr).xxx;
			float3 lerpResult24 = lerp( surfResult23 , pow( surfResult11 , temp_cast_2 ) , saturate( fresnelNode10 ));
			c.rgb = saturate( ( float4( lerpResult24 , 0.0 ) * ( i.vertexColor * i.vertexColor ) ) ).rgb;
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
		}

		ENDCG
		CGPROGRAM
		#pragma exclude_renderers xboxseries playstation switch 
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
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
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
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv2_texcoord2;
				o.customPack1.xy = v.texcoord1;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;740.6038,542.3677;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;938.8633,373.3969;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;8;463.3692,560.1281;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;74;-793.1871,-1586.117;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;75;-1317.995,-1932.013;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;76;-1123.583,-1667.912;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;71;-2017.101,-2246.331;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;60;-2011.13,-2041.331;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;73;-1643.096,-2163.004;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1006.677,1062.687;Float;False;Constant;_Fres_Scale;Fres_Scale;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1005.946,1196.725;Float;False;Constant;_Fres_Power;Fres_Power;6;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;10;-743.684,1029.769;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;82;-495.6068,-113.0901;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;29;-1144.009,-1188.121;Inherit;False;Property;_OuterCol;Outer_Color;2;0;Create;False;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-226.5166,136.2151;Float;False;Property;_Metal;Metal;5;0;Create;True;0;0;0;False;0;False;0;0.108;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-205.0953,230.9152;Float;False;Property;_Gloss;Gloss;6;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-1482.591,-1579.708;Inherit;False;Property;_Transition;Transition;9;0;Create;True;0;0;0;False;0;False;4;1.82;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-959.0336,808.3571;Float;True;Constant;_Fres_Bias;Fres_Bias;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-948.5796,-197.7046;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;85;-718.5796,-155.7046;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-1899.185,-1379.359;Float;False;Property;_InnerCol;Inner_Color;3;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.6698113,0.6698113,0.6698113,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;79;-1027.32,-420.9301;Inherit;False;Property;_SpecularCol;Specular_Color;4;0;Create;False;0;0;0;False;0;False;0.4245283,0.1381719,0.1381719,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;27;1240.67,339.2073;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;80;-266.3192,-548.1636;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-373.2943,-1371.387;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;81;0.02168274,-285.3824;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;414.6394,-818.7917;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-23.3696,-1026.972;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;22;-114.158,741.7731;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-1035.312,-98.4266;Inherit;False;Property;_SpecularPower;Specular_Power;10;0;Create;False;0;0;0;False;0;False;4;2.47;0.04;4.9;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;23;379.3736,-49.78924;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;24;649.7698,263.9623;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1278.741,530.032;Float;False;Property;_CoatGloss;Coat Gloss;8;0;Create;False;0;0;0;False;0;False;0.5;0.638;0;0.98;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-1266.363,178.3369;Float;False;Constant;_CoatAlbedo;CoatAlbedo;5;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomStandardSurface;11;-862.6736,367.9709;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-1297.063,415.2914;Float;False;Constant;_CoatMetallic;CoatMetallic;6;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;18;-332.1679,449.546;Inherit;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-824.7728,665.6749;Float;False;Property;_CoatStr;Coat Strength;7;0;Create;False;0;0;0;False;0;False;1;1.56;0;15;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1489.083,11.08329;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;RBG/Stock_Car;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;9;d3d11;glcore;gles;gles3;metal;vulkan;xboxone;ps4;ps5;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.ColorNode;44;-778.009,-1130.121;Float;False;Property;_Color;Main_Color;1;0;Create;False;0;0;0;False;0;False;0,0,0,0;1,0.1254902,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;28;0;8;0
WireConnection;28;1;8;0
WireConnection;26;0;24;0
WireConnection;26;1;28;0
WireConnection;74;0;29;0
WireConnection;74;1;2;0
WireConnection;74;2;76;0
WireConnection;75;0;73;0
WireConnection;76;0;75;0
WireConnection;76;1;77;0
WireConnection;73;0;71;0
WireConnection;73;1;60;0
WireConnection;10;1;16;0
WireConnection;10;2;20;0
WireConnection;10;3;21;0
WireConnection;82;0;10;0
WireConnection;82;1;85;0
WireConnection;85;0;84;0
WireConnection;85;1;83;0
WireConnection;27;0;26;0
WireConnection;80;0;79;0
WireConnection;80;1;78;0
WireConnection;80;2;82;0
WireConnection;78;0;44;0
WireConnection;78;1;74;0
WireConnection;81;0;78;0
WireConnection;81;1;79;0
WireConnection;81;2;82;0
WireConnection;6;0;5;0
WireConnection;6;1;81;0
WireConnection;22;0;10;0
WireConnection;23;0;6;0
WireConnection;23;3;3;0
WireConnection;23;4;4;0
WireConnection;24;0;23;0
WireConnection;24;1;18;0
WireConnection;24;2;22;0
WireConnection;11;0;12;0
WireConnection;11;3;13;0
WireConnection;11;4;14;0
WireConnection;18;0;11;0
WireConnection;18;1;19;0
WireConnection;0;13;27;0
ASEEND*/
//CHKSM=8BF1D2CFA7C72971B4B52B36DD6F43AD5D5EA5B7