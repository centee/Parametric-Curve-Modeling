using UnityEngine;

public class DragPoint : MonoBehaviour
{
    private bool isDrag;
    Camera cam;
    void Start()
    {
        cam = Camera.main;
    }
    void OnMouseDown() => isDrag = true;
    void OnMouseUp() => isDrag = false;

    void Update()
    {
        if (!isDrag) return;
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouseWorld.x, mouseWorld.y, 0);
    }
}
