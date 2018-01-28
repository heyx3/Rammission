// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Water/My River" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _Normal1 ("Normal 1", 2D) = "white" {}
        _Normal2 ("Normal 2", 2D) = "white" {}

        _Normal1PanScale ("Normal 1 Pan and Scale", Vector) = (0,0,1,1)
        _Normal2PanScale ("Normal 2 Pan and Scale", Vector) = (0,0,1,1)
        
        _FoamTex ("Foam", 2D) = "black" {}
        _FoamHDR ("Foam HDR", Float) = 1.0
        _FoamCol ("Foam Color", Color) = (1,1,1,1)
        _FoamScale ("Foam Scale", Float) = 1.0
        _FoamPan ("Foam Pan", Float) = 1.0

        _WaterFallStarts ("Waterfall starts", Vector) = (0.1,0.9,0,0)

        _RefractIndex ("Refraction Index", Float) = 1.0
        _Transparency ("Transparency", Range(0,1)) = 0.3
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue"="Transparent" }

        GrabPass {
            "_Render"
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        #include "UnityCG.cginc"

        #pragma target 4.0

        struct Input {
            INTERNAL_DATA
            float2 uv_FoamTex;
            float4 worldTangent;
            float3 worldNorm;
            float4 grabUV;
            float4 objPos;
        };

        float4 _Color, _FoamCol;
        sampler2D _Normal1, _Normal2, _Render, _FoamTex;
        float4 _Normal1PanScale, _Normal2PanScale;
        float _FoamPan, _FoamScale, _FoamHDR;
        float _RefractIndex, _Transparency, _Glossiness, _Metallic;
        float2 _WaterFallStarts;

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.objPos = v.vertex;
            o.worldNorm = normalize(mul(v.normal, unity_WorldToObject));
            o.worldTangent = float4(normalize(mul(unity_ObjectToWorld, v.tangent).xyz),
                                    v.tangent.w);
            float4 hpos = UnityObjectToClipPos(v.vertex);
            o.grabUV = ComputeGrabScreenPos(hpos);
        }
        void surf(Input IN, inout SurfaceOutputStandard o) {

            //Get the river's surface color.
            float3 foam = tex2D(_FoamTex,
                                float2(_FoamScale, 1.0) *
                                (IN.uv_FoamTex + float2(0.0 * _Time.y, 0.0))).rgb;
            foam *= _FoamCol * _FoamHDR;

            //See if we're in a waterfall.
            float waterfall1 = step(IN.uv_FoamTex.x, _WaterFallStarts.x),
                  waterfall2 = step(_WaterFallStarts.y, IN.uv_FoamTex.x);
            foam = max(waterfall1, waterfall2);
            o.Albedo = _Color.rgb + foam;
            

            //Compute world-space normal.
            float3 norm1 = UnpackNormal(tex2D(_Normal1,
                                              _Normal1PanScale.zw *
                                                  (IN.uv_FoamTex + (_Normal1PanScale.xy * _Time.y)))),
                   norm2 = UnpackNormal(tex2D(_Normal2,
                                              _Normal2PanScale.zw *
                                                  (IN.uv_FoamTex + (_Normal2PanScale.xy * _Time.y))));
            float3 tangentSpaceNormal = normalize(max(norm1, norm2));
            float3 tangent = IN.worldTangent.xyz,
                   bitangent = cross(IN.worldNorm, tangent) *
                               IN.worldTangent.w * unity_WorldTransformParams.w;
            o.Normal = float3(dot(float3(tangent.x, bitangent.x, IN.worldNorm.x),
                                  tangentSpaceNormal),
                              dot(float3(tangent.y, bitangent.y, IN.worldNorm.y),
                                  tangentSpaceNormal),
                              dot(float3(tangent.z, bitangent.z, IN.worldNorm.z),
                                  tangentSpaceNormal));


            //Blend the albedo with the refracted world behind it.
            float3 viewDir = normalize(WorldSpaceViewDir(IN.objPos));
            viewDir = normalize(refract(viewDir, o.Normal, _RefractIndex));
            viewDir = mul(UNITY_MATRIX_IT_MV, float4(viewDir, 0.0)).xyz;
            float4 grabUV = IN.grabUV + float4(viewDir * 0.1, 0.0);
            float3 worldRender = tex2Dproj(_Render, UNITY_PROJ_COORD(grabUV)).rgb;
            o.Albedo = lerp(o.Albedo, worldRender, _Transparency);

            //Set other surface properties.
            //Convert normal back to tangent-space.
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Normal = tangentSpaceNormal;
        }
        ENDCG
    }
	FallBack "Diffuse"
}
