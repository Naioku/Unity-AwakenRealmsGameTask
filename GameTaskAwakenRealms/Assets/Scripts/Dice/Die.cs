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
    public class Die : MonoBehaviour, IDraggable
    {
        private const float AngleDegreesTolerance = 1f;
    
        [SerializeField] private Interaction interaction;
        
        [Header("Markers")]
        [SerializeField] private string sideMarkersParentName = "Side markers";
        [SerializeField, HideInInspector] private Transform sideMarkersParent;
        [SerializeField] private GameObject sidePrefab;
        [SerializeField] private List<SideData> sidesData;

        [Header("Drag'n'throw")]
        [SerializeField] private float minRollingVelocity = 0.1f;
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
        private Rigidbody _rigidbody;
        private MeshRenderer _meshRenderer;
        private Color _meshColorBase;
        private Color _meshColorHighlight;
        
        private float _cacheMass;
        private float _cacheDrag;
        private float _cacheAngularDrag;
        
        private State _currentState;
        private Action<string> _onScoreCalculated;

        public Rigidbody Rigidbody => _rigidbody;
        public void Drag() => SwitchState(State.Drag);
        public void Drop(Action<string> onScoreCalculated)
        {
            _onScoreCalculated = onScoreCalculated;
            SwitchState(_rigidbody.velocity.magnitude > minThrowVelocity ? State.Throw : State.PutDown);
        }
        
        public void PerformAutoThrow() => SwitchState(State.AutoThrow);
        
        private bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, _meshCollider.bounds.extents.y + 0.1f, groundLayer);

        private void Awake()
        {
            _meshCollider = GetComponent<MeshCollider>();
            _rigidbody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshColorBase = _meshRenderer.material.color;
            _meshColorHighlight = _meshColorBase + new Color(0.1f, 0.1f, 0.1f, 1f);
            
            _cacheMass = _rigidbody.mass;
            _cacheDrag = _rigidbody.drag;
            _cacheAngularDrag = _rigidbody.angularDrag;
            
            InitInteraction();
        }

        private void Start() => SwitchState(State.PutDown);

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

        private void SwitchState(State newState)
        {
            switch (newState)
            {
                case State.Idle:
                    Debug.Log("Idle");
                    _rigidbody.isKinematic = true;
                    interaction.Enabled = true;
                    break;
                
                case State.Drag:
                    Debug.Log("Drag");
                    interaction.Enabled = false;
                    _rigidbody.isKinematic = false;
                    _rigidbody.mass = statesData.drag.mass;
                    _rigidbody.drag = statesData.drag.drag;
                    _rigidbody.angularDrag = statesData.drag.angularDrag;
                    break;
                
                case State.AutoThrow:
                    Debug.Log("Auto throw");
                    interaction.Enabled = false;
                    _rigidbody.isKinematic = false;
                    
                    var randomForce = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(statesData.autoThrow.minVerticalDirection, 1f),
                        Random.Range(-1f, 1f)).normalized;
                    
                    var randomTorque = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f)).normalized;
                    
                    _rigidbody.AddForce(randomForce * statesData.autoThrow.throwForce, ForceMode.Impulse);
                    _rigidbody.AddTorque(randomTorque * statesData.autoThrow.torqueForce, ForceMode.Impulse);
                    RunDelayedAction(() => SwitchState(State.Throw), 0.5f);
                    break;
                
                case State.Throw:
                    Debug.Log("Throw");
                    RestoreRigidbodySettings();
                    Managers.Instance.UpdateRegistrar.RegisterOnFixedUpdate(TrySwitchingStateToScoreDetection);
                    break;
                
                case State.PutDown:
                    Debug.Log("Put down");
                    interaction.Enabled = false;
                    _rigidbody.velocity = Vector3.zero;
                    RestoreRigidbodySettings();
                    Managers.Instance.UpdateRegistrar.RegisterOnFixedUpdate(TrySwitchingStateToIdle);
                    break;
                
                case State.ScoreDetection:
                    Debug.Log("Score calculation");
                    SideData? result = null;
                    foreach (SideData sideData in sidesData)
                    {
                        Debug.Log($"Dot {Vector3.Dot(sideData.ForwardVector,  Vector3.up)}");
                        if (Vector3.Dot(sideData.ForwardVector, Vector3.up) > statesData.scoreDetection.minDotProductPassing)
                        {
                            result = sideData;
                            break;
                        }
                    }
                    
                    if (result == null)
                    {
                        SwitchState(State.AutoThrow);
                        break;
                    }
                    
                    _onScoreCalculated.Invoke(result.Value.Number);
                    _onScoreCalculated = null;
                    SwitchState(State.Idle);
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
            _rigidbody.mass = _cacheMass;
            _rigidbody.drag = _cacheDrag;
            _rigidbody.angularDrag = _cacheAngularDrag;
        }

        private void TrySwitchingStateToScoreDetection()
        {
            if (!IsGrounded) return;
            if (_rigidbody.velocity.magnitude > minRollingVelocity) return;
            
            Managers.Instance.UpdateRegistrar.UnregisterFromFixedUpdate(TrySwitchingStateToScoreDetection);
            SwitchState(State.ScoreDetection);
        }        
        
        private void TrySwitchingStateToIdle()
        {
            if (!IsGrounded) return;
            if (_rigidbody.velocity.magnitude > minRollingVelocity) return;
            
            Managers.Instance.UpdateRegistrar.UnregisterFromFixedUpdate(TrySwitchingStateToIdle);
            SwitchState(State.Idle);
        }

        private enum State
        {
            Idle,
            Drag,
            AutoThrow,
            Throw,
            PutDown,
            ScoreDetection,
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
                sidesData.Add(new SideData(marker.GetComponent<Side>(), $"{sideIndex}"));
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
            [SerializeField] private string number;

            public SideData(Side side, string number)
            {
                this.side = side;
                this.number = number;
            }
            
            public Vector3 ForwardVector => side.transform.forward;
            public string Number => number;
        
            public void Update() => side.Number = number;
        }
#endif
    }
}
