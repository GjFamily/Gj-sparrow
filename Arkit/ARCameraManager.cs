using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ARCameraManager : MonoBehaviour {

	public Camera m_camera;
	public GameObject Point;
	public Material savedClearMaterial;
	public GameObject PointCloud;
	public uint numPpointsToShow = 400;
	#if !UNITY_EDITOR
	private UnityARSessionNativeInterface m_session;
	private ARSession ar_session;
	#endif

	// Use this for initialization
	void Start () {

		#if !UNITY_EDITOR
		m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
		ar_session = new ARSession();
		ar_session.addAnchor = (aranchor)=>{
		GameObject point = GameObject.Instantiate(Point);

		//do coordinate conversion from ARKit to Unity
		point.transform.position = UnityARMatrixOps.GetPosition (aranchor.planeAnchor.transform);
		point.transform.rotation = UnityARMatrixOps.GetRotation (aranchor.planeAnchor.transform);
		aranchor.gameObject = point;
		};
		ar_session.updateAnchor = (aranchor)=>{
		GameObject point = aranchor.gameObject;
//		Debug.Log("point last position:"+Point.transform.position);
		point.transform.position = UnityARMatrixOps.GetPosition (aranchor.planeAnchor.transform);
		point.transform.rotation = UnityARMatrixOps.GetRotation (aranchor.planeAnchor.transform);
//		Debug.Log("point new position:"+Point.transform.position);
		};

		OpenCamera();
		#endif
	}

	public bool IsSupported(){
		#if !UNITY_EDITOR
		return ARSession.IsSupported();
		#else
		return false;
		#endif
	}

	public void OpenCamera(){
		#if !UNITY_EDITOR
		if(m_camera){
			SetCamera(m_camera);
			ar_session.start();
		}
		#endif
	}

	public void CloseCamera(){
		#if !UNITY_EDITOR
		SetCamera (null);
		ar_session.stop ();
		#endif
	}

	public void PauseScan(){
		#if !UNITY_EDITOR
		ARVideo unityARVideo = m_camera.gameObject.GetComponent<ARVideo> ();
		unityARVideo.clearPoints ();
		ar_session.clearAnchor ();
		ar_session.pause ();
		#endif
	}

	public void ResumeScan(){
		#if UNITY_IOS && !UNITY_EDITOR
		ARVideo unityARVideo = m_camera.gameObject.GetComponent<ARVideo> ();
		unityARVideo.setPoints ();
		ar_session.resume ();
		#endif
	}

	public bool HitTest (Vector3 touchPoint, GameObject go) {
		#if UNITY_IOS && !UNITY_EDITOR
		var point = Camera.main.ScreenToViewportPoint(touchPoint);
		ARPoint arPoint = new ARPoint {
		x = point.x,
		y = point.y
		};
		return ar_session.hitTest (arPoint, go);
		#else
		return false;
		#endif
	}

	public void SetCamera(Camera newCamera)
	{
		if (m_camera != null) {
			ARVideo oldARVideo = m_camera.gameObject.GetComponent<ARVideo> ();
			if (oldARVideo != null) {
				Destroy (oldARVideo);
			}
		}
		SetupNewCamera (newCamera);
	}

	private void SetupNewCamera(Camera newCamera)
	{
		m_camera = newCamera;

		if (m_camera != null) {
			ARVideo unityARVideo = m_camera.gameObject.GetComponent<ARVideo> ();
			if (unityARVideo != null) {
				Destroy (unityARVideo);
			}
			unityARVideo = m_camera.gameObject.AddComponent<ARVideo> ();
			unityARVideo.m_ClearMaterial = savedClearMaterial;
			unityARVideo.PointCloudPrefab = PointCloud;
			unityARVideo.numPointsToShow = numPpointsToShow;
		}
	}

	// Update is called once per frame

	#if !UNITY_EDITOR
	void Update () {
	if (m_camera != null)
	{

	// JUST WORKS!
	Matrix4x4 matrix = m_session.GetCameraPose();
	m_camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
	m_camera.transform.localRotation = UnityARMatrixOps.GetRotation (matrix);
	m_camera.projectionMatrix = m_session.GetCameraProjection ();
	}

	}
	#endif
}
