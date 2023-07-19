// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/Albedo_Normal_GMAO_Headlights"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Albedo("Albedo", 2D) = "white" {}
		_GMAO("GMAO", 2D) = "white" {}
		_EmissionStrength("Emission Strength", Float) = 0
		_Emission("Emission", 2D) = "white" {}
		_BrakeLights("BrakeLights", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_Normal("Normal", Range( 0 , 1)) = 1
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_Grip("Grip", Range( 0 , 2)) = 1
		[Toggle]_Headlights("Headlights", Float) = 1
		[Toggle]_Brake("Brake", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _Normal;
		uniform float _Grip;
		uniform float4 _Color;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float _Headlights;
		uniform sampler2D _BrakeLights;
		uniform float4 _BrakeLights_ST;
		uniform float _Brake;
		uniform float _EmissionStrength;
		uniform sampler2D _GMAO;
		uniform float _Gloss;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, uv_Albedo ), ( _Normal * _Grip ) );
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Color ).rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float2 uv_BrakeLights = i.uv_texcoord * _BrakeLights_ST.xy + _BrakeLights_ST.zw;
			o.Emission = ( ( ( tex2D( _Emission, uv_Emission ) * (( _Headlights )?( 1.0 ):( 0.0 )) ) + ( tex2D( _BrakeLights, uv_BrakeLights ) * (( _Brake )?( 1.0 ):( 0.0 )) ) ) * _EmissionStrength ).rgb;
			float4 tex2DNode6 = tex2D( _GMAO, uv_Albedo );
			o.Metallic = tex2DNode6.g;
			o.Smoothness = ( tex2DNode6.r * _Gloss );
			o.Occlusion = ( tex2DNode6.b * _Grip );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1100.883,-121.4723;Float;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;0;False;0;False;None;85bc9613d5144524cb35e333a1795d7f;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-707.8816,-22.37229;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-292.1815,-225.0723;Float;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-341.1808,-445.3722;Inherit;True;Property;_MainTexSample;MainTexSample;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-342.7814,-17.9723;Inherit;True;Property;_NormalMap;NormalMap;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-321.0281,226.9473;Inherit;True;Property;_GMAO;GMAO;2;0;Create;True;0;0;0;False;0;False;-1;None;8bd9d3b7af2d9014099bf8d3657e4c58;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;61.21848,325.5279;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-308.4813,441.4278;Float;False;Property;_Gloss;Gloss;8;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1029.682,218.2277;Float;False;Property;_Normal;Normal;7;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-687.4911,293.7349;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1090.008,581.5214;Inherit;False;Property;_Grip;Grip;9;0;Create;True;0;0;0;False;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;82.50888,667.7349;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;83.49248,524.1212;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;29.81848,-290.0723;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;770.8257,-194.9915;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;RBG/Albedo_Normal_GMAO_Headlights;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;233.9458,-742.046;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;15;156.8686,-1064.777;Inherit;True;Property;_Emission;Emission;4;0;Create;True;0;0;0;False;0;False;-1;None;69d160a511367bd4ab21b972f114f150;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;-509.9692,-867.668;Inherit;True;Property;_BrakeLights;BrakeLights;5;0;Create;True;0;0;0;False;0;False;-1;None;f02cf2b1f46379a48ac29fd1d3077328;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-47.56496,-741.9012;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;21;-270.1978,-634.1141;Inherit;False;Property;_Brake;Brake;11;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;20;-23.0565,-895.9525;Inherit;False;Property;_Headlights;Headlights;10;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;121.4685,-501.9805;Inherit;False;Property;_EmissionStrength;Emission Strength;3;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;374.1341,-672.8552;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;515.4794,-467.7242;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
WireConnection;2;2;1;0
WireConnection;4;0;1;0
WireConnection;4;1;2;0
WireConnection;5;1;2;0
WireConnection;5;5;10;0
WireConnection;6;1;2;0
WireConnection;7;0;6;1
WireConnection;7;1;8;0
WireConnection;10;0;9;0
WireConnection;10;1;11;0
WireConnection;12;0;6;3
WireConnection;12;1;11;0
WireConnection;13;0;6;2
WireConnection;13;1;11;0
WireConnection;14;0;4;0
WireConnection;14;1;3;0
WireConnection;0;0;14;0
WireConnection;0;1;5;0
WireConnection;0;2;26;0
WireConnection;0;3;6;2
WireConnection;0;4;7;0
WireConnection;0;5;12;0
WireConnection;22;0;15;0
WireConnection;22;1;20;0
WireConnection;24;0;23;0
WireConnection;24;1;21;0
WireConnection;25;0;22;0
WireConnection;25;1;24;0
WireConnection;26;0;25;0
WireConnection;26;1;17;0
ASEEND*/
//CHKSM=2E7B9637923F5FA0F668D50498202F9C7A25EEA5