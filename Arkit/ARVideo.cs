using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.iOS;

public class ARVideo : MonoBehaviour
{
	public GameObject PointCloudPrefab = null;
	public uint numPointsToShow = 100;
	private bool pointShow = false;

	private List<GameObject> pointCloundObjects;
	private Vector3[] m_PointCloudData;

	public Material m_ClearMaterial;

	private CommandBuffer m_VideoCommandBuffer;
	private Texture2D _videoTextureY;
	private Texture2D _videoTextureCbCr;
	private Matrix4x4 _displayTransform;

	private bool bCommandBufferInitialized;

	private float fTexCoordScale;
	private ScreenOrientation screenOrientation;

	private ARTrackingState trackingState;
	private ARTrackingStateReason trackingReason;

	public void Start ()
	{
		fTexCoordScale = 1.0f;
		screenOrientation = ScreenOrientation.LandscapeLeft;
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += UpdateFrame;
		m_VideoCommandBuffer = new CommandBuffer (); 
		m_VideoCommandBuffer.Blit (null, BuiltinRenderTextureType.CurrentActive, m_ClearMaterial);
		bCommandBufferInitialized = false;
		if (PointCloudPrefab != null) {
			pointCloundObjects = new List<GameObject> ();
			for (int i = 0; i < numPointsToShow; i++) {
				pointCloundObjects.Add (Instantiate (PointCloudPrefab));
			}
			pointShow = true;
		}
	}

	void UpdateFrame (UnityARCamera cam)
	{
		_displayTransform = new Matrix4x4 ();
		_displayTransform.SetColumn (0, cam.displayTransform.column0);
		_displayTransform.SetColumn (1, cam.displayTransform.column1);
		_displayTransform.SetColumn (2, cam.displayTransform.column2);
		_displayTransform.SetColumn (3, cam.displayTransform.column3);
		if (cam.trackingState != trackingState) {
			trackingState = cam.trackingState;
			trackingReason = cam.trackingReason;
			UpdateTracking ();
		}
		m_PointCloudData = cam.pointCloudData;
	}

	void UpdateTracking ()
	{
//		Debug.Log ("state:" + trackingState + "; reason:" + trackingReason);
	}

	void InitializeCommandBuffer ()
	{
		GetComponent<Camera> ().AddCommandBuffer (CameraEvent.BeforeForwardOpaque, m_VideoCommandBuffer);
		bCommandBufferInitialized = true;
	}

	public void clearPoints ()
	{
		pointShow = false;
	}

	public void setPoints ()
	{
		pointShow = PointCloudPrefab != null;
	}

	void OnDestroy ()
	{
		if (bCommandBufferInitialized) {
			GetComponent<Camera> ().RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, m_VideoCommandBuffer);
		}
		UnityARSessionNativeInterface.ARFrameUpdatedEvent -= UpdateFrame;
		bCommandBufferInitialized = false;
	}

	#if !UNITY_EDITOR
	
	public void OnPreRender()
	{
	ARTextureHandles handles = UnityARSessionNativeInterface.GetARSessionNativeInterface ().GetARVideoTextureHandles();
	if (handles.textureY == System.IntPtr.Zero || handles.textureCbCr == System.IntPtr.Zero)
	{
	return;
	}

	if (!bCommandBufferInitialized) {
	InitializeCommandBuffer ();
	}

	Resolution currentResolution = Screen.currentResolution;

	// Texture Y
	if (_videoTextureY == null) {
	_videoTextureY = Texture2D.CreateExternalTexture(currentResolution.width, currentResolution.height,
	TextureFormat.R8, false, false, (System.IntPtr)handles.textureY);
	_videoTextureY.filterMode = FilterMode.Bilinear;
	_videoTextureY.wrapMode = TextureWrapMode.Repeat;
	m_ClearMaterial.SetTexture("_textureY", _videoTextureY);
	}

	// Texture CbCr
	if (_videoTextureCbCr == null) {
	_videoTextureCbCr = Texture2D.CreateExternalTexture(currentResolution.width, currentResolution.height,
	TextureFormat.RG16, false, false, (System.IntPtr)handles.textureCbCr);
	_videoTextureCbCr.filterMode = FilterMode.Bilinear;
	_videoTextureCbCr.wrapMode = TextureWrapMode.Repeat;
	m_ClearMaterial.SetTexture("_textureCbCr", _videoTextureCbCr);
	}

	_videoTextureY.UpdateExternalTexture(handles.textureY);
	_videoTextureCbCr.UpdateExternalTexture(handles.textureCbCr);

	m_ClearMaterial.SetMatrix("_DisplayTransform", _displayTransform);
	}



#else

	public void SetYTexure (Texture2D YTex)
	{
		_videoTextureY = YTex;
	}

	public Texture2D GetYTexure ()
	{
		return _videoTextureY;
	}

	public void SetUVTexure (Texture2D UVTex)
	{
		_videoTextureCbCr = UVTex;
	}

	public Texture2D GetUVTexure ()
	{
		return _videoTextureCbCr;
	}

	public void OnPreRender ()
	{

//		if (!bCommandBufferInitialized) {
//			InitializeCommandBuffer ();
//		}
//
//		m_ClearMaterial.SetTexture ("_textureY", _videoTextureY);
//		m_ClearMaterial.SetTexture ("_textureCbCr", _videoTextureCbCr);
//
//		m_ClearMaterial.SetMatrix ("_DisplayTransform", _displayTransform);
	}

	#endif
	public void Update ()
	{
		if (PointCloudPrefab != null) {
			if (m_PointCloudData != null && pointShow) {
				long min = Math.Min (m_PointCloudData.Length, numPointsToShow);
				int gap = (int)(m_PointCloudData.Length / (min - 1));
				if (gap == 0)
					gap = 1;
				for (int count = 0; count < min; count += gap) {
					Vector4 vert = m_PointCloudData [count];
					GameObject point = pointCloundObjects [count];
					point.transform.position = new Vector3 (vert.x, vert.y, vert.z);
					point.SetActive (true);
				}
			} else {
				for (int count = 0; count < numPointsToShow; count += 1) {
					GameObject point = pointCloundObjects [count];
					point.SetActive (false);
				}
			}
		}
	}

	//	public void Update(){
	//		if(m_PointCloudData != null && PointCloudPrefab != null){
	//			for(int count = 0; count < Math.Min(m_PointCloudData.Length, numPointsToShow); count++){
	//				Vector4 vert = m_PointCloudData[count];
	//				GameObject point = pointCloundObjects[count];
	//				point.transform.position = new Vector3(vert.x, vert.y, vert.z);
	//			}
	//		}
	//	}
}