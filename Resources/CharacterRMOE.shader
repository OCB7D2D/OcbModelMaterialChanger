Shader "OCB/Character/RMOE"
{
    Properties
    {
        _Albedo("Albedo (RGB)", 2D) = "white" {}
        _Normal("Normal Map", 2D) = "bump" {}
        _RMOE("RMOE Map", 2D) = "black" {}
        _Irradiated("Irradiated", Range(0, 1)) = 0
        _EmissiveColor("Emissive Color", Color) = (0,0,0,0)
        _IrradiatedColor("Irradiated Color", Color) = (0.3,1,0,1)
        _Color("Albedo Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _Albedo;
        sampler2D _RMOE;
        sampler2D _Normal;

        struct Input
        {
            float2 uv_Albedo;
            float2 uv_RMOE;
            float2 uv_Normal;
        };

        fixed _Irradiated;
        fixed4 _EmissiveColor;
        fixed4 _IrradiatedColor;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_Albedo, IN.uv_Albedo) * _Color;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            // Get the RMOE map and distribute channels
            fixed4 rmoe = tex2D(_RMOE, IN.uv_RMOE);
            // Not 100% sure if this is the correct setup?
            o.Emission = rmoe.a * lerp(_EmissiveColor, _IrradiatedColor, _Irradiated);
            // Metallic is in green channel
            o.Metallic = rmoe.g;
            // We have roughness in red channel
            o.Smoothness = 1 - rmoe.r;
            // Occlusion is in blue channel
            o.Occlusion = rmoe.b;
        }
        ENDCG
    }
    FallBack "Lit"
}
