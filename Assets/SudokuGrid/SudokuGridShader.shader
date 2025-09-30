Shader "Custom/SudokuGridShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        _MainLineThickness("Main Thickness", float) = 3
        _SecondaryThickness("Secondary Thickness", float) = 1
        _ScalingFactor("Scaling Factor", float) = 1000

        _BackgroundColor("Background Color", Color) = (1, 1, 1, 1)
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
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
            
            #define CELL_COUNT 81
            float _HighlightedCells[CELL_COUNT];

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
                half4 _HighlightColor;
                float _MainLineThickness;
                float _SecondaryThickness;
                float _ScalingFactor;
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

                float bigThickness = _MainLineThickness / _ScalingFactor;
                float bigCellSize = (1 - 4 * bigThickness) / 3.0;

                for (int i = 0; i < 4; i++) // This is redundant
                {
                    float halfthick = bigThickness / 2.0;
                    float lineCenter = i * (bigCellSize + bigThickness) + halfthick;
                    if (IsInRange(uv.x, lineCenter - halfthick, lineCenter + halfthick) || IsInRange(uv.y, lineCenter - halfthick, lineCenter + halfthick))
                    {
                        return _BaseColor;
                    }
                }

                float smallThickness = _SecondaryThickness / _ScalingFactor;
                float halfthick = smallThickness / 2.0;
                float smallCellSize = (bigCellSize - 2 * smallThickness) / 3.0;

                for (int i = 0; i < 3; i++)
                {
                    float startPoint = bigThickness + i * (bigCellSize + bigThickness);

                    for (int j = 0; j < 3; j++)
                    {
                        float lineCenter = startPoint + (j + 1) * (smallCellSize + halfthick) + j * halfthick;
                        if (IsInRange(uv.x, lineCenter - halfthick, lineCenter + halfthick) || IsInRange(uv.y, lineCenter - halfthick, lineCenter + halfthick))
                        {
                            return _BaseColor;
                        }
                    }
                }

                
                float y_inv = 1 - uv.y;
                for (int i = 0; i < CELL_COUNT; i++)
                {
                    if (_HighlightedCells[i] == 0) {
                        continue;
                    }
                    
                    int row = i / 9;
                    int cellY = row % 3;                   
                    int groupRow = row / 3;
                    float groupY = groupRow * (bigCellSize + bigThickness) + bigThickness;
                    float startY = groupY + cellY * (smallCellSize + smallThickness);
                    float endY = startY + smallCellSize + smallThickness;
                    
                    int col = i % 9;
                    int cellX = col % 3;
                    int groupCol = col / 3;
                    float groupX = groupCol * (bigCellSize + bigThickness) + bigThickness;
                    float startX = groupX + cellX * (smallCellSize + smallThickness);
                    float endX = startX + smallCellSize + smallThickness;

                    if (IsInRange(uv.x, startX, endX) && IsInRange(y_inv, startY, endY)) {
                        return _HighlightColor;
                    }
                }

                return _BackgroundColor;
            }
            ENDHLSL
        }
    }
}
