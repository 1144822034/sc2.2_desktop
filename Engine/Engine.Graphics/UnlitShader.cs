using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Engine.Graphics
{
	public class UnlitShader : Shader
	{
		private ShaderParameter m_worldViewProjectionMatrixParameter;

		private ShaderParameter m_textureParameter;

		private ShaderParameter m_samplerStateParameter;

		private ShaderParameter m_colorParameter;

		private ShaderParameter m_alphaThresholdParameter;

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

		public Vector4 Color
		{
			set
			{
				m_colorParameter.SetValue(value);
			}
		}

		public float AlphaThreshold
		{
			set
			{
				m_alphaThresholdParameter.SetValue(value);
			}
		}

		public UnlitShader(bool useVertexColor, bool useTexture, bool useAlphaThreshold)
			: base(new StreamReader(typeof(Shader).GetTypeInfo().Assembly.GetManifestResourceStream("Engine.Resources.Embedded.Unlit.vsh")).ReadToEnd(), new StreamReader(typeof(Shader).GetTypeInfo().Assembly.GetManifestResourceStream("Engine.Resources.Embedded.Unlit.psh")).ReadToEnd(), PrepareShaderMacros(useVertexColor, useTexture, useAlphaThreshold))
		{
			m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
			m_textureParameter = GetParameter("u_texture", allowNull: true);
			m_samplerStateParameter = GetParameter("u_samplerState", allowNull: true);
			m_colorParameter = GetParameter("u_color", allowNull: true);
			m_alphaThresholdParameter = GetParameter("u_alphaThreshold", allowNull: true);
			Transforms = new ShaderTransforms(1);
			Color = Vector4.One;
		}

		protected override void PrepareForDrawingOverride()
		{
			Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
			m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
		}

		private static ShaderMacro[] PrepareShaderMacros(bool useVertexColor, bool useTexture, bool useAlphaThreshold)
		{
			List<ShaderMacro> list = new List<ShaderMacro>();
			if (useVertexColor)
			{
				list.Add(new ShaderMacro("USE_VERTEXCOLOR"));
			}
			if (useTexture)
			{
				list.Add(new ShaderMacro("USE_TEXTURE"));
			}
			if (useAlphaThreshold)
			{
				list.Add(new ShaderMacro("USE_ALPHATHRESHOLD"));
			}
			return list.ToArray();
		}
	}
}
