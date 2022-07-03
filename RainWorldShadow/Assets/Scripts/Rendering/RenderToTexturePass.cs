using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class RenderToTexturePass : ScriptableRenderPass
{
	FilteringSettings m_FilteringSettings;
	string m_ProfilerTag;
	ProfilingSampler m_ProfilingSampler;

	public RenderTexture colorOutput;
	public RenderTexture depthOutput;

	public Material overrideMaterial { get; set; }
	public int overrideMaterialPassIndex { get; set; }

	List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
	{
		m_RenderStateBlock.mask |= RenderStateMask.Depth;
		m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
	}

	public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
	{
		StencilState stencilState = StencilState.defaultValue;
		stencilState.enabled = true;
		stencilState.SetCompareFunction(compareFunction);
		stencilState.SetPassOperation(passOp);
		stencilState.SetFailOperation(failOp);
		stencilState.SetZFailOperation(zFailOp);

		m_RenderStateBlock.mask |= RenderStateMask.Stencil;
		m_RenderStateBlock.stencilReference = reference;
		m_RenderStateBlock.stencilState = stencilState;
	}

	RenderStateBlock m_RenderStateBlock;

	public RenderToTexturePass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, int layerMask)
	{
		base.profilingSampler = new ProfilingSampler(nameof(RenderToTexturePass));

		m_ProfilerTag = profilerTag;
		m_ProfilingSampler = new ProfilingSampler(profilerTag);
		this.renderPassEvent = renderPassEvent;
		this.overrideMaterial = null;
		this.overrideMaterialPassIndex = 0;
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

		if (shaderTags != null && shaderTags.Length > 0)
		{
			foreach (var passName in shaderTags)
				m_ShaderTagIdList.Add(new ShaderTagId(passName));
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
		}

		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (!colorOutput)
		{
			Debug.LogWarning("Color output is null. Default render target will be used.");
			return;
		}

		if (!depthOutput)
		{
			Debug.LogWarning("Depth output is null. Default render target will be used.");
			return;
		}

		ConfigureTarget(colorOutput, depthOutput);
		ConfigureClear(ClearFlag.All, Color.clear);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (!colorOutput)
		{
			Debug.LogWarning("Color output is null. The render pass is skipped.");
			return;
		}

		if (!depthOutput)
		{
			Debug.LogWarning("Depth output is null. The render pass is skipped.");
			return;
		}

		SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
		DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
		drawingSettings.overrideMaterial = overrideMaterial;
		drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

		ref CameraData cameraData = ref renderingData.cameraData;
		Camera camera = cameraData.camera;

		// NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
		// Currently there's an issue which results in mismatched markers.
		CommandBuffer cmd = CommandBufferPool.Get();

		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			// Ensure we flush our command-buffer before we render...
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();

			// Render the objects...
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
		}
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
