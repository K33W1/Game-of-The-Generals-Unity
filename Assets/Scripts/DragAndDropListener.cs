using UnityEngine;

[DisallowMultipleComponent]
public class DragAndDropListener : MonoBehaviour
{
    private Vector3 originalPosition;
    private Vector2 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    public void OnClickDown(Vector3 mousePosition)
    {
        SavePosition(mousePosition);
    }

    public void Drag(Vector3 mousePosition)
    {
        transform.localPosition = new Vector3()
        {
            x = mousePosition.x - startPosition.x,
            y = mousePosition.y - startPosition.y,
            z = transform.localPosition.z
        };
    }

    public void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;
    }

    private void SavePosition(Vector3 mousePosition)
    {
        originalPosition = transform.position;
        startPosition.x = mousePosition.x - transform.localPosition.x;
        startPosition.y = mousePosition.y - transform.localPosition.y;
    }
}
