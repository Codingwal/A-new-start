Shader "SLE/MapGeneration/Terrain" 
{

	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
	
	SubShader
	{
		Tags 
		{ 
			"RenderPipeline" = "UniversalPipeline" 
			"RenderType" = "Opaque"
		    "Queue" = "Geometry"
		}
		LOD 200

		Pass
		{
			Name "Universal Forward"
			Tags 
			{ 
				"LightMode" = "UniversalForward" 
			}

			Cull Back
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#pragma target 4.5
			#pragma exclude_renderers gles gles3
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ DOTS_INSTANCING_ON
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			const static int   maxLayerCount = 8;
			const static float epsilon = 1e-4;

			CBUFFER_START(UnityPerMaterial)

			int    layerCount;
			float3 baseColors[maxLayerCount];
			float  baseStartHeights[maxLayerCount];
			float  baseBlends[maxLayerCount];
			float  baseColorStrengths[maxLayerCount];
			float  baseTextureScales[maxLayerCount];

			float minHeight;
			float maxHeight;

			CBUFFER_END

			TEXTURE2D_ARRAY(baseTextures);
			SAMPLER(sampler_baseTextures);

			struct v2f
			{
				float4 vertex     : SV_POSITION;
				float3 worldPos   : TEXCOORD0;
				half3 worldNormal : TEXCOORD1;
				half4 diff        : COLOR0; // diffuse lighting color
				half3 ambient     : COLOR1;
			};

			float inverseLerp(float a, float b, float value) 
			{
				return saturate((value - a) / (b - a));
			}

			float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) 
			{
				float3 scaledWorldPos = worldPos / scale;
				
				float3 xProjection = (float3)SAMPLE_TEXTURE2D_ARRAY(baseTextures, sampler_baseTextures, float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex) * blendAxes.x;
				float3 yProjection = (float3)SAMPLE_TEXTURE2D_ARRAY(baseTextures, sampler_baseTextures, float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex) * blendAxes.y;
				float3 zProjection = (float3)SAMPLE_TEXTURE2D_ARRAY(baseTextures, sampler_baseTextures, float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex) * blendAxes.z;
				
				return xProjection + yProjection + zProjection;
			}

			v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
			{
				v2f o;

				o.worldPos    = mul(unity_ObjectToWorld, vertex).xyz;
				o.vertex      = TransformObjectToHClip(vertex.xyz);
				o.worldNormal = TransformObjectToWorldNormal(normal);

				Light mainLight = GetMainLight();

				half nl = max(0, dot(o.worldNormal, mainLight.direction.xyz));
				
				// factor in the light color
				o.diff = nl * half4(mainLight.color, 1);
				
				o.ambient = SampleSH(o.worldNormal);
				
				TransformWorldToShadowCoord(vertex.xyz);
				
				return o;
			}

			half4 _Color;
			half4 frag(v2f IN) : SV_Target
			{
				float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
				float3 blendAxes    = abs(IN.worldNormal);
				
				blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

				half3 col = _Color.xyz;
				for (int i = 0; i < layerCount; i++) 
				{
					// - epsilon ensures that the first 2 arguments aren't zero (could lead to errors) even if baseBlends[i] is zero
					float a = -baseBlends[i] / 2.0 - epsilon;
					float b = baseBlends[i] / 2.0;
					float t = heightPercent - baseStartHeights[i];
					
					float drawStrength = inverseLerp(a, b, t);

					float3 baseColor    = baseColors[i] * baseColorStrengths[i];

					float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrengths[i]);
					// textureColor = 0;
					
					col = col * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
				}
				// col = baseColors[2];
				// col.r = 200;

				_Color.rgb = col;

				Light mainLight = GetMainLight();

				return _Color;
			}

			ENDHLSL
		}
	}
}
