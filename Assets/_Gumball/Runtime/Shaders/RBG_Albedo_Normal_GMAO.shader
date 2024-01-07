// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/Albedo_Normal_GMAO"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Albedo("Albedo", 2D) = "white" {}
		_GMAO("GMAO", 2D) = "white" {}
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_NormalMap("NormalMap", 2D) = "bump" {}
		_Normal("Normal", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma exclude_renderers xboxseries playstation switch 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _Normal;
		uniform float4 _Color;
		uniform sampler2D _GMAO;
		uniform float _Gloss;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, uv_Albedo ), _Normal );
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Color ).rgb;
			float4 tex2DNode10 = tex2D( _GMAO, uv_Albedo );
			o.Metallic = tex2DNode10.g;
			o.Smoothness = ( tex2DNode10.r * _Gloss );
			o.Occlusion = tex2DNode10.b;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.TexturePropertyNode;14;-1379.201,-88.89999;Float;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;0;False;0;False;None;f84cc9c12257f6d49a75e2a16ba4d048;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ColorNode;2;-570.5,-192.5;Float;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-619.4993,-412.7999;Inherit;True;Property;_MainTexSample;MainTexSample;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;RBG/Albedo_Normal_GMAO;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;9;d3d11;glcore;gles;gles3;metal;vulkan;xboxone;ps4;ps5;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1308,250.8;Float;False;Property;_Normal;Normal;5;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-248.5,-257.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;7;-661.3995,19.79999;Inherit;True;Property;_NormalMap;NormalMap;4;0;Create;True;0;0;0;False;0;False;-1;None;be7f3d07bcdaecc43bd9a639e54d6a14;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1030.4,-1.500001;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-215.1001,424.1006;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-655.5994,518.0001;Float;False;Property;_Gloss;Gloss;3;0;Create;True;0;0;0;False;0;False;0;0.689;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-715.8464,216.6196;Inherit;True;Property;_GMAO;GMAO;2;0;Create;True;0;0;0;False;0;False;-1;None;e9937f6b30fb5c94288839164371fff1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;5;0;14;0
WireConnection;5;1;13;0
WireConnection;0;0;6;0
WireConnection;0;1;7;0
WireConnection;0;3;10;2
WireConnection;0;4;12;0
WireConnection;0;5;10;3
WireConnection;6;0;5;0
WireConnection;6;1;2;0
WireConnection;7;1;13;0
WireConnection;7;5;9;0
WireConnection;13;2;14;0
WireConnection;12;0;10;1
WireConnection;12;1;4;0
WireConnection;10;1;13;0
ASEEND*/
//CHKSM=5AAEC370AD007C393C6389F3D5CC30215FB52AB9