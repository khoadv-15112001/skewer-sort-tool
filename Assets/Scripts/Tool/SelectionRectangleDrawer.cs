using UnityEngine;
/// <summary>
/// Draws a selection rectangle on the left mouse button down & dragging
///
/// You only need an InputReceiverManager that basically tracks
/// - if the left mouse button is currently down and saves it in "LeftMouseButtonDown"
/// - saves the initial click position when mouse button was clicked  and saves it in "InitialMousePositionOnLeftClick"
/// - updates the current mouse position and saves it in "CurrentMousePosition"
///
/// </summary>
public class SelectionRectangleDrawer : MonoBehaviour
{
    //set color via inspector for the selection rectangles filler color
    public Color SelectionRectangleFillerColor;
    //set color via inspector for the selection rectangles border color
    public Color SelectionRectangleBorderColor;
    //set selection rectangles  border thickness
    public int SelectionRectangleBorderThickness = 2;
    private Texture2D _selectionRectangleFiller;
    private Texture2D _selectionRectangleBorder;
    private bool _drawSelectionRectangle;
    private float _x1, _x2, _y1, _y2;
    private Vector2 pos1, pos2;
    void Start()
    {
        _selectionRectangleFiller = new Texture2D(1, 1);
        _selectionRectangleFiller.SetPixel(0, 0, SelectionRectangleFillerColor);
        _selectionRectangleFiller.Apply();
        _selectionRectangleFiller.wrapMode = TextureWrapMode.Clamp;
        _selectionRectangleFiller.filterMode = FilterMode.Point;
        _selectionRectangleBorder = new Texture2D(1, 1);
        _selectionRectangleBorder.SetPixel(0, 0, SelectionRectangleBorderColor);
        _selectionRectangleBorder.Apply();
        _selectionRectangleBorder.wrapMode = TextureWrapMode.Clamp;
        _selectionRectangleBorder.filterMode = FilterMode.Point;
    }

    private Vector3 startPos;
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0)) startPos = Input.mousePosition;
        if (!_drawSelectionRectangle && Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0) && !Mathf.Approximately(Vector2.Distance(Input.mousePosition,
                                                                                                       startPos), 0f))
            _drawSelectionRectangle = true;
        else if (Input.GetMouseButtonUp(0) && _drawSelectionRectangle)
            _drawSelectionRectangle = false;
    }
    private void OnGUI()
    {
        if (_drawSelectionRectangle)
            drawSelectionRectangle();
    }
    private void drawSelectionRectangle()
    {
        pos1 = startPos;
        pos2 = Input.mousePosition;
        //check initial mouse position on X axis versus dragging mouse position
        if (pos1.x < pos2.x)
        {
            _x1 = pos1.x;
            _x2 = pos2.x;
        }
        else
        {
            _x1 = pos2.x;
            _x2 = pos1.x;
        }
        //check initial mouse position on Y axis versus dragging mouse position
        if (pos1.y < pos2.y)
        {
            _y1 = pos1.y;
            _y2 = pos2.y;
        }
        else
        {
            _y1 = pos2.y;
            _y2 = pos1.y;
        }
        //filler
        GUI.DrawTexture(new Rect(_x1, Screen.height - _y1, _x2 - _x1, _y1 - _y2), _selectionRectangleFiller, ScaleMode.StretchToFill);
        //top line
        GUI.DrawTexture(new Rect(_x1, Screen.height - _y1, _x2 - _x1, -SelectionRectangleBorderThickness), _selectionRectangleBorder, ScaleMode.StretchToFill);
        //bottom line
        GUI.DrawTexture(new Rect(_x1, Screen.height - _y2, _x2 - _x1, SelectionRectangleBorderThickness), _selectionRectangleBorder, ScaleMode.StretchToFill);
        //left line
        GUI.DrawTexture(new Rect(_x1, Screen.height - _y1, SelectionRectangleBorderThickness, _y1 - _y2), _selectionRectangleBorder, ScaleMode.StretchToFill);
        //right line
        GUI.DrawTexture(new Rect(_x2, Screen.height - _y1, -SelectionRectangleBorderThickness, _y1 - _y2), _selectionRectangleBorder, ScaleMode.StretchToFill);
        
        ToolManager.Instance.SelectMultipleGrills(new Vector2(_x1, _y1), new Vector2(_x2, _y2));
    }

}