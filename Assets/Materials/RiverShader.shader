// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Water/My River" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _Normal1 ("Normal 1", 2D) = "white" {}
        _Normal2 ("Normal 2", 2D) = "white" {}

        _Normal1Pan ("Normal 1 Pan", Vector) = (0,0,0,0)
        _Normal2Pan ("Normal 2 Pan", Vector) = (0,0,0,0)
        
        _FoamTex ("Foam", 2D) = "black" {}
        _FoamCol ("Foam Color", Color) = (1,1,1,1)
        _FoamPan ("Foam Pan", Float) = 1.0

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
    SubShader {
        Tags { "RenderType" = "Opaque" }

        GrabPass {
            "_Render"
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        #include "UnityCG.cginc"

        #pragma target 3.0

        struct Input {
            float2 uv_Normal1;
            float2 uv_Normal2;
            float2 uv_FoamTex;
            float4 grabUV;
        };

        float4 _Color, _FoamCol;
        sampler2D _Normal1, _Normal2, _Render, _FoamTex;
        float2 _Normal1Pan, _Normal2Pan;
        float _FoamPan;
        half _Glossiness;
        half _Metallic;

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float4 hpos = UnityObjectToClipPos(v.vertex);
            o.grabUV = ComputeGrabScreenPos(hpos);
        }
        void surf(Input IN, inout SurfaceOutputStandard o) {
            float3 foam = tex2D(_FoamTex, IN.uv_FoamTex + float2(_FoamPan * _Time.x, 0.0)).rgb;
            foam *= _FoamCol;
            o.Albedo = _Color.rgb + foam;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;

            //TODO: Refract.
            float4 worldRender = tex2Dproj(_Render, UNITY_PROJ_COORD(IN.grabUV));

            float3 norm1 = UnpackNormal(tex2D(_Normal1, IN.uv_Normal1 + (_Normal1Pan * _Time.x))),
                   norm2 = UnpackNormal(tex2D(_Normal2, IN.uv_Normal2 + (_Normal2Pan * _Time.x)));
            o.Normal = normalize(max(norm1, norm2));
        }
        ENDCG
    }
	FallBack "Diffuse"
}
