Shader "Unlit/CrossSection"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "grey" {}
        _PlanePosition ("Plane Position", Vector) = (0, 0, 0, 0)
        _PlaneNormal ("Plane Normal", Vector) = (0, 1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Include Unity helper macros
            #include "UnityCG.cginc"

            // Properties from the inspector
            sampler2D _MainTex;
            float4 _PlanePosition; // xyz is the position of the plane
            float4 _PlaneNormal;   // xyz is the normal of the plane

            struct appdata
            {
                float4 vertex : POSITION; // Vertex position
                float2 uv : TEXCOORD0;    // UV coordinates
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;       // UV coordinates passed to the fragment
                float4 vertex : SV_POSITION; // Vertex position in clip space
                float3 worldPos : TEXCOORD1; // Vertex position in world space
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Transform to clip space
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // Transform to world space
                o.uv = v.uv; // Pass UV coordinates
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate the distance of the fragment from the plane
                float distance = dot(_PlaneNormal.xyz, i.worldPos - _PlanePosition.xyz);

                // Discard fragments below the plane
                if (distance < 0.0)
                    discard;

                // Sample the texture and return the color
                fixed4 color = tex2D(_MainTex, i.uv);
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
