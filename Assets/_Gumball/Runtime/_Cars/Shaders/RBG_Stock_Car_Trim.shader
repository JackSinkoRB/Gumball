// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RBG/Stock_Car_Trim"
{
	Properties
	{
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
			half3 worldNormal;
			float2 uv2_texcoord2;
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

		uniform sampler2D _MainTex;
		uniform half4 _MainTex_ST;
		uniform half4 _Rollcage_Color;
		uniform sampler2D _Mask_01;
		uniform half4 _Mask_01_ST;
		uniform half4 _Tub_Color;
		uniform half4 _EngineBay_Color;
		uniform half4 _Wheel_01_Color;
		uniform sampler2D _Mask_02;
		uniform half4 _Mask_02_ST;
		uniform half4 _Wheel_02_Color;
		uniform half4 _Wheel_03_Color;
		uniform half4 _Seat_01_Color;
		uniform sampler2D _Mask_03;
		uniform half4 _Mask_03_ST;
		uniform half4 _Seat_02_Color;
		uniform half4 _Seat_03_Color;
		uniform half4 _E_Brake_01_Color;
		uniform sampler2D _Mask_04;
		uniform half4 _Mask_04_ST;
		uniform half4 _E_Brake_02_Color;
		uniform half4 _Shifter_01_Color;
		uniform half4 _Shifter_02_Color;
		uniform sampler2D _Mask_05;
		uniform half4 _Mask_05_ST;
		uniform half4 _Tacho_01_Color;
		uniform half4 _Tacho_02_Color;
		uniform half4 _Tacho_03_Color;
		uniform sampler2D _Mask_06;
		uniform half4 _Mask_06_ST;
		uniform half4 _Cosmetic_01_Color;
		uniform half4 _Cosmetic_02_Color;
		uniform half4 _Cosmetic_03_Color;
		uniform sampler2D _Mask_07;
		uniform half4 _Mask_07_ST;
		uniform half4 _Cosmetic_04_Color;
		uniform half4 _Cosmetic_05_Color;
		uniform half4 _Alloy_01_Color;
		uniform sampler2D _Mask_08;
		uniform half4 _Mask_08_ST;
		uniform half4 _Alloy_02_Color;
		uniform half4 _Alloy_03_Color;
		uniform half4 _Cosmetic_06_Color;
		uniform sampler2D _Mask_09;
		uniform half4 _Mask_09_ST;
		uniform half4 _Cosmetic_07_Color;
		uniform half4 _Alloy_04_Color;
		uniform sampler2D _Gloss_Metal;
		uniform half4 _Gloss_Metal_ST;
		uniform half _Rollcage_Metal;
		uniform half _Tub_Metal;
		uniform half _EngineBay_Metal;
		uniform half _Wheel_01_Metal;
		uniform half _Wheel_02_Metal;
		uniform half _Wheel_03_Metal;
		uniform half _Seat_01_Metal;
		uniform half _Seat_02_Metal;
		uniform half _Seat_03_Metal;
		uniform half _E_Brake_01_Metal;
		uniform half _E_Brake_02_Metal;
		uniform half _Shifter_01_Metal;
		uniform half _Shifter_02_Metal;
		uniform half _Tacho_01_Metal;
		uniform half _Tacho_02_Metal;
		uniform half _Tacho_03_Metal;
		uniform half _Cosmetic_01_Metal;
		uniform half _Cosmetic_02_Metal;
		uniform half _Cosmetic_03_Metal;
		uniform half _Cosmetic_04_Metal;
		uniform half _Cosmetic_05_Metal;
		uniform half _Alloy_01_Metal;
		uniform half _Alloy_02_Metal;
		uniform half _Alloy_03_Metal;
		uniform half _Cosmetic_06_Metal;
		uniform half _Cosmetic_07_Metal;
		uniform half _Alloy_04_Metal;
		uniform half _Rollcage_Gloss;
		uniform half _Tub_Gloss;
		uniform half _EngineBay_Gloss;
		uniform half _Wheel_01_Gloss;
		uniform half _Wheel_02_Gloss;
		uniform half _Wheel_03_Gloss;
		uniform half _Seat_01_Gloss;
		uniform half _Seat_02_Gloss;
		uniform half _Seat_03_Gloss;
		uniform half _E_Brake_01_Gloss;
		uniform half _E_Brake_02_Gloss;
		uniform half _Shifter_01_Gloss;
		uniform half _Shifter_02_Gloss;
		uniform half _Tacho_01_Gloss;
		uniform half _Tacho_02_Gloss;
		uniform half _Tacho_03_Gloss;
		uniform half _Cosmetic_01_Gloss;
		uniform half _Cosmetic_02_Gloss;
		uniform half _Cosmetic_03_Gloss;
		uniform half _Cosmetic_04_Gloss;
		uniform half _Cosmetic_05_Gloss;
		uniform half _Alloy_01_Gloss;
		uniform half _Alloy_02_Gloss;
		uniform half _Alloy_03_Gloss;
		uniform half _Cosmetic_06_Gloss;
		uniform half _Cosmetic_07_Gloss;
		uniform half _Alloy_04_Gloss;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s23 = (SurfaceOutputStandard ) 0;
			float2 uv1_MainTex = i.uv2_texcoord2 * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv1_Mask_01 = i.uv2_texcoord2 * _Mask_01_ST.xy + _Mask_01_ST.zw;
			half4 tex2DNode41 = tex2D( _Mask_01, uv1_Mask_01 );
			half4 lerpResult35 = lerp( tex2D( _MainTex, uv1_MainTex ) , _Rollcage_Color , tex2DNode41.r);
			half4 lerpResult38 = lerp( lerpResult35 , _Tub_Color , tex2DNode41.g);
			half4 lerpResult40 = lerp( lerpResult38 , _EngineBay_Color , tex2DNode41.b);
			float2 uv1_Mask_02 = i.uv2_texcoord2 * _Mask_02_ST.xy + _Mask_02_ST.zw;
			half4 tex2DNode49 = tex2D( _Mask_02, uv1_Mask_02 );
			half4 lerpResult43 = lerp( lerpResult40 , _Wheel_01_Color , tex2DNode49.r);
			half4 lerpResult44 = lerp( lerpResult43 , _Wheel_02_Color , tex2DNode49.g);
			half4 lerpResult46 = lerp( lerpResult44 , _Wheel_03_Color , tex2DNode49.b);
			float2 uv1_Mask_03 = i.uv2_texcoord2 * _Mask_03_ST.xy + _Mask_03_ST.zw;
			half4 tex2DNode56 = tex2D( _Mask_03, uv1_Mask_03 );
			half4 lerpResult51 = lerp( lerpResult46 , _Seat_01_Color , tex2DNode56.r);
			half4 lerpResult52 = lerp( lerpResult51 , _Seat_02_Color , tex2DNode56.g);
			half4 lerpResult54 = lerp( lerpResult52 , _Seat_03_Color , tex2DNode56.b);
			float2 uv1_Mask_04 = i.uv2_texcoord2 * _Mask_04_ST.xy + _Mask_04_ST.zw;
			half4 tex2DNode63 = tex2D( _Mask_04, uv1_Mask_04 );
			half4 lerpResult58 = lerp( lerpResult54 , _E_Brake_01_Color , tex2DNode63.r);
			half4 lerpResult59 = lerp( lerpResult58 , _E_Brake_02_Color , tex2DNode63.g);
			half4 lerpResult61 = lerp( lerpResult59 , _Shifter_01_Color , tex2DNode63.b);
			float2 uv1_Mask_05 = i.uv2_texcoord2 * _Mask_05_ST.xy + _Mask_05_ST.zw;
			half4 tex2DNode70 = tex2D( _Mask_05, uv1_Mask_05 );
			half4 lerpResult65 = lerp( lerpResult61 , _Shifter_02_Color , tex2DNode70.r);
			half4 lerpResult66 = lerp( lerpResult65 , _Tacho_01_Color , tex2DNode70.g);
			half4 lerpResult68 = lerp( lerpResult66 , _Tacho_02_Color , tex2DNode70.b);
			float2 uv1_Mask_06 = i.uv2_texcoord2 * _Mask_06_ST.xy + _Mask_06_ST.zw;
			half4 tex2DNode77 = tex2D( _Mask_06, uv1_Mask_06 );
			half4 lerpResult72 = lerp( lerpResult68 , _Tacho_03_Color , tex2DNode77.r);
			half4 lerpResult73 = lerp( lerpResult72 , _Cosmetic_01_Color , tex2DNode77.g);
			half4 lerpResult75 = lerp( lerpResult73 , _Cosmetic_02_Color , tex2DNode77.b);
			float2 uv1_Mask_07 = i.uv2_texcoord2 * _Mask_07_ST.xy + _Mask_07_ST.zw;
			half4 tex2DNode81 = tex2D( _Mask_07, uv1_Mask_07 );
			half4 lerpResult78 = lerp( lerpResult75 , _Cosmetic_03_Color , tex2DNode81.r);
			half4 lerpResult79 = lerp( lerpResult78 , _Cosmetic_04_Color , tex2DNode81.g);
			half4 lerpResult80 = lerp( lerpResult79 , _Cosmetic_05_Color , tex2DNode81.b);
			float2 uv1_Mask_08 = i.uv2_texcoord2 * _Mask_08_ST.xy + _Mask_08_ST.zw;
			half4 tex2DNode86 = tex2D( _Mask_08, uv1_Mask_08 );
			half4 lerpResult87 = lerp( lerpResult80 , _Alloy_01_Color , tex2DNode86.r);
			half4 lerpResult89 = lerp( lerpResult87 , _Alloy_02_Color , tex2DNode86.g);
			half4 lerpResult91 = lerp( lerpResult89 , _Alloy_03_Color , tex2DNode86.b);
			float2 uv1_Mask_09 = i.uv2_texcoord2 * _Mask_09_ST.xy + _Mask_09_ST.zw;
			half4 tex2DNode202 = tex2D( _Mask_09, uv1_Mask_09 );
			half4 lerpResult208 = lerp( lerpResult91 , _Cosmetic_06_Color , tex2DNode202.r);
			half4 lerpResult212 = lerp( lerpResult208 , _Cosmetic_07_Color , tex2DNode202.g);
			half4 lerpResult218 = lerp( lerpResult212 , _Alloy_04_Color , tex2DNode202.b);
			s23.Albedo = lerpResult218.rgb;
			half3 ase_worldNormal = i.worldNormal;
			half3 ase_normWorldNormal = normalize( ase_worldNormal );
			s23.Normal = ase_normWorldNormal;
			s23.Emission = float3( 0,0,0 );
			float2 uv1_Gloss_Metal = i.uv2_texcoord2 * _Gloss_Metal_ST.xy + _Gloss_Metal_ST.zw;
			half4 tex2DNode32 = tex2D( _Gloss_Metal, uv1_Gloss_Metal );
			half4 temp_cast_1 = (_Rollcage_Metal).xxxx;
			half4 lerpResult105 = lerp( tex2DNode32 , temp_cast_1 , tex2DNode41.r);
			half4 temp_cast_2 = (_Tub_Metal).xxxx;
			half4 lerpResult106 = lerp( lerpResult105 , temp_cast_2 , tex2DNode41.g);
			half4 temp_cast_3 = (_EngineBay_Metal).xxxx;
			half4 lerpResult107 = lerp( lerpResult106 , temp_cast_3 , tex2DNode41.b);
			half4 temp_cast_4 = (_Wheel_01_Metal).xxxx;
			half4 lerpResult113 = lerp( lerpResult107 , temp_cast_4 , tex2DNode49.r);
			half4 temp_cast_5 = (_Wheel_02_Metal).xxxx;
			half4 lerpResult114 = lerp( lerpResult113 , temp_cast_5 , tex2DNode49.g);
			half4 temp_cast_6 = (_Wheel_03_Metal).xxxx;
			half4 lerpResult115 = lerp( lerpResult114 , temp_cast_6 , tex2DNode49.b);
			half4 temp_cast_7 = (_Seat_01_Metal).xxxx;
			half4 lerpResult134 = lerp( lerpResult115 , temp_cast_7 , tex2DNode56.r);
			half4 temp_cast_8 = (_Seat_02_Metal).xxxx;
			half4 lerpResult127 = lerp( lerpResult134 , temp_cast_8 , tex2DNode56.g);
			half4 temp_cast_9 = (_Seat_03_Metal).xxxx;
			half4 lerpResult132 = lerp( lerpResult127 , temp_cast_9 , tex2DNode56.b);
			half4 temp_cast_10 = (_E_Brake_01_Metal).xxxx;
			half4 lerpResult139 = lerp( lerpResult132 , temp_cast_10 , tex2DNode63.r);
			half4 temp_cast_11 = (_E_Brake_02_Metal).xxxx;
			half4 lerpResult135 = lerp( lerpResult139 , temp_cast_11 , tex2DNode63.g);
			half4 temp_cast_12 = (_Shifter_01_Metal).xxxx;
			half4 lerpResult137 = lerp( lerpResult135 , temp_cast_12 , tex2DNode63.b);
			half4 temp_cast_13 = (_Shifter_02_Metal).xxxx;
			half4 lerpResult152 = lerp( lerpResult137 , temp_cast_13 , tex2DNode70.r);
			half4 temp_cast_14 = (_Tacho_01_Metal).xxxx;
			half4 lerpResult149 = lerp( lerpResult152 , temp_cast_14 , tex2DNode70.g);
			half4 temp_cast_15 = (_Tacho_02_Metal).xxxx;
			half4 lerpResult154 = lerp( lerpResult149 , temp_cast_15 , tex2DNode70.b);
			half4 temp_cast_16 = (_Tacho_03_Metal).xxxx;
			half4 lerpResult166 = lerp( lerpResult154 , temp_cast_16 , tex2DNode77.r);
			half4 temp_cast_17 = (_Cosmetic_01_Metal).xxxx;
			half4 lerpResult160 = lerp( lerpResult166 , temp_cast_17 , tex2DNode77.g);
			half4 temp_cast_18 = (_Cosmetic_02_Metal).xxxx;
			half4 lerpResult164 = lerp( lerpResult160 , temp_cast_18 , tex2DNode77.b);
			half4 temp_cast_19 = (_Cosmetic_03_Metal).xxxx;
			half4 lerpResult176 = lerp( lerpResult164 , temp_cast_19 , tex2DNode81.r);
			half4 temp_cast_20 = (_Cosmetic_04_Metal).xxxx;
			half4 lerpResult171 = lerp( lerpResult176 , temp_cast_20 , tex2DNode81.g);
			half4 temp_cast_21 = (_Cosmetic_05_Metal).xxxx;
			half4 lerpResult175 = lerp( lerpResult171 , temp_cast_21 , tex2DNode81.b);
			half4 temp_cast_22 = (_Alloy_01_Metal).xxxx;
			half4 lerpResult193 = lerp( lerpResult175 , temp_cast_22 , tex2DNode86.r);
			half4 temp_cast_23 = (_Alloy_02_Metal).xxxx;
			half4 lerpResult191 = lerp( lerpResult193 , temp_cast_23 , tex2DNode86.g);
			half4 temp_cast_24 = (_Alloy_03_Metal).xxxx;
			half4 lerpResult189 = lerp( lerpResult191 , temp_cast_24 , tex2DNode86.b);
			half4 temp_cast_25 = (_Cosmetic_06_Metal).xxxx;
			half4 lerpResult203 = lerp( lerpResult189 , temp_cast_25 , tex2DNode202.r);
			half4 temp_cast_26 = (_Cosmetic_07_Metal).xxxx;
			half4 lerpResult206 = lerp( lerpResult203 , temp_cast_26 , tex2DNode202.g);
			half4 temp_cast_27 = (_Alloy_04_Metal).xxxx;
			half4 lerpResult211 = lerp( lerpResult206 , temp_cast_27 , tex2DNode202.b);
			s23.Metallic = lerpResult211.r;
			half4 temp_cast_29 = (_Rollcage_Gloss).xxxx;
			half4 lerpResult97 = lerp( tex2DNode32 , temp_cast_29 , tex2DNode41.r);
			half4 temp_cast_30 = (_Tub_Gloss).xxxx;
			half4 lerpResult100 = lerp( lerpResult97 , temp_cast_30 , tex2DNode41.g);
			half4 temp_cast_31 = (_EngineBay_Gloss).xxxx;
			half4 lerpResult101 = lerp( lerpResult100 , temp_cast_31 , tex2DNode41.b);
			half4 temp_cast_32 = (_Wheel_01_Gloss).xxxx;
			half4 lerpResult111 = lerp( lerpResult101 , temp_cast_32 , tex2DNode49.r);
			half4 temp_cast_33 = (_Wheel_02_Gloss).xxxx;
			half4 lerpResult112 = lerp( lerpResult111 , temp_cast_33 , tex2DNode49.g);
			half4 temp_cast_34 = (_Wheel_03_Gloss).xxxx;
			half4 lerpResult116 = lerp( lerpResult112 , temp_cast_34 , tex2DNode49.b);
			half4 temp_cast_35 = (_Seat_01_Gloss).xxxx;
			half4 lerpResult133 = lerp( lerpResult116 , temp_cast_35 , tex2DNode56.r);
			half4 temp_cast_36 = (_Seat_02_Gloss).xxxx;
			half4 lerpResult128 = lerp( lerpResult133 , temp_cast_36 , tex2DNode56.g);
			half4 temp_cast_37 = (_Seat_03_Gloss).xxxx;
			half4 lerpResult126 = lerp( lerpResult128 , temp_cast_37 , tex2DNode56.b);
			half4 temp_cast_38 = (_E_Brake_01_Gloss).xxxx;
			half4 lerpResult138 = lerp( lerpResult126 , temp_cast_38 , tex2DNode63.r);
			half4 temp_cast_39 = (_E_Brake_02_Gloss).xxxx;
			half4 lerpResult136 = lerp( lerpResult138 , temp_cast_39 , tex2DNode63.g);
			half4 temp_cast_40 = (_Shifter_01_Gloss).xxxx;
			half4 lerpResult146 = lerp( lerpResult136 , temp_cast_40 , tex2DNode63.b);
			half4 temp_cast_41 = (_Shifter_02_Gloss).xxxx;
			half4 lerpResult147 = lerp( lerpResult146 , temp_cast_41 , tex2DNode70.r);
			half4 temp_cast_42 = (_Tacho_01_Gloss).xxxx;
			half4 lerpResult151 = lerp( lerpResult147 , temp_cast_42 , tex2DNode70.g);
			half4 temp_cast_43 = (_Tacho_02_Gloss).xxxx;
			half4 lerpResult153 = lerp( lerpResult151 , temp_cast_43 , tex2DNode70.b);
			half4 temp_cast_44 = (_Tacho_03_Gloss).xxxx;
			half4 lerpResult161 = lerp( lerpResult153 , temp_cast_44 , tex2DNode77.r);
			half4 temp_cast_45 = (_Cosmetic_01_Gloss).xxxx;
			half4 lerpResult162 = lerp( lerpResult161 , temp_cast_45 , tex2DNode77.g);
			half4 temp_cast_46 = (_Cosmetic_02_Gloss).xxxx;
			half4 lerpResult163 = lerp( lerpResult162 , temp_cast_46 , tex2DNode77.b);
			half4 temp_cast_47 = (_Cosmetic_03_Gloss).xxxx;
			half4 lerpResult172 = lerp( lerpResult163 , temp_cast_47 , tex2DNode81.r);
			half4 temp_cast_48 = (_Cosmetic_04_Gloss).xxxx;
			half4 lerpResult173 = lerp( lerpResult172 , temp_cast_48 , tex2DNode81.g);
			half4 temp_cast_49 = (_Cosmetic_05_Gloss).xxxx;
			half4 lerpResult174 = lerp( lerpResult173 , temp_cast_49 , tex2DNode81.b);
			half4 temp_cast_50 = (_Alloy_01_Gloss).xxxx;
			half4 lerpResult188 = lerp( lerpResult174 , temp_cast_50 , tex2DNode86.r);
			half4 temp_cast_51 = (_Alloy_02_Gloss).xxxx;
			half4 lerpResult190 = lerp( lerpResult188 , temp_cast_51 , tex2DNode86.g);
			half4 temp_cast_52 = (_Alloy_03_Gloss).xxxx;
			half4 lerpResult192 = lerp( lerpResult190 , temp_cast_52 , tex2DNode86.b);
			half4 temp_cast_53 = (_Cosmetic_06_Gloss).xxxx;
			half4 lerpResult204 = lerp( lerpResult192 , temp_cast_53 , tex2DNode202.r);
			half4 temp_cast_54 = (_Cosmetic_07_Gloss).xxxx;
			half4 lerpResult209 = lerp( lerpResult204 , temp_cast_54 , tex2DNode202.g);
			half4 temp_cast_55 = (_Alloy_04_Gloss).xxxx;
			half4 lerpResult210 = lerp( lerpResult209 , temp_cast_55 , tex2DNode202.b);
			s23.Smoothness = lerpResult210.r;
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
			c.rgb = saturate( ( half4( surfResult23 , 0.0 ) * ( i.vertexColor * i.vertexColor ) ) ).rgb;
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
Node;AmplifyShaderEditor.SamplerNode;41;-3252.042,-1290.175;Inherit;True;Property;_Mask_01;Mask_01;2;0;Create;True;0;0;0;False;0;False;-1;None;b5cb45ea470d9ce468d8d9c65c055beb;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;32;-2370.408,-1706.742;Inherit;True;Property;_Gloss_Metal;Gloss_Metal;1;0;Create;True;0;0;0;False;0;False;-1;None;bf8b7fa90b1e6dc49bd65f8cfb7c8c9e;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;105;-1799.123,-1428.358;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-3253.591,-1569.286;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;9b5564956d892794683dba5c8702a32c;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;36;-3106.441,-941.2389;Inherit;False;Property;_Rollcage_Color;Rollcage_Color;11;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;1,0.1601619,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;97;-2177.842,-1309.165;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;106;-1594.031,-1306.842;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;37;-3104.849,-753.3101;Inherit;False;Property;_Tub_Color;Tub_Color;14;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.3865249,0.7582074,0.8113207,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;35;-2717.714,-1194.13;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;100;-1974.701,-1241.961;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;101;-1773.477,-1122.092;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;39;-3092.529,-555.8329;Inherit;False;Property;_EngineBay_Color;EngineBay_Color;17;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.9339623,0.7505314,0.3568435,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;107;-1447.11,-1147.066;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;49;-2881.169,-131.7701;Inherit;True;Property;_Mask_02;Mask_02;3;0;Create;True;0;0;0;False;0;False;-1;None;9833f8504a5e23141bc10eae4b81d4bc;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;38;-2583.17,-930.2803;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;111;-1740.02,-389.6241;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;40;-2439.918,-614.7582;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;42;-2840.341,121.0546;Inherit;False;Property;_Wheel_01_Color;Wheel_01_Color;20;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.08490542,0.08370384,0.08370384,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;113;-1356.1,-508.8177;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;114;-1160.109,-387.3011;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;47;-2768.756,337.8041;Inherit;False;Property;_Wheel_02_Color;Wheel_02_Color;23;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.5873531,0.5972914,0.6320754,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;112;-1540.779,-322.4201;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;43;-2201.107,-167.7966;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;116;-1339.555,-202.5509;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;44;-2084.613,66.46158;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;45;-2756.435,535.2797;Inherit;False;Property;_Wheel_03_Color;Wheel_03_Color;26;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.9056604,0.7350701,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;115;-986.187,-181.5249;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;56;-1929.687,708.7496;Inherit;True;Property;_Mask_03;Mask_03;4;0;Create;True;0;0;0;False;0;False;-1;None;82fd5f23dd509b74bae8d4a223cc0954;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;133;-942.9877,430.7923;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;46;-1840.614,308.8588;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;50;-1869.136,931.0283;Inherit;False;Property;_Seat_01_Color;Seat_01_Color;29;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;134;-559.0684,311.5988;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;128;-743.7466,497.9963;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;127;-363.0765,433.1153;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;51;-1502.403,678.1057;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;55;-1867.544,1118.957;Inherit;False;Property;_Seat_02_Color;Seat_02_Color;32;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.0866411,0.09643104,0.1037735,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;63;-1224.766,1742.106;Inherit;True;Property;_Mask_04;Mask_04;5;0;Create;True;0;0;0;False;0;False;-1;None;d50ee9936a3ee1b47b73c5bac868e4cd;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;132;-189.1556,638.8912;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;52;-1320.008,889.0809;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;126;-549.0223,625.6653;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;53;-1855.224,1316.433;Inherit;False;Property;_Seat_03_Color;Seat_03_Color;35;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.5436988,0.5924624,0.7735849,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;139;360.4612,1413.684;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;54;-1190.653,1231.487;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;57;-1127.554,1977.75;Inherit;False;Property;_E_Brake_01_Color;E_Brake_01_Color;38;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0,0.4905624,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;138;-23.45788,1532.877;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;135;556.4535,1535.2;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;58;-675.6572,1666.251;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;136;175.7829,1600.081;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;62;-1125.962,2165.679;Inherit;False;Property;_E_Brake_02_Color;E_Brake_02_Color;41;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.8584906,0.1515216,0.1012365,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;146;370.5073,1727.75;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;137;730.3748,1740.976;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;70;-572.2147,2652.499;Inherit;True;Property;_Mask_05;Mask_05;6;0;Create;True;0;0;0;False;0;False;-1;None;8d73bff813ab3f84cb5564ec60202a25;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;59;-441.8222,1894.335;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;60;-1113.641,2363.155;Inherit;False;Property;_Shifter_01_Color;Shifter_01_Color;44;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.9433962,0.9433962,0.9433962,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;147;677.0058,2390.939;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;152;1060.924,2271.747;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;61;-197.8241,2136.734;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;64;-458.1416,2868.456;Inherit;False;Property;_Shifter_02_Color;Shifter_02_Color;47;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.7169812,0.6865433,0.6865433,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;69;-456.5497,3056.385;Inherit;False;Property;_Tacho_01_Color;Tacho_01_Color;50;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.1116494,0.1226408,0.1127634,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;65;-6.250392,2556.959;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;149;1256.917,2393.262;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;151;876.2466,2458.144;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-444.2296,3253.861;Inherit;False;Property;_Tacho_02_Color;Tacho_02_Color;53;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;1,0.3969272,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;153;1070.97,2585.813;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;66;227.5843,2785.041;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;77;172.8203,3617.764;Inherit;True;Property;_Mask_06;Mask_06;7;0;Create;True;0;0;0;False;0;False;-1;None;2ee11a0844393ae41b60afb8f2bd881d;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;154;1430.838,2599.039;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;161;1353.24,3282.186;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;68;471.5838,3027.44;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;166;1737.158,3162.994;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;71;289.8314,3833.538;Inherit;False;Property;_Tacho_03_Color;Tacho_03_Color;56;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0,0.2865818,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;162;1552.481,3349.391;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;76;291.4233,4021.469;Inherit;False;Property;_Cosmetic_01_Color;Cosmetic_01_Color;59;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.643868,0.8584906,0.6617246,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;72;741.728,3522.04;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;160;1933.151,3284.509;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;163;1782.861,3487.038;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;73;975.563,3750.124;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;164;2107.07,3490.286;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;74;303.7444,4218.948;Inherit;False;Property;_Cosmetic_02_Color;Cosmetic_02_Color;62;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;1,0.3725483,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;75;1318.548,4013.733;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;5563.51,6134.245;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;27;5879.379,6096.089;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;6203.236,5950.534;Half;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;RBG/Stock_Car_Trim;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;9;d3d11;glcore;gles;gles3;metal;vulkan;xboxone;ps4;ps5;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;167;1359.076,3814.017;Half;False;Property;_Cosmetic_02_Gloss;Cosmetic_02_Gloss;63;0;Create;True;0;0;0;False;0;False;0;0.6112931;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;168;1345.689,3729.877;Half;False;Property;_Cosmetic_01_Gloss;Cosmetic_01_Gloss;60;0;Create;True;0;0;0;False;0;False;0;0.734;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;1326.153,3643.144;Half;False;Property;_Tacho_03_Gloss;Tacho_03_Gloss;57;0;Create;True;0;0;0;False;0;False;0.7;0.719;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;170;1686.547,3807.476;Half;False;Property;_Cosmetic_02_Metal;Cosmetic_02_Metal;64;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;169;1686.846,3713.844;Half;False;Property;_Cosmetic_01_Metal;Cosmetic_01_Metal;61;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;159;1687.787,3625.959;Half;False;Property;_Tacho_03_Metal;Tacho_03_Metal;58;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;156;653.9184,2733.897;Half;False;Property;_Shifter_02_Gloss;Shifter_02_Gloss;48;0;Create;True;0;0;0;False;0;False;0.7;0.927;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;669.4548,2831.63;Half;False;Property;_Tacho_01_Gloss;Tacho_01_Gloss;51;0;Create;True;0;0;0;False;0;False;0;0.734;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;157;678.8418,2920.77;Half;False;Property;_Tacho_02_Gloss;Tacho_02_Gloss;54;0;Create;True;0;0;0;False;0;False;0;0.6112931;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;155;1011.313,2916.229;Half;False;Property;_Tacho_02_Metal;Tacho_02_Metal;55;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;1008.808,2822.597;Half;False;Property;_Tacho_01_Metal;Tacho_01_Metal;52;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;1014.147,2732.908;Half;False;Property;_Shifter_02_Metal;Shifter_02_Metal;49;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;-21.62188,2062.707;Half;False;Property;_Shifter_01_Gloss;Shifter_01_Gloss;45;0;Create;True;0;0;0;False;0;False;0;0.924;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-31.00891,1973.567;Half;False;Property;_E_Brake_02_Gloss;E_Brake_02_Gloss;42;0;Create;True;0;0;0;False;0;False;0;0.905;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-46.54531,1875.834;Half;False;Property;_E_Brake_01_Gloss;E_Brake_01_Gloss;39;0;Create;True;0;0;0;False;0;False;0.7;0.719;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;313.683,1876.649;Half;False;Property;_E_Brake_01_Metal;E_Brake_01_Metal;40;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;144;310.1481,1964.534;Half;False;Property;_E_Brake_02_Metal;E_Brake_02_Metal;43;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;145;312.0931,2051.944;Half;False;Property;_Shifter_01_Metal;Shifter_01_Metal;46;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;-941.1516,960.6219;Half;False;Property;_Seat_03_Gloss;Seat_03_Gloss;36;0;Create;True;0;0;0;False;0;False;0;0.883;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-950.5386,871.4818;Half;False;Property;_Seat_02_Gloss;Seat_02_Gloss;33;0;Create;True;0;0;0;False;0;False;0;0.662;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-966.0751,773.7488;Half;False;Property;_Seat_01_Gloss;Seat_01_Gloss;30;0;Create;True;0;0;0;False;0;False;0.7;0.364;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-607.4365,949.8586;Half;False;Property;_Seat_03_Metal;Seat_03_Metal;37;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-609.3814,862.4489;Half;False;Property;_Seat_02_Metal;Seat_02_Metal;34;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-606.8465,774.564;Half;False;Property;_Seat_01_Metal;Seat_01_Metal;31;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-1756.262,-41.19105;Half;False;Property;_Wheel_01_Gloss;Wheel_01_Gloss;21;0;Create;True;0;0;0;False;0;False;0.7;0.604;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-1747.57,51.06554;Half;False;Property;_Wheel_02_Gloss;Wheel_02_Gloss;24;0;Create;True;0;0;0;False;0;False;0;0.832;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-1738.183,140.2056;Half;False;Property;_Wheel_03_Gloss;Wheel_03_Gloss;27;0;Create;True;0;0;0;False;0;False;0;0.841;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;-1406.414,42.03258;Half;False;Property;_Wheel_02_Metal;Wheel_02_Metal;25;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-1404.469,129.4422;Half;False;Property;_Wheel_03_Metal;Wheel_03_Metal;28;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-1403.879,-45.85218;Half;False;Property;_Wheel_01_Metal;Wheel_01_Metal;22;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-2172.106,-779.3355;Half;False;Property;_EngineBay_Gloss;EngineBay_Gloss;18;0;Create;True;0;0;0;False;0;False;0;0.841;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-2181.493,-868.4754;Half;False;Property;_Tub_Gloss;Tub_Gloss;15;0;Create;True;0;0;0;False;0;False;0;0.734;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-2190.184,-959.3156;Half;False;Property;_Rollcage_Gloss;Rollcage_Gloss;12;0;Create;True;0;0;0;False;0;False;0.7;0.878;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1838.391,-790.0988;Half;False;Property;_EngineBay_Metal;EngineBay_Metal;19;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-1840.336,-877.5084;Half;False;Property;_Tub_Metal;Tub_Metal;16;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-1837.801,-965.3932;Half;False;Property;_Rollcage_Metal;Rollcage_Metal;13;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;8;5081.397,6350.143;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;5298.161,6363.495;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomStandardSurface;23;5149.367,6050.443;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;81;1256.305,4610.479;Inherit;True;Property;_Mask_07;Mask_07;8;0;Create;True;0;0;0;False;0;False;-1;None;1dccdf6af414b904bafbddda7a9f7e5b;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;176;2711.622,4132.639;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;172;2327.703,4251.831;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;82;1368.702,4837.424;Inherit;False;Property;_Cosmetic_03_Color;Cosmetic_03_Color;65;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0,1,0.875,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;171;2907.615,4254.154;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;83;1370.294,5025.356;Inherit;False;Property;_Cosmetic_04_Color;Cosmetic_04_Color;68;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.8867924,0.28643,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;78;1820.599,4525.925;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;173;2526.944,4319.036;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;174;2721.667,4446.706;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;175;3084.387,4439.963;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;79;2054.434,4754.01;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;84;1382.615,5222.835;Inherit;False;Property;_Cosmetic_05_Color;Cosmetic_05_Color;71;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.3960779,1,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;86;2149.081,5539.043;Inherit;True;Property;_Mask_08;Mask_08;9;0;Create;True;0;0;0;False;0;False;-1;None;18cdd54a1626bf846892954631c3708d;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;85;2236.152,5740.786;Inherit;False;Property;_Alloy_01_Color;Alloy_01_Color;80;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0.620194,0.6590203,0.7264151,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;188;2999.802,4971.406;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;193;3385.43,4871.003;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;80;2298.434,4996.409;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;191;3581.423,4992.518;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;88;2240.999,5923.28;Inherit;False;Property;_Alloy_02_Color;Alloy_02_Color;83;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.5506853,0.640868,0.9339623,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;87;2609.469,5439.491;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;190;3200.752,5057.4;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;89;2763.017,5766.649;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;185;3353.608,5682.891;Half;False;Property;_Alloy_03_Metal;Alloy_03_Metal;88;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;3353.908,5589.259;Half;False;Property;_Alloy_02_Metal;Alloy_02_Metal;85;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;186;3349.724,5503.087;Half;False;Property;_Alloy_01_Metal;Alloy_01_Metal;82;0;Create;True;0;0;0;False;0;False;0;0.2908154;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;2968.336,5652.114;Half;False;Property;_Alloy_02_Gloss;Alloy_02_Gloss;84;0;Create;True;0;0;0;False;0;False;0;0.765;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;2948.8,5565.382;Half;False;Property;_Alloy_01_Gloss;Alloy_01_Gloss;81;0;Create;True;0;0;0;False;0;False;0.7;0.719;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;180;2333.539,4783.663;Half;False;Property;_Cosmetic_05_Gloss;Cosmetic_05_Gloss;72;0;Create;True;0;0;0;False;0;False;0;0.6112931;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;181;2320.152,4699.522;Half;False;Property;_Cosmetic_04_Gloss;Cosmetic_04_Gloss;69;0;Create;True;0;0;0;False;0;False;0;0.734;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;182;2300.616,4612.79;Half;False;Property;_Cosmetic_03_Gloss;Cosmetic_03_Gloss;66;0;Create;True;0;0;0;False;0;False;0.7;0.719;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;2661.01,4777.122;Half;False;Property;_Cosmetic_05_Metal;Cosmetic_05_Metal;73;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;2661.31,4683.49;Half;False;Property;_Cosmetic_04_Metal;Cosmetic_04_Metal;70;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;177;2662.25,4595.604;Half;False;Property;_Cosmetic_03_Metal;Cosmetic_03_Metal;67;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;194;2981.723,5737.255;Half;False;Property;_Alloy_03_Gloss;Alloy_03_Gloss;87;0;Create;False;0;0;0;False;0;False;0;0.49;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;203;3533.626,6619.637;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
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
Node;AmplifyShaderEditor.SamplerNode;202;2105.56,7097.477;Inherit;True;Property;_Mask_09;Mask_09;10;0;Create;True;0;0;0;False;0;False;-1;None;1dccdf6af414b904bafbddda7a9f7e5b;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;205;2190.704,7324.422;Inherit;False;Property;_Cosmetic_06_Color;Cosmetic_06_Color;74;0;Create;True;0;0;0;False;0;False;0.4622642,0.4622642,0.4622642,0;0,1,0.875,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;207;2192.295,7512.354;Inherit;False;Property;_Cosmetic_07_Color;Cosmetic_07_Color;77;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0.8867924,0.28643,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;90;2240.704,6112.361;Inherit;False;Property;_Alloy_03_Color;Alloy_03_Color;86;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.4245282,0.3864808,0.3904441,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;233;3122.62,7099.788;Half;False;Property;_Cosmetic_06_Gloss;Cosmetic_06_Gloss;75;0;Create;True;0;0;0;False;0;False;0.7;0.719;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;232;3142.156,7186.52;Half;False;Property;_Cosmetic_07_Gloss;Cosmetic_07_Gloss;78;0;Create;True;0;0;0;False;0;False;0;0.734;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;236;3484.254,7082.602;Half;False;Property;_Cosmetic_06_Metal;Cosmetic_06_Metal;76;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;235;3483.314,7170.488;Half;False;Property;_Cosmetic_07_Metal;Cosmetic_07_Metal;79;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;213;2204.616,7709.833;Inherit;False;Property;_Alloy_04_Color;Alloy_04_Color;89;0;Create;True;0;0;0;False;0;False;0.2735849,0.2735849,0.2735849,0;0.3960786,1,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;231;3155.543,7270.661;Half;False;Property;_Alloy_04_Gloss;Alloy_04_Gloss;90;0;Create;True;0;0;0;False;0;False;0;0.6112931;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;234;3483.014,7264.12;Half;False;Property;_Alloy_04_Metal;Alloy_04_Metal;91;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
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
WireConnection;26;0;23;0
WireConnection;26;1;201;0
WireConnection;27;0;26;0
WireConnection;0;13;27;0
WireConnection;201;0;8;0
WireConnection;201;1;8;0
WireConnection;23;0;218;0
WireConnection;23;3;211;0
WireConnection;23;4;210;0
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
WireConnection;203;0;189;0
WireConnection;203;1;236;0
WireConnection;203;2;202;1
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
ASEEND*/
//CHKSM=B6717A838EB33A1C43C09D87FD457D0D85634D18