using UnityEngine;

[System.Serializable]
public class DragNDropController
{
    [SerializeField] private float draggingHeight = 2f;
    [SerializeField] private GameObject draggingHandlePrefab;
    [SerializeField] private Transform draggingArea;

    private Camera _mainCamera;
    private Plane _plane;
    
    private Transform _draggingHandle;
    private SpringJoint _draggingHandleSpringJoint;
    private IDraggable _draggingObject;
    
    private bool _isDragging;

    public void Initialize(Camera mainCamera)
    {
        _plane = new Plane(Vector3.up, new Vector3(0, draggingHeight, 0));
        _mainCamera = mainCamera;
        _draggingHandle = Object.Instantiate(draggingHandlePrefab).transform;
        _draggingHandleSpringJoint = _draggingHandle.GetComponent<SpringJoint>();
        _draggingHandle.gameObject.SetActive(false);
    }

    public void Destroy()
    {
        if (_isDragging)
        {
            Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(MoveDraggedObject);
        }
        
        if (_draggingHandle)
        {
            Object.Destroy(_draggingHandle.gameObject);
        }
    }

    public void Drag(IDraggable draggedObject)
    {
        _draggingHandle.gameObject.SetActive(true);
        _draggingObject = draggedObject;
        _draggingObject.Drag();
        _draggingHandleSpringJoint.connectedBody = _draggingObject.Rigidbody;
        Managers.Instance.UpdateRegistrar.RegisterOnUpdate(MoveDraggedObject);
        _isDragging = true;
    }

    public void Drop()
    {
        if (!_isDragging) return;
        
        Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(MoveDraggedObject);
        _draggingHandleSpringJoint.connectedBody = null;
        _draggingObject.Drop(HandleScoreCalculated);
        _draggingObject = null;
        _draggingHandle.gameObject.SetActive(false);
        _isDragging = false;
    }

    private void MoveDraggedObject()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Managers.Instance.InputManager.CursorPosition);
        if (!_plane.Raycast(ray, out var distance)) return;
        
        Vector3 point = ray.GetPoint(distance);
        float padding = 2f;
        float halfScaleX = (draggingArea.lossyScale.x - padding) / 2f;
        float halfScaleZ = (draggingArea.lossyScale.z - padding) / 2f;
        point.x = Mathf.Clamp(point.x, draggingArea.position.x - halfScaleX, draggingArea.position.x + halfScaleX);
        point.z = Mathf.Clamp(point.z, draggingArea.position.z - halfScaleZ, draggingArea.position.z + halfScaleZ);
        
        _draggingHandle.position = point;
    }

    private void HandleScoreCalculated(string score)
    {
        Debug.Log(score);
    }
}