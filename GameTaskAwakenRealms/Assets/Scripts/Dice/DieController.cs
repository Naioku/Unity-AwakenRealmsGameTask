using System;
using System.Collections;
using System.Collections.Generic;
using InteractionSystem;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dice
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshRenderer))]
    public class DieController : MonoBehaviour, IDraggable
    {
        private const float AngleDegreesTolerance = 1f;
    
        [SerializeField] private Interaction interaction;
        
        [Header("Markers")]
        [SerializeField] private string sideMarkersParentName = "Side markers";
        [SerializeField, HideInInspector] private Transform sideMarkersParent;
        [SerializeField] private GameObject sidePrefab;
        [SerializeField] private List<SideData> sidesData;

        [Header("Drag'n'throw")]
        [Tooltip("Minimal velocity below which the Die is considered to be idle.")]
        [SerializeField] private float minRollingVelocity = 0.1f;
        [Tooltip("Minimal velocity when the Die is considered to be in the throw state.")]
        [SerializeField] private float minThrowVelocity = 10f;
        [SerializeField] private LayerMask groundLayer = 1 << 7;
        [SerializeField] private StatesData statesData = new()
        {
            drag = new Drag
            {
                mass = 0.1f,
                drag = 100f,
                angularDrag = 200f,
            },
            autoThrow = new AutoThrow
            {
                minVerticalDirection = 0.4f,
                throwForce = 100,
                torqueForce = 50f,
            },
            scoreDetection = new ScoreDetection
            {
                minDotProductPassing = 0.9f,
            }
        };

        private MeshCollider _meshCollider;
        private MeshRenderer _meshRenderer;
        private Color _meshColorBase;
        private Color _meshColorHighlight;
        
        private float _cacheMass;
        private float _cacheDrag;
        private float _cacheAngularDrag;
        
        private Enums.DieState _currentState;
        
        public event Action<Enums.DieState> OnStateChanged;
        public event Action<int> OnScoreDetected;

        public Rigidbody Rigidbody { get; private set; }

        public void Drag() => SwitchState(Enums.DieState.Drag);
        public void Drop() => SwitchState(
                Rigidbody.velocity.magnitude > minThrowVelocity 
                    ? Enums.DieState.Throw 
                    : Enums.DieState.PutDown);

        public void PerformAutoThrow()
        {
            if (_currentState != Enums.DieState.Idle) return;
            
            SwitchState(Enums.DieState.AutoThrow);
        }

        private bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, _meshCollider.bounds.extents.y + 0.1f, groundLayer);

        private void Awake()
        {
            _meshCollider = GetComponent<MeshCollider>();
            Rigidbody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshColorBase = _meshRenderer.material.color;
            _meshColorHighlight = _meshColorBase + new Color(0.1f, 0.1f, 0.1f, 1f);
            
            _cacheMass = Rigidbody.mass;
            _cacheDrag = Rigidbody.drag;
            _cacheAngularDrag = Rigidbody.angularDrag;
            
            InitInteraction();
        }

        private void Start() => SwitchState(Enums.DieState.PutDown);

        private void InitInteraction()
        {
            if (interaction == null)
            {
                Debug.LogError("No interaction set.");
                return;
            }

            interaction.Owner = this;
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.EnterInteraction, HandleHoverEnter);
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.EnterType, HandleHoverEnter);
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.ExitInteraction, HandleHoverExit);
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.ExitType, HandleHoverExit);
        }

        private void HandleHoverEnter(InteractionDataArgs obj) => _meshRenderer.material.color = _meshColorHighlight;
        private void HandleHoverExit(InteractionDataArgs obj) => _meshRenderer.material.color = _meshColorBase;

        private void SwitchState(Enums.DieState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
            switch (newState)
            {
                case Enums.DieState.Idle:
                    Debug.Log("Idle");
                    Rigidbody.isKinematic = true;
                    interaction.Enabled = true;
                    break;
                
                case Enums.DieState.Drag:
                    Debug.Log("Drag");
                    interaction.Enabled = false;
                    Rigidbody.isKinematic = false;
                    Rigidbody.mass = statesData.drag.mass;
                    Rigidbody.drag = statesData.drag.drag;
                    Rigidbody.angularDrag = statesData.drag.angularDrag;
                    break;
                
                case Enums.DieState.AutoThrow:
                    Debug.Log("Auto throw");
                    interaction.Enabled = false;
                    Rigidbody.isKinematic = false;
                    
                    var randomForce = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(statesData.autoThrow.minVerticalDirection, 1f),
                        Random.Range(-1f, 1f)).normalized;
                    
                    var randomTorque = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f)).normalized;
                    
                    Rigidbody.AddForce(randomForce * statesData.autoThrow.throwForce, ForceMode.Impulse);
                    Rigidbody.AddTorque(randomTorque * statesData.autoThrow.torqueForce, ForceMode.Impulse);
                    RunDelayedAction(() => SwitchState(Enums.DieState.Throw), 0.5f);
                    break;
                
                case Enums.DieState.Throw:
                    Debug.Log("Throw");
                    RestoreRigidbodySettings();
                    Managers.Instance.UpdateRegistrar.RegisterOnFixedUpdate(TrySwitchingStateToScoreDetection);
                    break;
                
                case Enums.DieState.PutDown:
                    Debug.Log("Put down");
                    interaction.Enabled = false;
                    Rigidbody.velocity = Vector3.zero;
                    RestoreRigidbodySettings();
                    Managers.Instance.UpdateRegistrar.RegisterOnFixedUpdate(TrySwitchingStateToIdle);
                    break;
                
                case Enums.DieState.ScoreDetection:
                    Debug.Log("Score calculation");
                    SideData? result = null;
                    foreach (SideData sideData in sidesData)
                    {
                        if (Vector3.Dot(sideData.ForwardVector, Vector3.up) > statesData.scoreDetection.minDotProductPassing)
                        {
                            result = sideData;
                            break;
                        }
                    }
                    
                    if (result == null)
                    {
                        SwitchState(Enums.DieState.AutoThrow);
                        break;
                    }
                    
                    OnScoreDetected?.Invoke(result.Value.Number);
                    SwitchState(Enums.DieState.Idle);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void RunDelayedAction(Action action, float delay)
        {
            StartCoroutine(Run());
            return;
            
            IEnumerator Run()
            {
                yield return new WaitForSeconds(delay);
                action.Invoke();
            }
        }

        private void RestoreRigidbodySettings()
        {
            Rigidbody.mass = _cacheMass;
            Rigidbody.drag = _cacheDrag;
            Rigidbody.angularDrag = _cacheAngularDrag;
        }

        private void TrySwitchingStateToScoreDetection()
        {
            if (!IsGrounded) return;
            if (Rigidbody.velocity.magnitude > minRollingVelocity) return;
            
            Managers.Instance.UpdateRegistrar.UnregisterFromFixedUpdate(TrySwitchingStateToScoreDetection);
            SwitchState(Enums.DieState.ScoreDetection);
        }        
        
        private void TrySwitchingStateToIdle()
        {
            if (!IsGrounded) return;
            if (Rigidbody.velocity.magnitude > minRollingVelocity) return;
            
            Managers.Instance.UpdateRegistrar.UnregisterFromFixedUpdate(TrySwitchingStateToIdle);
            SwitchState(Enums.DieState.Idle);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sidesData == null) return;
            UpdateSides();
        }

        [ContextMenu("Generate side markers")]
        private void CMGenerateSideMarkers()
        {
            if (!_meshCollider)
            {
                Debug.LogError("No mesh collider set.");
                return;
            }

            if (!_meshCollider.sharedMesh)
            {
                Debug.LogError("Mesh is not set in the collider.");
                return;
            }
        
            if (!sidePrefab)
            {
                Debug.LogError("No side prefab set.");
                return;
            }

            List<Triangle> triangleData = CreateTriangleData(_meshCollider.sharedMesh);
            List<List<Triangle>> sides = CreateSides(triangleData);
        
            if (sideMarkersParent)
            {
                CleanExistingMarkers();
            }

            GenerateNewMarkers(sides);
            EditorUtility.SetDirty(this);

            Debug.Log($"{sides.Count} markers generated.");
        }

        private List<Triangle> CreateTriangleData(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            var triangleData = new List<Triangle>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = transform.TransformPoint(vertices[triangles[i]]);
                Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]]);

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                Vector3 center = (v0 + v1 + v2) / 3f;

                triangleData.Add(new Triangle { center = center, normal = normal });
            }

            return triangleData;
        }

        private static List<List<Triangle>> CreateSides(List<Triangle> triangleData)
        {
            List<List<Triangle>> sideGroups = new List<List<Triangle>>();
            foreach (Triangle checkedTriangle in triangleData)
            {
                bool added = false;
                foreach (List<Triangle> group in sideGroups)
                {
                    Triangle firstTriangleFromGroup = group[0];
                    float angle = Vector3.Angle(firstTriangleFromGroup.normal, checkedTriangle.normal);
                    if (angle > AngleDegreesTolerance) continue;
                
                    group.Add(checkedTriangle);
                    added = true;
                    break;
                }

                if (!added)
                {
                    sideGroups.Add(new List<Triangle> { checkedTriangle });
                }
            }
        
            return sideGroups;
        }

        private void CleanExistingMarkers()
        {
            DestroyImmediate(sideMarkersParent.gameObject);
            sidesData.Clear();
        }

        private void GenerateNewMarkers(List<List<Triangle>> sides)
        {
            sideMarkersParent = new GameObject(sideMarkersParentName).transform;
            sideMarkersParent.transform.SetParent(transform);
            sideMarkersParent.transform.position = transform.position;
            sideMarkersParent.transform.rotation = transform.rotation;
        
            int sideIndex = 1;
            foreach (List<Triangle> side in sides)
            {
                Vector3 averagePos = Vector3.zero;
                Vector3 averageNormal = Vector3.zero;

                foreach (Triangle triangle in side)
                {
                    averagePos += triangle.center;
                    averageNormal += triangle.normal;
                }

                averagePos /= side.Count;
                averageNormal.Normalize();
            
                GameObject marker = (GameObject)PrefabUtility.InstantiatePrefab(sidePrefab, sideMarkersParent);
                marker.name += $"_{sideIndex}";
                marker.transform.position = averagePos;
                marker.transform.rotation = Quaternion.LookRotation(averageNormal);
                sidesData.Add(new SideData(marker.GetComponent<Side>(), sideIndex));
                sideIndex++;
            }

            UpdateSides();
        }

        private void UpdateSides()
        {
            foreach (SideData side in sidesData)
            {
                side.Update();
            }
        }

        private struct Triangle
        {
            public Vector3 center;
            public Vector3 normal;
        }
    
        [Serializable]
        private struct SideData
        {
            [SerializeField] private Side side;
            [SerializeField] private int number;

            public SideData(Side side, int number)
            {
                this.side = side;
                this.number = number;
            }
            
            public Vector3 ForwardVector => side.transform.forward;
            public int Number => number;
        
            public void Update() => side.Number = number;
        }
#endif
    }
}
