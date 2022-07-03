using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public enum RenderQueueType
{
	Opaque,
	Transparent,
}

public class RenderToTextureFeature : ScriptableRendererFeature
{
	[System.Serializable]
	public class FilterSettings
	{
		public RenderQueueType RenderQueueType;
		public LayerMask LayerMask;
		public string[] PassNames;

		public FilterSettings()
		{
			RenderQueueType = RenderQueueType.Opaque;
			LayerMask = 0;
		}
	}

	[System.Serializable]
	public class DepthStateSettings
	{
		public bool overrideDepthState = false;
		public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
		public bool enableWrite = true;
	}

	public string passTag = "RenderToTextureFeature";
	public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

	public RenderTexture colorOutput;
	public RenderTexture depthOutput;

	public FilterSettings filterSettings = new FilterSettings();
	public DepthStateSettings depthStateSettings = new DepthStateSettings();
	public StencilStateData stencilSettings = new StencilStateData();

	public Material overrideMaterial = null;
	public int overrideMaterialPassIndex = 0;

	RenderToTexturePass m_RenderToTexturePass;

	public override void Create()
	{
		FilterSettings filter = filterSettings;

		// RenderToTexture pass doesn't support events before rendering prepasses.
		// The camera is not setup before this point and all rendering is monoscopic.
		// Events before BeforeRenderingPrepasses should be used for input texture passes (shadow map, LUT, etc) that doesn't depend on the camera.
		// These events are filtering in the UI, but we still should prevent users from changing it from code or
		// by changing the serialized data.
		if (Event < RenderPassEvent.BeforeRenderingPrePasses)
		{
			Debug.LogWarning($"RenderToTexture pass doesn't support event {Event}. Set to BeforeRenderingPrePasses.");
			Event = RenderPassEvent.BeforeRenderingPrePasses;
		}

		m_RenderToTexturePass = new RenderToTexturePass(passTag, Event, filter.PassNames, filter.LayerMask);

		m_RenderToTexturePass.colorOutput = colorOutput;
		m_RenderToTexturePass.depthOutput = depthOutput;

		if (depthStateSettings.overrideDepthState)
		{
			m_RenderToTexturePass.SetDetphState(depthStateSettings.enableWrite, depthStateSettings.depthCompareFunction);
		}

		if (stencilSettings.overrideStencilState)
		{
			m_RenderToTexturePass.SetStencilState(stencilSettings.stencilReference,
				stencilSettings.stencilCompareFunction, stencilSettings.passOperation,
				stencilSettings.failOperation, stencilSettings.zFailOperation);
		}

		m_RenderToTexturePass.overrideMaterial = overrideMaterial;
		m_RenderToTexturePass.overrideMaterialPassIndex = overrideMaterialPassIndex;

	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_RenderToTexturePass);
	}
}
