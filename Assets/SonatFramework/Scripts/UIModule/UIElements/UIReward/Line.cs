using UnityEngine;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int points = 50;
    public float step = 0.5f;
    public float amplitude = 0.3f;
    public float frequency = 1;
    public float length = 4;
    public float momentSpeed = 20;

    // Start is called before the first frame update
    private void Start()
    {
        //Draw();
    }

    // Update is called once per frame
    private void Update()
    {
        Draw();
    }

    private void Draw()
    {
        float xStart = 0;
        var rad = Mathf.PI * step;
        var xFinish = length;
        lineRenderer.positionCount = points;

        for (var i = 0; i < points; i++)
        {
            var progress = i * 1.0f / (points - 1);
            var x = Mathf.Lerp(xStart, xFinish, progress);
            var y = Mathf.Sin(rad * frequency * x) * Mathf.Sin(Time.time * momentSpeed) * amplitude;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}