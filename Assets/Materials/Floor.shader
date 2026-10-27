Shader "Custom/ProceduralTiledFloorHLSL"
{
    Properties
    {
        [Header(Tiling and Layout)]
        _GridScale ("Grid Scale", Float) = 10.0
        _GroutWidth ("Grout Width", Range(0.001, 0.1)) = 0.02
        _BevelWidth ("Tile Bevel Width", Range(0.0, 0.2)) = 0.04

        [Header(Color and Appearance)]
        _TileColorA ("Primary Tile Color", Color) = (0.95, 0.95, 0.95, 1.0)
        _TileColorB ("Secondary Tile Color", Color) = (0.05, 0.05, 0.05, 1.0)
        _GroutColor ("Grout Color", Color) = (0.2, 0.2, 0.2, 1.0)

        [Header(Roughness and Metallic)]
        _TileRoughness ("Tile Roughness", Range(0.0, 1.0)) = 0.2
        _GroutRoughness ("Grout Roughness", Range(0.0, 1.0)) = 0.8
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0

        [Header(Tile Height and Normal Generation)]
        _TileHeightVariation ("Uneven Tile Height Variation", Range(0.0, 0.1)) = 0.025
        _NormalIntensity ("Normal Intensity", Range(0.1, 5.0)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.HLSL"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.HLSL"

            CBUFFER_START(UnityPerMaterial)
                float _GridScale;
                float _GroutWidth;
                float _BevelWidth;
                float4 _TileColorA;
                float4 _TileColorB;
                float4 _GroutColor;
                float _TileRoughness;
                float _GroutRoughness;
                float _Metallic;
                float _TileHeightVariation;
                float _NormalIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 tangentWS    : TEXCOORD3;
                float3 bitangentWS  : TEXCOORD4;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            // Returns: x = isTile (0 or 1), y = checker pattern (0 or 1), z = tile height offset, w = bevel factor
            float4 GetTileData(float2 uv)
            {
                uv *= _GridScale;
                float2 cellID = floor(uv);
                float2 cellUV = frac(uv);

                // Checkerboard pattern calculation
                float checker = fmod(cellID.x + cellID.y, 2.0);

                // Random height offset per tile for uneven lighting response
                float tileRandomHeight = (Hash21(cellID) - 0.5) * _TileHeightVariation;

                // Grout boundaries
                float2 edgeDist = abs(cellUV - 0.5);
                float maxDist = max(edgeDist.x, edgeDist.y);
                float groutLimit = 0.5 - _GroutWidth;
                float isTile = step(maxDist, groutLimit);

                // Bevel calculation near edges
                float distFromEdge = 0.5 - maxDist;
                float bevelFactor = saturate(distFromEdge / max(_BevelWidth, 0.0001));
                bevelFactor = smoothstep(0.0, 1.0, bevelFactor);

                return float4(isTile, checker, tileRandomHeight, bevelFactor);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * input.tangentOS.w;
                
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                // Finite difference sampling for normal map generation based on tile heights and bevels
                float eps = 0.002;
                float4 cData = GetTileData(uv);
                float4 rData = GetTileData(uv + float2(eps, 0));
                float4 uData = GetTileData(uv + float2(0, eps));

                // Height function combining tile height offset + edge beveling
                float hCenter = (cData.x > 0.5) ? (cData.z + cData.w * 0.01) : 0.0;
                float hRight  = (rData.x > 0.5) ? (rData.z + rData.w * 0.01) : 0.0;
                float hUp     = (uData.x > 0.5) ? (uData.z + uData.w * 0.01) : 0.0;

                float dHdx = (hRight - hCenter) / eps;
                float dHdy = (hUp - hCenter) / eps;

                // Construct crisp normal from tile height changes
                float3 localNormal = normalize(float3(-dHdx * _NormalIntensity, -dHdy * _NormalIntensity, 1.0));
                float3 normalWS = normalize(
                    localNormal.x * input.tangentWS +
                    localNormal.y * input.bitangentWS +
                    localNormal.z * input.normalWS
                );

                // Colors: Checkerboard alternation + Grout
                float3 tileColor = lerp(_TileColorA.rgb, _TileColorB.rgb, cData.y);
                float3 finalColor = lerp(_GroutColor.rgb, tileColor, cData.x);

                // Darken bevel edges slightly for depth separation
                finalColor *= lerp(0.85, 1.0, cData.w);

                float finalRoughness = lerp(_GroutRoughness, _TileRoughness, cData.x);

                // URP Lighting calculation
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                float3 lightDir = mainLight.direction;
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                
                float NdotL = saturate(dot(normalWS, lightDir));
                float3 shadowColor = mainLight.color * NdotL * mainLight.shadowAttenuation;
                
                float3 ambient = SampleSH(normalWS);
                float3 lighting = ambient + shadowColor;

                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specularPower = exp2(lerp(10.0, 8.0, finalRoughness));
                float specular = pow(NdotH, specularPower) * (1.0 - finalRoughness);

                float3 finalRGB = finalColor * lighting + (mainLight.color * specular * (1.0 - _Metallic));

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}