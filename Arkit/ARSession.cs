using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ARSession {

	private Dictionary<string, ARPlaneAnchorGameObject> planeAnchorMap;
	private UnityARAnchorManager unityARAnchorManager;

	public Action<ARPlaneAnchorGameObject> addAnchor = null;
	public Action<ARPlaneAnchorGameObject> updateAnchor = null;
	private bool isPause = false;

	public ARSession ()
	{
		planeAnchorMap = new Dictionary<string,ARPlaneAnchorGameObject> ();
//		UnityARSessionNativeInterface.ARAnchorAddedEvent += AddAnchorEvent;
//		UnityARSessionNativeInterface.ARAnchorUpdatedEvent += UpdateAnchorEvent;
//		UnityARSessionNativeInterface.ARAnchorRemovedEvent += RemoveAnchorEvent;
	}

	public static bool IsSupported()
	{
		return new ARKitWorldTrackingSessionConfiguration().IsSupported;
	}

	public void stop(){
		UnityARSessionNativeInterface.GetARSessionNativeInterface ().Pause ();
    }

	public void run(UnityARSessionRunOption runOpts, ARKitWorldTrackingSessionConfiguration config)
    {
		if(!config.IsSupported)
			return ;
        UnityARSessionNativeInterface session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        session.RunWithConfigAndOptions(config, runOpts);
    }

    public void start(){
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.planeDetection = UnityARPlaneDetection.Horizontal;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = false;
		config.enableLightEstimation = true;
        run(UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking, config);
	}

	public void pause(){
		isPause = true;
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.planeDetection = UnityARPlaneDetection.None;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = false;
		config.enableLightEstimation = false;
        run(UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors, config);
	}

	public void resume(){
		isPause = false;
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.planeDetection = UnityARPlaneDetection.Horizontal;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = false;
		config.enableLightEstimation = true;
        run(0, config);
	}

	public void clearAnchor()
    {
        foreach (ARPlaneAnchorGameObject arpag in GetCurrentPlaneAnchors())
        {
            GameObject.Destroy(arpag.gameObject);
        }

        planeAnchorMap.Clear();
    }


	public void AddAnchorEvent(ARPlaneAnchor arPlaneAnchor)
	{
		if (isPause)
			return;
		ARPlaneAnchorGameObject arpag = new ARPlaneAnchorGameObject ();
		arpag.planeAnchor = arPlaneAnchor;
		if (addAnchor!=null)
			addAnchor (arpag);
		planeAnchorMap.Add (arPlaneAnchor.identifier, arpag);
	}

	public void RemoveAnchorEvent(ARPlaneAnchor arPlaneAnchor)
	{
		if (isPause)
			return;
		if (planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) {
			ARPlaneAnchorGameObject arpag = planeAnchorMap [arPlaneAnchor.identifier];
			GameObject.Destroy (arpag.gameObject);
			planeAnchorMap.Remove (arPlaneAnchor.identifier);
		}
	}

	public void UpdateAnchorEvent(ARPlaneAnchor arPlaneAnchor)
	{
		if (isPause)
			return;
		if (planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) {
			ARPlaneAnchorGameObject arpag = planeAnchorMap [arPlaneAnchor.identifier];
			arpag.planeAnchor = arPlaneAnchor;
			if (updateAnchor!= null)
				updateAnchor (arpag);
			planeAnchorMap [arPlaneAnchor.identifier] = arpag;
		}
	}

	public void Destroy()
	{
        clearAnchor();
	}

	public List<ARPlaneAnchorGameObject> GetCurrentPlaneAnchors()
	{
		return planeAnchorMap.Values.ToList();
	}

	public bool hitTest (ARPoint point, GameObject go)
	{
		// prioritize reults types
		ARHitTestResultType[] resultTypes = {
			ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
			// if you want to use infinite planes use this:
			ARHitTestResultType.ARHitTestResultTypeExistingPlane,
			ARHitTestResultType.ARHitTestResultTypeHorizontalPlane, 
			ARHitTestResultType.ARHitTestResultTypeFeaturePoint
		}; 

		UnityARSessionNativeInterface session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
		foreach (ARHitTestResultType resultType in resultTypes)
		{
			List<ARHitTestResult> hitResults = session.HitTest (point, resultType);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					go.transform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
					go.transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
//					Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", go.transform.position.x, go.transform.position.y, go.transform.position.z));
					return true;
				}
			}
		}

		return false;
	}
}

