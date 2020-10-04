using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DragAndDropController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float raycastDistance = 100f;

    public DragAndDropListener HeldObject { get; private set; }

    private void Update()
    {
        if (HeldObject != null)
            HeldObject.Drag(GetMousePosition());
    }

    public void OnClickDown()
    {
        Collider2D collider = Physics2D.OverlapPoint(GetMousePosition());

        if (collider == null)
            return;
        if (!collider.TryGetComponent<DragAndDropListener>(out var piece))
            return;

        HeldObject = piece;
        HeldObject.OnClickDown(GetMousePosition());
    }

    public void OnClickUp()
    {
        HeldObject = null;
    }

    private Vector3 GetMousePosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mousePosDepth = new Vector3(mousePos.x, mousePos.y, raycastDistance);
        return Camera.main.ScreenToWorldPoint(mousePosDepth);
    }
}
