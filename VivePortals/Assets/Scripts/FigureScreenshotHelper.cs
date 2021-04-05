using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
public class FigureScreenshotHelper : MonoBehaviour {
    private Camera camera;
    public int Width;
    public int Height;
    private RenderTexture targetTexture;

    void Start() {
        camera = GetComponent<Camera>();

        targetTexture = new RenderTexture(Width, Height, 8, RenderTextureFormat.ARGB32, 1);
    }

    void LateUpdate() {
        if(Input.GetKeyDown(KeyCode.F8)) {
            camera.targetTexture = targetTexture;
            StartCoroutine(TakeScreenShot());
        }
    }

    public IEnumerator TakeScreenShot() {
        yield return new WaitForEndOfFrame();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = targetTexture;
        camera.Render();
        Texture2D imageOverview = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB24, false, true);
        imageOverview.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        imageOverview.Apply();
        RenderTexture.active = currentRT;

        // Encode texture into PNG
        byte[] bytes = imageOverview.EncodeToPNG();

        string filename = string.Format("screen_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/" + filename;
        File.WriteAllBytes(path, bytes);

        camera.targetTexture = null;
    }
}