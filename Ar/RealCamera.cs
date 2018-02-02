
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RealCamera : MonoBehaviour {
    private WebCamTexture camTexture;
    void Start()
    {
        StartCoroutine(initWebCamera());
    }
    IEnumerator initWebCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (camTexture != null)
                camTexture.Stop();

            if (WebCamTexture.devices.Length > 0)
            {
                camTexture = new WebCamTexture(WebCamTexture.devices[0].name);
            }
            else {
                camTexture = new WebCamTexture();
            }
            camTexture.requestedFPS = 60;
            camTexture.requestedHeight = Screen.height;
            camTexture.requestedWidth = Screen.width;

            GetComponent<Image>().canvasRenderer.SetTexture(camTexture);
            //camTexture.Play();
        }
    }
}

