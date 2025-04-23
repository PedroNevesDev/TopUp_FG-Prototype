using UnityEngine;
using System.IO;

public class PNGCapture : MonoBehaviour
{
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            Capture();
        }   
    }
    public Camera renderCamera;
    public RenderTexture renderTexture;
    public string outputFileName = "CapturedObject.png";

    private Texture2D RenderWithBackground(Color bgColor)
    {
        // Set background color and force camera clear
        renderCamera.backgroundColor = bgColor;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;

        // Set RenderTexture active
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        renderCamera.targetTexture = renderTexture;

        // Render the camera
        renderCamera.Render();

        // Read the pixels
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        // Cleanup
        RenderTexture.active = currentRT;
        return tex;
    }

    public void Capture()
    {
        Texture2D blackTex = RenderWithBackground(Color.black);
        Texture2D whiteTex = RenderWithBackground(Color.white);

        int width = renderTexture.width;
        int height = renderTexture.height;

        Texture2D finalTex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color cBlack = blackTex.GetPixel(x, y);
                Color cWhite = whiteTex.GetPixel(x, y);

                float alpha = 1f - (cWhite.r - cBlack.r);
                alpha = Mathf.Clamp01(alpha);

                // Prevent dividing by zero
                Color finalColor = (alpha > 0.001f) ? cBlack / alpha : Color.clear;
                finalColor.a = alpha;

                finalTex.SetPixel(x, y, finalColor);
            }
        }

        finalTex.Apply();

        // Save PNG
        byte[] bytes = finalTex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, outputFileName);
        File.WriteAllBytes(path, bytes);

        Debug.Log($"✅ Saved PNG to: {path}");
    }
}