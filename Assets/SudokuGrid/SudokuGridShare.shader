Shader "Custom/SudokuGridShare"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        _MainLineThickness("Main Thickness", float) = 3
        _SecondaryThickness("Secondary Thickness", float) = 1

        _BackgroundColor("Background Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _BackgroundColor;
                float _MainLineThickness;
                float _SecondaryThickness;
            CBUFFER_END

            bool IsInRange(float value, float minVal, float maxVal)
            {
                return value >= minVal && value <= maxVal;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float mainThickness = _MainLineThickness / 100.0;
                float secThickness = _SecondaryThickness / 100.0;    

                if (uv.x < mainThickness || uv.y < mainThickness || uv.x > 1 - mainThickness ||uv.y > 1 - mainThickness) {
                    return _BaseColor;
                }

                float primary_area = 1 - mainThickness * 2;
                float cell_area = (1 - 4 * mainThickness) / 3;

                for (int i = 1; i < 3; i++)
                {
                    if(IsInRange(uv.x, i * primary_area / 3 + mainThickness / 2, i * primary_area / 3 + 3 * mainThickness / 2) ||
                        IsInRange(uv.y, i * primary_area / 3 + mainThickness / 2, i * primary_area / 3 + 3 * mainThickness / 2))
                    {
                        return _BaseColor;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    float current_cell_start = mainThickness + i * (cell_area + mainThickness);
                    
                    for (int j = 1; j < 3; j++)
                    {
                        float line_center = current_cell_start + j * (cell_area / 3);
                        if (IsInRange(uv.x, line_center - secThickness, line_center + secThickness) || IsInRange(uv.y, line_center - secThickness, line_center + secThickness))
                        {
                            return _BaseColor;
                        }
                    }
                }

                return _BackgroundColor;
            }
            ENDHLSL
        }
    }
}
