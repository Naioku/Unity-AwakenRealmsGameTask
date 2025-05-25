using UnityEngine;

[System.Serializable]
public class DragNDropController
{
    [SerializeField] private float draggingHeight = 2f;
    [SerializeField] private GameObject draggingHandlePrefab;

    private Camera _mainCamera;
    private Plane _plane;
    
    private Transform _draggingHandle;
    private SpringJoint _draggingHandleSpringJoint;
    private Rigidbody _draggingObjectRigidbody;
    private float _cacheDraggingObjectMass;
    private float _cacheDraggingObjectDrag;
    private float _cacheDraggingObjectAngularDrag;
    
    private bool _isDragging;

    public void Initialize(Camera mainCamera)
    {
        _plane = new Plane(Vector3.up, new Vector3(0, draggingHeight, 0));
        _mainCamera = mainCamera;
        _draggingHandle = Object.Instantiate(draggingHandlePrefab).transform;
        _draggingHandleSpringJoint = _draggingHandle.GetComponent<SpringJoint>();
        _draggingHandle.gameObject.SetActive(false);
    }
    
    public void Drag(IDraggable draggedObject)
    {
        _draggingHandle.gameObject.SetActive(true);
        _draggingObjectRigidbody = draggedObject.Rigidbody;
        _cacheDraggingObjectMass = _draggingObjectRigidbody.mass;
        _cacheDraggingObjectDrag = _draggingObjectRigidbody.drag;
        _cacheDraggingObjectAngularDrag = _draggingObjectRigidbody.angularDrag;
        _draggingObjectRigidbody.mass = 0.1f;
        _draggingObjectRigidbody.drag = 100f;
        _draggingObjectRigidbody.angularDrag = 200f;
        _draggingHandleSpringJoint.connectedBody = _draggingObjectRigidbody;
        Managers.Instance.UpdateRegistrar.RegisterOnUpdate(MoveDraggedObject);
        _isDragging = true;
    }

    public void Drop()
    {
        if (!_isDragging) return;
        
        Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(MoveDraggedObject);
        _draggingHandleSpringJoint.connectedBody = null;
        _draggingObjectRigidbody.mass = _cacheDraggingObjectMass;
        _draggingObjectRigidbody.drag = _cacheDraggingObjectDrag;
        _draggingObjectRigidbody.angularDrag = _cacheDraggingObjectAngularDrag;
        _draggingObjectRigidbody = null;
        _draggingHandle.gameObject.SetActive(false);
        _isDragging = false;
    }

    private void MoveDraggedObject()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Managers.Instance.InputManager.CursorPosition);
        if (!_plane.Raycast(ray, out var distance)) return;
     
        _draggingHandle.position = ray.GetPoint(distance);
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
}