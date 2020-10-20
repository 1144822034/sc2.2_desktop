using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Game
{
	public class ModelShader : Shader
	{
		private ShaderParameter m_worldMatrixParameter;

		private ShaderParameter m_worldViewProjectionMatrixParameter;

		private ShaderParameter m_textureParameter;

		private ShaderParameter m_samplerStateParameter;

		private ShaderParameter m_materialColorParameter;

		private ShaderParameter m_emissionColorParameter;

		private ShaderParameter m_alphaThresholdParameter;

		private ShaderParameter m_ambientLightColorParameter;

		private ShaderParameter m_diffuseLightColor1Parameter;

		private ShaderParameter m_directionToLight1Parameter;

		private ShaderParameter m_diffuseLightColor2Parameter;

		private ShaderParameter m_directionToLight2Parameter;

		private ShaderParameter m_fogColorParameter;

		private ShaderParameter m_fogStartInvLengthParameter;

		private ShaderParameter m_fogYMultiplierParameter;

		private ShaderParameter m_worldUpParameter;

		private int m_instancesCount;

		public readonly ShaderTransforms Transforms;

		public Texture2D Texture
		{
			set
			{
				m_textureParameter.SetValue(value);
			}
		}

		public SamplerState SamplerState
		{
			set
			{
				m_samplerStateParameter.SetValue(value);
			}
		}

		public Vector4 MaterialColor
		{
			set
			{
				m_materialColorParameter.SetValue(value);
			}
		}

		public Vector4 EmissionColor
		{
			set
			{
				m_emissionColorParameter.SetValue(value);
			}
		}

		public float AlphaThreshold
		{
			set
			{
				m_alphaThresholdParameter.SetValue(value);
			}
		}

		public Vector3 AmbientLightColor
		{
			set
			{
				m_ambientLightColorParameter.SetValue(value);
			}
		}

		public Vector3 DiffuseLightColor1
		{
			set
			{
				m_diffuseLightColor1Parameter.SetValue(value);
			}
		}

		public Vector3 DiffuseLightColor2
		{
			set
			{
				m_diffuseLightColor2Parameter.SetValue(value);
			}
		}

		public Vector3 LightDirection1
		{
			set
			{
				m_directionToLight1Parameter.SetValue(-value);
			}
		}

		public Vector3 LightDirection2
		{
			set
			{
				m_directionToLight2Parameter.SetValue(-value);
			}
		}

		public Vector3 FogColor
		{
			set
			{
				m_fogColorParameter.SetValue(value);
			}
		}

		public Vector2 FogStartInvLength
		{
			set
			{
				m_fogStartInvLengthParameter.SetValue(value);
			}
		}

		public float FogYMultiplier
		{
			set
			{
				m_fogYMultiplierParameter.SetValue(value);
			}
		}

		public Vector3 WorldUp
		{
			set
			{
				m_worldUpParameter.SetValue(value);
			}
		}

		public int InstancesCount
		{
			get
			{
				return m_instancesCount;
			}
			set
			{
				if (value < 0 || value > Transforms.MaxWorldMatrices)
				{
					throw new InvalidOperationException("Invalid instances count.");
				}
				m_instancesCount = value;
			}
		}

		public ModelShader(bool useAlphaThreshold, int maxInstancesCount = 1)
			: base(ContentManager.Get<string>("Shaders/ModelVsh"), ContentManager.Get<string>("Shaders/ModelPsh"), PrepareShaderMacros(useAlphaThreshold, maxInstancesCount))
		{
			m_worldMatrixParameter = GetParameter("u_worldMatrix");
			m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix");
			m_textureParameter = GetParameter("u_texture");
			m_samplerStateParameter = GetParameter("u_samplerState");
			m_materialColorParameter = GetParameter("u_materialColor");
			m_emissionColorParameter = GetParameter("u_emissionColor");
			m_alphaThresholdParameter = GetParameter("u_alphaThreshold", allowNull: true);
			m_ambientLightColorParameter = GetParameter("u_ambientLightColor");
			m_diffuseLightColor1Parameter = GetParameter("u_diffuseLightColor1");
			m_directionToLight1Parameter = GetParameter("u_directionToLight1");
			m_diffuseLightColor2Parameter = GetParameter("u_diffuseLightColor2");
			m_directionToLight2Parameter = GetParameter("u_directionToLight2");
			m_fogColorParameter = GetParameter("u_fogColor");
			m_fogStartInvLengthParameter = GetParameter("u_fogStartInvLength");
			m_fogYMultiplierParameter = GetParameter("u_fogYMultiplier");
			m_worldUpParameter = GetParameter("u_worldUp");
			Transforms = new ShaderTransforms(maxInstancesCount);
		}

		protected override void PrepareForDrawingOverride()
		{
			Transforms.UpdateMatrices(m_instancesCount, worldView: false, viewProjection: false, worldViewProjection: true);
			m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, InstancesCount);
			m_worldMatrixParameter.SetValue(Transforms.World, InstancesCount);
		}

		public static ShaderMacro[] PrepareShaderMacros(bool useAlphaThreshold, int maxInstancesCount)
		{
			List<ShaderMacro> list = new List<ShaderMacro>();
			if (useAlphaThreshold)
			{
				list.Add(new ShaderMacro("ALPHATESTED"));
			}
			list.Add(new ShaderMacro("MAX_INSTANCES_COUNT", maxInstancesCount.ToString(CultureInfo.InvariantCulture)));
			return list.ToArray();
		}
	}
}
