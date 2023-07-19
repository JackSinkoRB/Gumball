// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/RBG_Tire"
{
	Properties
	{
		_Tire_Color("Tire_Color", Color) = (1,1,1,1)
		_Wall_Color("Wall_Color", Color) = (1,1,1,1)
		_GMAO("GMAO", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_Grip("Grip", Range( 0 , 1)) = 0.9529412
		_Sidewall("Sidewall", 2D) = "white" {}
		_Wall_Gloss("Wall_Gloss", Range( 0 , 1)) = 1
		_Wall_Metal("Wall_Metal", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
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
			float2 uv3_texcoord3;
		};

		uniform sampler2D _NormalMap;
		uniform sampler2D _GMAO;
		uniform float _Grip;
		uniform float4 _Tire_Color;
		uniform float4 _Wall_Color;
		uniform sampler2D _Sidewall;
		uniform float4 _Sidewall_ST;
		uniform float _Wall_Metal;
		uniform float _Wall_Gloss;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 tex2DNode10 = tex2D( _GMAO, i.uv_texcoord );
			float lerpResult22 = lerp( 1.0 , tex2DNode10.g , _Grip);
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, i.uv_texcoord ), lerpResult22 );
			float2 uv2_Sidewall = i.uv3_texcoord3 * _Sidewall_ST.xy + _Sidewall_ST.zw;
			float4 tex2DNode72 = tex2D( _Sidewall, uv2_Sidewall );
			float4 lerpResult73 = lerp( _Tire_Color , _Wall_Color , tex2DNode72.r);
			o.Albedo = lerpResult73.rgb;
			float lerpResult81 = lerp( 0.0 , _Wall_Metal , tex2DNode72.r);
			o.Metallic = lerpResult81;
			float lerpResult84 = lerp( 0.0 , _Wall_Gloss , tex2DNode72.r);
			float lerpResult32 = lerp( tex2DNode10.b , ( tex2DNode10.b + ( 1.0 - tex2DNode10.g ) ) , _Grip);
			float clampResult42 = clamp( lerpResult32 , 0.0 , 1.0 );
			float clampResult69 = clamp( lerpResult22 , 0.49 , 1.0 );
			float clampResult57 = clamp( ( lerpResult84 + ( ( clampResult42 * 0.6 ) * clampResult69 ) ) , 0.0 , 1.0 );
			o.Smoothness = clampResult57;
			o.Occlusion = clampResult42;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.SamplerNode;10;-1691.443,362.3224;Inherit;True;Property;_GMAO;GMAO;2;0;Create;True;0;0;0;False;0;False;-1;d287397b31c7ef74ca3ea010d5e8cc9b;d287397b31c7ef74ca3ea010d5e8cc9b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;70;-858.9409,440.5449;Inherit;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;0;False;0;False;0.49;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-1123.676,637.7302;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;34;-1344.237,664.901;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;32;-899.0676,562.682;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;42;-579.7684,538.8406;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-2112.941,288.0203;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-1350.634,-80.06092;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1762.692,-12.41327;Inherit;False;Property;_Grip;Grip;4;0;Create;True;0;0;0;False;0;False;0.9529412;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;73;-301.575,-715.4581;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-385.8499,354.3737;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-508.4448,-236.761;Inherit;False;Constant;_Tire_Metal;Tire_Metal;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;81;-157.4448,-342.761;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-588.0992,-175.6498;Inherit;True;Property;_NormalMap;NormalMap;3;0;Create;True;0;0;0;False;0;False;-1;fd478d8e7a6bbdb49bac231037d04c3f;fd478d8e7a6bbdb49bac231037d04c3f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;69;-635.9409,366.5449;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;22;-1051.211,113.1965;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-704.0317,125.1415;Inherit;True;Constant;_Gloss;Gloss;5;0;Create;True;0;0;0;False;0;False;0.6;0.29;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-195.9409,258.5449;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-479.1114,24.23894;Inherit;False;Constant;_Tire_Gloss;Tire_Gloss;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;57;179.6455,185.1249;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;85;-33.44482,177.239;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;84;-195.1114,-61.76105;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-542.4449,-331.761;Inherit;False;Property;_Wall_Metal;Wall_Metal;7;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-517.1115,118.239;Inherit;False;Property;_Wall_Gloss;Wall_Gloss;6;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;74;-1041.575,-610.4581;Float;False;Property;_Wall_Color;Wall_Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-1027.395,-400.0268;Float;False;Property;_Tire_Color;Tire_Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.05660371,0.05206475,0.05206475,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;420,-118;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;RBG/RBG_Tire;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;9;d3d11;glcore;gles;gles3;metal;vulkan;xboxone;ps4;ps5;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;72;-954.575,-845.4581;Inherit;True;Property;_Sidewall;Sidewall;5;0;Create;True;0;0;0;False;0;False;-1;None;005aa283a8f090546944d957f1231401;True;2;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;10;1;13;0
WireConnection;39;0;10;3
WireConnection;39;1;34;0
WireConnection;34;0;10;2
WireConnection;32;0;10;3
WireConnection;32;1;39;0
WireConnection;32;2;15;0
WireConnection;42;0;32;0
WireConnection;73;0;2;0
WireConnection;73;1;74;0
WireConnection;73;2;72;1
WireConnection;56;0;42;0
WireConnection;56;1;53;0
WireConnection;81;0;79;0
WireConnection;81;1;76;0
WireConnection;81;2;72;1
WireConnection;7;1;13;0
WireConnection;7;5;22;0
WireConnection;69;0;22;0
WireConnection;69;1;70;0
WireConnection;22;0;25;0
WireConnection;22;1;10;2
WireConnection;22;2;15;0
WireConnection;71;0;56;0
WireConnection;71;1;69;0
WireConnection;57;0;85;0
WireConnection;85;0;84;0
WireConnection;85;1;71;0
WireConnection;84;0;83;0
WireConnection;84;1;82;0
WireConnection;84;2;72;1
WireConnection;0;0;73;0
WireConnection;0;1;7;0
WireConnection;0;3;81;0
WireConnection;0;4;57;0
WireConnection;0;5;42;0
ASEEND*/
//CHKSM=A8F62273CCC7E7EC476102A56B53EDC36432D1BB