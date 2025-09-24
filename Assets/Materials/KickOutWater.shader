Shader "Custom/KickOutWater"
{
    Properties
    {
        _ShallowColour("ShallowColour", Color) = (0.3882353, 0.6117647, 0.6980392, 0.3803922)
        _DeepColour("DeepColour", Color) = (0, 0.4745098, 1, 0.1137255)
        _Distance("Distance", Float) = 10
        _Speed("Speed", Float) = 1
        _Scale("Scale", Float) = 0.5
        _FoamAmount("FoamAmount", Float) = 1
        _FoamCuttoff("FoamCuttoff", Float) = 1
        _FoamSpeed("FoamSpeed", Float) = 1
        _FoamScale("FoamScale", Float) = 100
        _FoamColour("FoamColour", Color) = (0, 0, 0, 0.5019608)
        [HideInInspector]_BUILTIN_QueueOffset("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueControl("Float", Float) = -1

        // Blending state
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            // RenderPipeline: <None>
            "RenderType"="Transparent"
            "BuiltInMaterialType" = "Lit"
            "Queue"="Geometry-2"
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="BuiltInLitSubTarget"
        }
        Pass
        {
            Name "BuiltIn Forward"
            Tags
            {
                "LightMode" = "ForwardBase"
            }
        
        // Render State
        Cull Back
        Blend [_SrcBlend] [_DstBlend]
        ZTest LEqual
        ZWrite [_ZWrite]
        ColorMask RGB
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
             float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV : INTERP0;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP1;
            #endif
             float4 tangentWS : INTERP2;
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float4 shadowCoord : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.shadowCoord.xyzw = input.shadowCoord;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.shadowCoord = input.shadowCoord.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // Graph Functions
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);
        
            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));
        
            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_ChannelMixer_float (float3 In, float3 Red, float3 Green, float3 Blue, out float3 Out)
        {
            Out = float3(dot(In, Red), dot(In, Green), dot(In, Blue));
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4 = _ShallowColour;
            float4 _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4 = _DeepColour;
            float _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float);
            float4 _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_R_1_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[0];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_G_2_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[1];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_B_3_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[2];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[3];
            float _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float, _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float, _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float);
            float _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float = _Distance;
            float _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float;
            Unity_Divide_float(_Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float, _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float, _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float);
            float _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float;
            Unity_Saturate_float(_Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float, _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float);
            float4 _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4;
            Unity_Lerp_float4(_Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4, _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4, (_Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float.xxxx), _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4);
            float4 _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4 = _FoamColour;
            float _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float);
            float4 _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_c8655d8965bc45bea0a8df55462ac648_R_1_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[0];
            float _Split_c8655d8965bc45bea0a8df55462ac648_G_2_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[1];
            float _Split_c8655d8965bc45bea0a8df55462ac648_B_3_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[2];
            float _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[3];
            float _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float, _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float, _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float);
            float _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float = _FoamAmount;
            float _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float;
            Unity_Divide_float(_Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float, _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float, _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float);
            float _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float;
            Unity_Saturate_float(_Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float, _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float);
            float _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float = _FoamCuttoff;
            float _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float, _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float, _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float);
            float _Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float = _FoamScale;
            float _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float = _Speed;
            float2 _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2 = float2(0, _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float);
            float2 _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2, _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2);
            float2 _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float.xx), _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2, _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2);
            float _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2, 1, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float);
            float _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float;
            Unity_Step_float(_Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float, _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float);
            float _Split_0317a4582c26453d9a74089507c72b11_R_1_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[0];
            float _Split_0317a4582c26453d9a74089507c72b11_G_2_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[1];
            float _Split_0317a4582c26453d9a74089507c72b11_B_3_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[2];
            float _Split_0317a4582c26453d9a74089507c72b11_A_4_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[3];
            float _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            Unity_Multiply_float_float(_Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float, _Split_0317a4582c26453d9a74089507c72b11_A_4_Float, _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float);
            float4 _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4, _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4, (_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float.xxxx), _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4);
            float _Property_eb5def6349174effa733d2cc0aede367_Out_0_Float = _Scale;
            float _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float = _Speed;
            float _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2 = float2(0, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_eb5def6349174effa733d2cc0aede367_Out_0_Float.xx), _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2, _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2);
            float _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2, 20, _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3;
            float3x3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float,0.01,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix, _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3);
            float _Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float = _Scale;
            float3 _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3;
            Unity_Multiply_float3_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, (_Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float.xxx), _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3);
            float3 _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3;
            Unity_Add_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3, _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3;
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red = float3 (1.02, 2, -0.23);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green = float3 (0, 1, 0);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue = float3 (0, 0, 1);
            Unity_ChannelMixer_float(_Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3);
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_R_1_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[0];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_G_2_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[1];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_B_3_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[2];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[3];
            float3 _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            Unity_Lerp_float3((_Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4.xyz), _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3, (_Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float.xxx), _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3);
            float3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3;
            float3x3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float,0.05,_NormalFromHeight_200a425826b0451cb764f63131ddd373_Position,_NormalFromHeight_200a425826b0451cb764f63131ddd373_TangentMatrix, _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3);
            surface.BaseColor = _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            surface.NormalTS = _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = 0;
            surface.Smoothness = _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            surface.Occlusion = 1;
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            // World Tangent isn't an available input on v2f_surf
        
            result._ShadowCoord = varyings.shadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            result.sh = varyings.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lmap.xy = varyings.lightmapUV;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
            result.shadowCoord = surfVertex._ShadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            result.sh = surfVertex.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lightmapUV = surfVertex.lmap.xy;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "BuiltIn ForwardAdd"
            Tags
            {
                "LightMode" = "ForwardAdd"
            }
        
        // Render State
        Blend [_SrcBlend] One
        ZWrite [_ZWrite]
        ColorMask RGB
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdadd_fullshadows
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD_ADD
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
             float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV : INTERP0;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP1;
            #endif
             float4 tangentWS : INTERP2;
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float4 shadowCoord : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.shadowCoord.xyzw = input.shadowCoord;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.shadowCoord = input.shadowCoord.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // Graph Functions
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);
        
            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));
        
            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_ChannelMixer_float (float3 In, float3 Red, float3 Green, float3 Blue, out float3 Out)
        {
            Out = float3(dot(In, Red), dot(In, Green), dot(In, Blue));
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4 = _ShallowColour;
            float4 _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4 = _DeepColour;
            float _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float);
            float4 _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_R_1_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[0];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_G_2_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[1];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_B_3_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[2];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[3];
            float _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float, _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float, _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float);
            float _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float = _Distance;
            float _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float;
            Unity_Divide_float(_Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float, _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float, _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float);
            float _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float;
            Unity_Saturate_float(_Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float, _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float);
            float4 _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4;
            Unity_Lerp_float4(_Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4, _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4, (_Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float.xxxx), _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4);
            float4 _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4 = _FoamColour;
            float _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float);
            float4 _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_c8655d8965bc45bea0a8df55462ac648_R_1_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[0];
            float _Split_c8655d8965bc45bea0a8df55462ac648_G_2_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[1];
            float _Split_c8655d8965bc45bea0a8df55462ac648_B_3_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[2];
            float _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[3];
            float _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float, _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float, _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float);
            float _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float = _FoamAmount;
            float _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float;
            Unity_Divide_float(_Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float, _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float, _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float);
            float _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float;
            Unity_Saturate_float(_Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float, _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float);
            float _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float = _FoamCuttoff;
            float _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float, _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float, _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float);
            float _Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float = _FoamScale;
            float _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float = _Speed;
            float2 _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2 = float2(0, _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float);
            float2 _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2, _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2);
            float2 _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float.xx), _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2, _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2);
            float _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2, 1, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float);
            float _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float;
            Unity_Step_float(_Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float, _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float);
            float _Split_0317a4582c26453d9a74089507c72b11_R_1_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[0];
            float _Split_0317a4582c26453d9a74089507c72b11_G_2_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[1];
            float _Split_0317a4582c26453d9a74089507c72b11_B_3_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[2];
            float _Split_0317a4582c26453d9a74089507c72b11_A_4_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[3];
            float _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            Unity_Multiply_float_float(_Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float, _Split_0317a4582c26453d9a74089507c72b11_A_4_Float, _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float);
            float4 _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4, _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4, (_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float.xxxx), _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4);
            float _Property_eb5def6349174effa733d2cc0aede367_Out_0_Float = _Scale;
            float _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float = _Speed;
            float _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2 = float2(0, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_eb5def6349174effa733d2cc0aede367_Out_0_Float.xx), _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2, _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2);
            float _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2, 20, _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3;
            float3x3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float,0.01,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix, _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3);
            float _Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float = _Scale;
            float3 _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3;
            Unity_Multiply_float3_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, (_Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float.xxx), _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3);
            float3 _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3;
            Unity_Add_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3, _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3;
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red = float3 (1.02, 2, -0.23);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green = float3 (0, 1, 0);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue = float3 (0, 0, 1);
            Unity_ChannelMixer_float(_Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3);
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_R_1_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[0];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_G_2_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[1];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_B_3_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[2];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[3];
            float3 _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            Unity_Lerp_float3((_Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4.xyz), _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3, (_Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float.xxx), _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3);
            float3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3;
            float3x3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float,0.05,_NormalFromHeight_200a425826b0451cb764f63131ddd373_Position,_NormalFromHeight_200a425826b0451cb764f63131ddd373_TangentMatrix, _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3);
            surface.BaseColor = _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            surface.NormalTS = _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = 0;
            surface.Smoothness = _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            surface.Occlusion = 1;
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            // World Tangent isn't an available input on v2f_surf
        
            result._ShadowCoord = varyings.shadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            result.sh = varyings.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lmap.xy = varyings.lightmapUV;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
            result.shadowCoord = surfVertex._ShadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            result.sh = surfVertex.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lightmapUV = surfVertex.lmap.xy;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/PBRForwardAddPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "BuiltIn Deferred"
            Tags
            {
                "LightMode" = "Deferred"
            }
        
        // Render State
        Cull Back
        Blend [_SrcBlend] [_DstBlend]
        ZTest LEqual
        ZWrite [_ZWrite]
        ColorMask RGB
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma multi_compile_instancing
        #pragma exclude_renderers nomrt
        #pragma multi_compile_prepassfinal
        #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEFERRED
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
             float4 shadowCoord;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 lightmapUV : INTERP0;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP1;
            #endif
             float4 tangentWS : INTERP2;
             float4 texCoord0 : INTERP3;
             float4 fogFactorAndVertexLight : INTERP4;
             float4 shadowCoord : INTERP5;
             float3 positionWS : INTERP6;
             float3 normalWS : INTERP7;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.shadowCoord.xyzw = input.shadowCoord;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.shadowCoord = input.shadowCoord.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // Graph Functions
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);
        
            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));
        
            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_ChannelMixer_float (float3 In, float3 Red, float3 Green, float3 Blue, out float3 Out)
        {
            Out = float3(dot(In, Red), dot(In, Green), dot(In, Blue));
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4 = _ShallowColour;
            float4 _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4 = _DeepColour;
            float _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float);
            float4 _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_R_1_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[0];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_G_2_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[1];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_B_3_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[2];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[3];
            float _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float, _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float, _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float);
            float _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float = _Distance;
            float _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float;
            Unity_Divide_float(_Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float, _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float, _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float);
            float _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float;
            Unity_Saturate_float(_Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float, _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float);
            float4 _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4;
            Unity_Lerp_float4(_Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4, _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4, (_Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float.xxxx), _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4);
            float4 _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4 = _FoamColour;
            float _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float);
            float4 _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_c8655d8965bc45bea0a8df55462ac648_R_1_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[0];
            float _Split_c8655d8965bc45bea0a8df55462ac648_G_2_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[1];
            float _Split_c8655d8965bc45bea0a8df55462ac648_B_3_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[2];
            float _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[3];
            float _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float, _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float, _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float);
            float _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float = _FoamAmount;
            float _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float;
            Unity_Divide_float(_Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float, _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float, _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float);
            float _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float;
            Unity_Saturate_float(_Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float, _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float);
            float _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float = _FoamCuttoff;
            float _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float, _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float, _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float);
            float _Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float = _FoamScale;
            float _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float = _Speed;
            float2 _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2 = float2(0, _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float);
            float2 _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2, _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2);
            float2 _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float.xx), _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2, _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2);
            float _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2, 1, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float);
            float _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float;
            Unity_Step_float(_Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float, _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float);
            float _Split_0317a4582c26453d9a74089507c72b11_R_1_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[0];
            float _Split_0317a4582c26453d9a74089507c72b11_G_2_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[1];
            float _Split_0317a4582c26453d9a74089507c72b11_B_3_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[2];
            float _Split_0317a4582c26453d9a74089507c72b11_A_4_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[3];
            float _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            Unity_Multiply_float_float(_Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float, _Split_0317a4582c26453d9a74089507c72b11_A_4_Float, _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float);
            float4 _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4, _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4, (_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float.xxxx), _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4);
            float _Property_eb5def6349174effa733d2cc0aede367_Out_0_Float = _Scale;
            float _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float = _Speed;
            float _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2 = float2(0, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_eb5def6349174effa733d2cc0aede367_Out_0_Float.xx), _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2, _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2);
            float _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2, 20, _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3;
            float3x3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float,0.01,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix, _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3);
            float _Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float = _Scale;
            float3 _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3;
            Unity_Multiply_float3_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, (_Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float.xxx), _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3);
            float3 _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3;
            Unity_Add_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3, _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3;
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red = float3 (1.02, 2, -0.23);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green = float3 (0, 1, 0);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue = float3 (0, 0, 1);
            Unity_ChannelMixer_float(_Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3);
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_R_1_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[0];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_G_2_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[1];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_B_3_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[2];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[3];
            float3 _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            Unity_Lerp_float3((_Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4.xyz), _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3, (_Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float.xxx), _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3);
            float3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3;
            float3x3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_200a425826b0451cb764f63131ddd373_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float,0.05,_NormalFromHeight_200a425826b0451cb764f63131ddd373_Position,_NormalFromHeight_200a425826b0451cb764f63131ddd373_TangentMatrix, _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3);
            surface.BaseColor = _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            surface.NormalTS = _NormalFromHeight_200a425826b0451cb764f63131ddd373_Out_1_Vector3;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = 0;
            surface.Smoothness = _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            surface.Occlusion = 1;
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            // World Tangent isn't an available input on v2f_surf
        
            result._ShadowCoord = varyings.shadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            result.sh = varyings.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lmap.xy = varyings.lightmapUV;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
            result.shadowCoord = surfVertex._ShadowCoord;
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            result.sh = surfVertex.sh;
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            result.lightmapUV = surfVertex.lmap.xy;
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/PBRDeferredPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Back
        Blend [_SrcBlend] [_DstBlend]
        ZTest LEqual
        ZWrite [_ZWrite]
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_shadowcaster
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define BUILTIN_TARGET_API 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 WorldSpaceTangent;
             float3 WorldSpaceBiTangent;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // Graph Functions
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_NormalFromHeight_Tangent_float(float In, float Strength, float3 Position, float3x3 TangentMatrix, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'Normal From Height' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            float3 worldDerivativeX = ddx(Position);
            float3 worldDerivativeY = ddy(Position);
        
            float3 crossX = cross(TangentMatrix[2].xyz, worldDerivativeX);
            float3 crossY = cross(worldDerivativeY, TangentMatrix[2].xyz);
            float d = dot(worldDerivativeX, crossY);
            float sgn = d < 0.0 ? (-1.0f) : 1.0f;
            float surface = sgn / max(0.000000000000001192093f, abs(d));
        
            float dHdx = ddx(In);
            float dHdy = ddy(In);
            float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
            Out = SafeNormalize(TangentMatrix[2].xyz - (Strength * surfGrad));
            Out = TransformWorldToTangent(Out, TangentMatrix);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_ChannelMixer_float (float3 In, float3 Red, float3 Green, float3 Blue, out float3 Out)
        {
            Out = float3(dot(In, Red), dot(In, Green), dot(In, Blue));
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4 = _ShallowColour;
            float4 _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4 = _DeepColour;
            float _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float);
            float4 _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_R_1_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[0];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_G_2_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[1];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_B_3_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[2];
            float _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float = _ScreenPosition_944042e8058545179f895f1686da4cf6_Out_0_Vector4[3];
            float _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_18c1f5cc70e34258b79c6bc9572feb9f_Out_1_Float, _Split_fe7a2d5eb8f145c584754acc1d81901b_A_4_Float, _Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float);
            float _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float = _Distance;
            float _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float;
            Unity_Divide_float(_Subtract_40a7edc4b11d49c29f2c11b06caf2924_Out_2_Float, _Property_c651968cae33471d9d94dbee5864549a_Out_0_Float, _Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float);
            float _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float;
            Unity_Saturate_float(_Divide_8613cd17a2ea4758b4a2d1dfd293406d_Out_2_Float, _Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float);
            float4 _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4;
            Unity_Lerp_float4(_Property_e8ee57e2a2b14682bf9b2168f9408b67_Out_0_Vector4, _Property_1925ff74894b431f9d187aec104f5e78_Out_0_Vector4, (_Saturate_bf6044a88298487a98e6d0ec075c3fbc_Out_1_Float.xxxx), _Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4);
            float4 _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4 = _FoamColour;
            float _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float;
            Unity_SceneDepth_Eye_float(float4(IN.NDCPosition.xy, 0, 0), _SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float);
            float4 _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4 = IN.ScreenPosition;
            float _Split_c8655d8965bc45bea0a8df55462ac648_R_1_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[0];
            float _Split_c8655d8965bc45bea0a8df55462ac648_G_2_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[1];
            float _Split_c8655d8965bc45bea0a8df55462ac648_B_3_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[2];
            float _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float = _ScreenPosition_19016beb5610463b84b05a8b58e564c8_Out_0_Vector4[3];
            float _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float;
            Unity_Subtract_float(_SceneDepth_d102ceee2c1144498509fe74584489b0_Out_1_Float, _Split_c8655d8965bc45bea0a8df55462ac648_A_4_Float, _Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float);
            float _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float = _FoamAmount;
            float _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float;
            Unity_Divide_float(_Subtract_01c7915e2b204269bacfbbb03ceb778b_Out_2_Float, _Property_a6e8451c04f448a0838e0a9d66a63dfa_Out_0_Float, _Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float);
            float _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float;
            Unity_Saturate_float(_Divide_c6adbd5120ab4725b48d036267a1d94a_Out_2_Float, _Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float);
            float _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float = _FoamCuttoff;
            float _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_1ce8aa9cabb84fd7a2bce08ce8b4567c_Out_1_Float, _Property_c657032848054f039358b5a7e8a93bec_Out_0_Float, _Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float);
            float _Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float = _FoamScale;
            float _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float = _Speed;
            float2 _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2 = float2(0, _Property_b99e7f9b94614827b2261a3cc49cff7d_Out_0_Float);
            float2 _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Vector2_fc6d22e324864fa2a6d8c2957d00193d_Out_0_Vector2, _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2);
            float2 _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_d7fc932a4b8c4b89a3973cfe05fc539a_Out_0_Float.xx), _Multiply_4d0d63e6918942bba72175512f067f73_Out_2_Vector2, _TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2);
            float _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_4708474048d34cf8844fd4cd5e816c58_Out_3_Vector2, 1, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float);
            float _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float;
            Unity_Step_float(_Multiply_575a6de849c5451fa96a7a78dcf49396_Out_2_Float, _GradientNoise_587f9d5f598d46a7bf84796d009a507e_Out_2_Float, _Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float);
            float _Split_0317a4582c26453d9a74089507c72b11_R_1_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[0];
            float _Split_0317a4582c26453d9a74089507c72b11_G_2_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[1];
            float _Split_0317a4582c26453d9a74089507c72b11_B_3_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[2];
            float _Split_0317a4582c26453d9a74089507c72b11_A_4_Float = _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4[3];
            float _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float;
            Unity_Multiply_float_float(_Step_1be6354faf094b359b3b6a6ea6dc254c_Out_2_Float, _Split_0317a4582c26453d9a74089507c72b11_A_4_Float, _Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float);
            float4 _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4;
            Unity_Lerp_float4(_Lerp_89d5f4caa78d41de9ea7862ad1f2e7e5_Out_3_Vector4, _Property_d2d4ad694a434e3bbbdf2a8ad61503c8_Out_0_Vector4, (_Multiply_5a5d15ef3b814547acf1bbd8fbdceaae_Out_2_Float.xxxx), _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4);
            float _Property_eb5def6349174effa733d2cc0aede367_Out_0_Float = _Scale;
            float _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float = _Speed;
            float _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_f77954e4ac4f4d64b6b5c2ccb58cbde4_Out_0_Float, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2 = float2(0, _Multiply_882e4e6e09bb41e28c582bcfbe3853e5_Out_2_Float);
            float2 _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, (_Property_eb5def6349174effa733d2cc0aede367_Out_0_Float.xx), _Vector2_09bcb82e8baa4750af1c09faea7f7bca_Out_0_Vector2, _TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2);
            float _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_271a2d4cdb454efd86c7f200a26db1cf_Out_3_Vector2, 20, _GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3;
            float3x3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
            float3 _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position = IN.WorldSpacePosition;
            Unity_NormalFromHeight_Tangent_float(_GradientNoise_9d4af7ecf90342179fa6435057e87205_Out_2_Float,0.01,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Position,_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_TangentMatrix, _NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3);
            float _Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float = _Scale;
            float3 _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3;
            Unity_Multiply_float3_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, (_Property_310e03e7863b4bd882061be10f49bf96_Out_0_Float.xxx), _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3);
            float3 _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3;
            Unity_Add_float3(_NormalFromHeight_3da2ce1ebd004090a076cf01ce2df672_Out_1_Vector3, _Multiply_12cbdb179fd1452284c9054fd2f67526_Out_2_Vector3, _Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3;
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red = float3 (1.02, 2, -0.23);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green = float3 (0, 1, 0);
            float3 _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue = float3 (0, 0, 1);
            Unity_ChannelMixer_float(_Add_fe8f1fa2a6534feeaaa6db4434c9ec01_Out_2_Vector3, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Red, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Green, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Blue, _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3);
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_R_1_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[0];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_G_2_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[1];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_B_3_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[2];
            float _Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float = _Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4[3];
            float3 _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            Unity_Lerp_float3((_Lerp_b74ea912a1ce48de9af92709ba30f4fb_Out_3_Vector4.xyz), _ChannelMixer_b5036884163640fe93ede0f79beb5413_Out_1_Vector3, (_Split_0ef7239775ca46aeac5c07f0d4ecaf64_A_4_Float.xxx), _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3);
            surface.BaseColor = _Lerp_29f2e8d06da641c8b1f4c76587cd4374_Out_3_Vector3;
            surface.Emission = float3(0, 0, 0);
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
            // use bitangent on the fly like in hdrp
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0)* GetOddNegativeScale();
            float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
        
            // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
            // This is explained in section 2.2 in "surface gradient based bump mapping framework"
            output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
            output.WorldSpaceBiTangent = renormFactor * bitang;
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.texcoord1  = attributes.uv1;
            result.texcoord2  = attributes.uv2;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            result.worldPos = varyings.positionWS;
            result.worldNormal = varyings.normalWS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            result.positionWS = surfVertex.worldPos;
            result.normalWS = surfVertex.worldNormal;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SceneSelectionPass
        #define BUILTIN_TARGET_API 1
        #define SCENESELECTIONPASS 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS ScenePickingPass
        #define BUILTIN_TARGET_API 1
        #define SCENEPICKINGPASS 1
        #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _ShallowColour;
        float4 _DeepColour;
        float _Distance;
        float _Speed;
        float _Scale;
        float _FoamAmount;
        float _FoamCuttoff;
        float _FoamSpeed;
        float _FoamScale;
        float4 _FoamColour;
        CBUFFER_END
        
        
        // Object and Global properties
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        // GraphFunctions: <None>
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            surface.Alpha = 0.8;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInLitGUI" ""
    FallBack "Hidden/Shader Graph/FallbackError"
}
