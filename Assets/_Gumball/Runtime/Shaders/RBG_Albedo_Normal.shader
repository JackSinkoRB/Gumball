// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/Albedo_Normal"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Albedo("Albedo", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_Normal("Normal", Float) = 1
		_Metal("Metal", Range( 0 , 1)) = 1
		_Gloss("Gloss", Range( 0 , 10)) = 3
		_Gloss_Exponent("Gloss_Exponent", Range( 0 , 4)) = 2.03
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
		uniform float4 _NormalMap_ST;
		uniform float _Normal;
		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _Metal;
		uniform float _Gloss;
		uniform half _Gloss_Exponent;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _Normal );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode5 = tex2D( _Albedo, uv_Albedo );
			o.Albedo = ( _Color * tex2DNode5 ).rgb;
			o.Metallic = _Metal;
			float clampResult17 = clamp( ( _Gloss * pow( tex2DNode5.r , _Gloss_Exponent ) ) , 0.0 , 0.95 );
			o.Smoothness = clampResult17;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;RBG/Albedo_Normal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;9;d3d11;glcore;gles;gles3;metal;vulkan;xboxone;ps4;ps5;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.ColorNode;2;-539.5,-456.5;Float;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-244.5,-252.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1000.172,136.5162;Float;False;Property;_Metal;Metal;4;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-345.757,437.731;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;0.95;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-757.6429,502.1529;Float;False;Property;_Gloss;Gloss;5;0;Create;True;0;0;0;False;0;False;3;2.4;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-964.757,308.731;Half;False;Property;_Gloss_Exponent;Gloss_Exponent;6;0;Create;True;0;0;0;False;0;False;2.03;0.52;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-1025.659,-71.73882;Inherit;True;Property;_NormalMap;NormalMap;2;0;Create;True;0;0;0;False;0;False;-1;None;499b9d5332d3c614989b3aec24b1280b;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-1212.8,-23.4;Float;False;Property;_Normal;Normal;3;0;Create;True;0;0;0;False;0;False;1;0.99;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1110.96,-327.0743;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;0;False;0;False;-1;None;d99be5b86b0f31a44a150798843d021f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;14;-654.757,229.231;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;17;-184.057,279.331;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-409.2569,211.531;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
WireConnection;0;0;6;0
WireConnection;0;1;7;0
WireConnection;0;3;12;0
WireConnection;0;4;17;0
WireConnection;6;0;2;0
WireConnection;6;1;5;0
WireConnection;7;5;9;0
WireConnection;14;0;5;1
WireConnection;14;1;15;0
WireConnection;17;0;16;0
WireConnection;17;2;18;0
WireConnection;16;0;13;0
WireConnection;16;1;14;0
ASEEND*/
//CHKSM=CC031B87811177DF2B7ED5ADB8623156A3002C2A