Shader "Custom/MultiTextures" {
	Properties {
		_ControlTex ("ControlTexture (RGBA)", 2D) = "black" {}
		_Tex0("Texture0 (RGB)", 2D) = "white" {}
		_Tex1("Texture1 (RGB)", 2D) = "white" {}
		_Tex2("Texture2 (RGB)", 2D) = "white" {}
		_Tex3("Texture3 (RGB)", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _ControlTex;
		sampler2D _Tex0;
		sampler2D _Tex1;
		sampler2D _Tex2;
		sampler2D _Tex3;

		struct Input {
			float2 uv_ControlTex;
			float2 uv_Tex0;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float4 control = tex2D (_ControlTex, IN.uv_ControlTex);
			float4 tex0 = tex2D(_Tex0, IN.uv_Tex0);
			float4 tex1 = tex2D(_Tex1, IN.uv_Tex0);
			float4 tex2 = tex2D(_Tex2, IN.uv_Tex0);
			float4 tex3 = tex2D(_Tex3, IN.uv_Tex0);

			float4 result = float4(0, 0, 0, 0);
			result += tex0 * control.a;
			result += tex1 * control.r;
			result += tex2 * control.g;
			result += tex3 * control.b;
			o.Albedo = result.rgb;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = result.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
