using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLines : MonoBehaviour {

    public LineRenderer linePrefab;
    private List<GameObject> ListLines = new List<GameObject>();
    private LineRenderer currentLine;
    public int lineVertexIndex = 1;
    public float ScreenZ;
    public float Width = 0.01f;
    public Vector3 cursorScreenPos, cursorWorldPos;

    public GameObject canvas;
    private Text txt_width;
    private int signature_count = 0;

    public AudioSource audioCapture;

    // Use this for initialization
    void Start()
    {
        ScreenZ = Camera.main.nearClipPlane;
        txt_width = canvas.transform.Find("linewidth").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        {
            Width += scroll * 0.01f;
            Width = Mathf.Clamp(Width, 0.001f, 0.05f);
            txt_width.text = Width.ToString();
        }
        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(Input.GetKey(KeyCode.W))
        {
            //因为ugui默认采用Screen Space-Overlay模式，不会camera显示canvas信息
            //canvas.SetActive(false);
            StartCoroutine(CaptureCamera(Camera.main, new Rect(0, 0, Screen.width, Screen.height)));
            audioCapture.Play();
            //canvas.SetActive(true);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            DeleteLastLine();
        }
        if (currentLine == null && Input.GetMouseButton(0))
        {
            currentLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
            currentLine.name = "Line" + ListLines.Count;
            //采用世界坐标系
            currentLine.useWorldSpace = true;
            currentLine.transform.parent = this.transform;
            currentLine.startWidth = currentLine.endWidth = Width;

            cursorScreenPos = Input.mousePosition;
            cursorScreenPos.z = ScreenZ;
            cursorWorldPos = Camera.main.ScreenToWorldPoint(cursorScreenPos);
            //添加2个点是有必要的，为了初始绘制更加稳定
            currentLine.SetPosition(0, cursorWorldPos);
            currentLine.SetPosition(1, cursorWorldPos);
            lineVertexIndex = 2;
            ListLines.Add(currentLine.gameObject);
            StartCoroutine(drawLines());
        }
        if (currentLine && Input.GetMouseButtonUp(0))
        {
            currentLine = null;
        }
    }

    IEnumerator drawLines()
    {
        while (Input.GetMouseButton(0))
        {
            yield return new WaitForEndOfFrame();
            if (currentLine)
            {
                lineVertexIndex++;
                currentLine.positionCount = lineVertexIndex;
                cursorScreenPos = Input.mousePosition;
                cursorScreenPos.z = ScreenZ;
                cursorWorldPos = Camera.main.ScreenToWorldPoint(cursorScreenPos);
                currentLine.SetPosition(lineVertexIndex - 1, cursorWorldPos);
            }
        }
    }

    void DeleteLastLine()
    {
        if (ListLines.Count > 0)
        {
            Destroy(ListLines[ListLines.Count - 1]);
            ListLines.RemoveAt(ListLines.Count - 1);
        }
    }

    IEnumerator CaptureCamera(Camera camera, Rect rect)
    {
        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0); 
        screenShot.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null; 
        GameObject.Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Application.dataPath + "/Signature" 
            + signature_count + ".png";
        signature_count++;
        System.IO.File.WriteAllBytes(filename, bytes);
    }
}
