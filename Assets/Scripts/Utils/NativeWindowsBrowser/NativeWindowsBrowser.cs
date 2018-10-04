using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.IO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public class DllTest
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    public static bool GetSaveFileName1([In, Out] OpenFileName ofn)
    {
        return GetSaveFileName(ofn);
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();
}

public class NativeWindowsBrowser : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] RectTransform _drawingArea;
    [SerializeField] CanvasScaler _scaler;
    [SerializeField] Transform canvas;

    int width;
    int height;
    Vector3[] corners;

    private void Start()
    {
        if(_camera == null)
        _camera = FindObjectOfType<Camera>();
        corners = new Vector3[4];
        _drawingArea.GetWorldCorners(corners);
    }

    public void Open()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.dlgOwner = DllTest.GetActiveWindow(); // makes the window modal, but also causes flicker, solved by two WaitForEndOfFrame in coroutine
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "PNG Files\0*.png\0All Files\0*.*\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = Environment.CurrentDirectory;

        ofn.title = "Open Project";
        ofn.defExt = "PNG";
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR

        if (DllTest.GetSaveFileName(ofn))
        {
            StartCoroutine(TakeScreenShot(ofn));
        }
    }

    public IEnumerator TakeScreenShot(OpenFileName ofn)
    {        
        yield return new WaitForEndOfFrame(); // it must be a coroutine 
        yield return new WaitForEndOfFrame();

        Program.Instance.HideAllBoundingBoxes();
        Program.Instance.AllowPathObjectUpdate();

        ScalableObject drawArea = Program.Instance.GetAreaObject();
        Vector2 minPosition = drawArea.DownLeftAnchor;
        minPosition = canvas.TransformPoint(minPosition);
        Vector3 minPosition3 = new Vector3(minPosition.x, minPosition.y, corners[0].z);
        minPosition = _camera.WorldToScreenPoint(minPosition3);

        Vector2 maxPosition = drawArea.UpRightAnchor;
        maxPosition = canvas.TransformPoint(maxPosition);
        Vector3 maxPosition3 = new Vector3(maxPosition.x, maxPosition.y, corners[2].z);
        maxPosition = _camera.WorldToScreenPoint(maxPosition3);

        Vector2 size = maxPosition - minPosition;

        Texture2D snapShot = new Texture2D((int)size.x-2, (int)size.y, TextureFormat.ARGB32, false);
        RenderTexture snapShotRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        RenderTexture.active = snapShotRT;
        _camera.targetTexture = snapShotRT;
        _camera.Render();

        snapShot.ReadPixels(new Rect((int)minPosition.x+2, Screen.height - (int)maxPosition.y, Screen.width, Screen.height), 0, 0);

        snapShot.Apply();
        RenderTexture.active = null;
        _camera.targetTexture = null;

        var bytes = snapShot.EncodeToPNG();
        Destroy(snapShot);
        File.WriteAllBytes(ofn.file, bytes);
    }

}
