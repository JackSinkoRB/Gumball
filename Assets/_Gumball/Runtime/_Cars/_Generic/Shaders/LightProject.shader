// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "/Projector/LightProjector"
{
	Properties
	{
		_LightTex("LightTex", 2D) = "white" {}
		_FalloffTex("FalloffTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Power("Power", Range( 0 , 100)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#define ASE_USING_SAMPLING_MACROS 1
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#pragma surface surf Unlit alpha:fade keepalpha noshadow nolightmap  nodynlightmap nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float4 vertexToFrag27;
			float4 vertexToFrag34;
		};

		uniform float4 _Color;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_LightTex);
		float4x4 unity_Projector;
		SamplerState sampler_LightTex;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_FalloffTex);
		float4x4 unity_ProjectorClip;
		SamplerState sampler_FalloffTex;
		uniform float _Power;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 ase_vertex4Pos = v.vertex;
			o.vertexToFrag27 = mul( unity_Projector, ase_vertex4Pos );
			o.vertexToFrag34 = mul( unity_ProjectorClip, ase_vertex4Pos );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 tex2DNode35 = SAMPLE_TEXTURE2D( _LightTex, sampler_LightTex, ( (i.vertexToFrag27).xy / (i.vertexToFrag27).w ) );
			float4 appendResult43 = (float4(( float4( (_Color).rgb , 0.0 ) * tex2DNode35 ).rgb , ( 1.0 - tex2DNode35.a )));
			float4 temp_output_44_0 = ( appendResult43 * SAMPLE_TEXTURE2D( _FalloffTex, sampler_FalloffTex, ( (i.vertexToFrag34).xy / (i.vertexToFrag34).w ) ) );
			o.Emission = ( temp_output_44_0 * _Power ).xyz;
			o.Alpha = temp_output_44_0.x;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.PosVertexDataNode;23;-2002.586,10.70776;Inherit;False;1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;29;-1410.586,10.70776;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;32;-1170.586,-69.29223;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;33;-1202.586,-261.2923;Float;False;Property;_Color;Color;2;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.9811321,0.9811321,0.9811321,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;35;-1026.587,-69.29223;Inherit;True;Property;_LightTex;LightTex;0;0;Create;True;0;0;0;False;0;False;-1;None;20fa3557ce2e0ca4e8e20a2348fad081;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;36;-946.5865,-165.2922;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;39;-690.5865,26.70776;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;40;-1170.586,314.7076;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-690.5865,-85.29221;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;42;-1026.587,314.7076;Inherit;True;Property;_FalloffTex;FalloffTex;1;0;Create;True;0;0;0;False;0;False;-1;None;c270737393346e54c979f701ce812faa;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;43;-514.5865,-37.29222;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;30;-1410.586,-69.29223;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;38;-1410.586,314.7076;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;37;-1410.586,394.7076;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;34;-1650.586,314.7076;Inherit;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexToFragmentNode;27;-1650.586,-69.29223;Inherit;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1794.586,-69.29223;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1794.586,314.7076;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.UnityProjectorClipMatrixNode;26;-2002.586,314.7076;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.PosVertexDataNode;28;-2002.586,394.7076;Inherit;False;1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnityProjectorMatrixNode;24;-2002.586,-69.29223;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-334.9115,180.436;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-297.3556,417.6441;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-642.8083,493.4969;Inherit;False;Property;_Power;Power;3;0;Create;True;0;0;0;False;0;False;0;100;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;50;-9.03035,205.4403;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;/Projector/LightProjector;False;False;False;False;False;False;True;True;True;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;27;0
WireConnection;32;0;30;0
WireConnection;32;1;29;0
WireConnection;35;1;32;0
WireConnection;36;0;33;0
WireConnection;39;0;35;4
WireConnection;40;0;38;0
WireConnection;40;1;37;0
WireConnection;41;0;36;0
WireConnection;41;1;35;0
WireConnection;42;1;40;0
WireConnection;43;0;41;0
WireConnection;43;3;39;0
WireConnection;30;0;27;0
WireConnection;38;0;34;0
WireConnection;37;0;34;0
WireConnection;34;0;31;0
WireConnection;27;0;25;0
WireConnection;25;0;24;0
WireConnection;25;1;23;0
WireConnection;31;0;26;0
WireConnection;31;1;28;0
WireConnection;44;0;43;0
WireConnection;44;1;42;0
WireConnection;52;0;44;0
WireConnection;52;1;53;0
WireConnection;50;2;52;0
WireConnection;50;9;44;0
ASEEND*/
//CHKSM=CE8D5F071DBDDCD2D90B6D9D838058C0DAC7AAE3