Shader "Custom/DoorWorldClipShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.3

        _ClipMin ("Clip Min", Vector) = (-1,-1,-1,0)
        _ClipMax ("Clip Max", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Color;
        half _Metallic;
        half _Smoothness;

        float4 _ClipMin;
        float4 _ClipMax;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 p = IN.worldPos;

            if (p.x < _ClipMin.x || p.x > _ClipMax.x ||
                p.y < _ClipMin.y || p.y > _ClipMax.y ||
                p.z < _ClipMin.z || p.z > _ClipMax.z)
            {
                clip(-1);
            }

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}