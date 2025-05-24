using System.Collections.Generic;
using InteractionSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dice
{
    public class Die : MonoBehaviour
    {
        private const float AngleDegreesTolerance = 1f;
    
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private string sideMarkersParentName = "Side markers";
        [SerializeField, HideInInspector] private Transform sideMarkersParent;
        [SerializeField] private GameObject sidePrefab;
        [SerializeField] private List<SideData> sidesData;
        [SerializeField] private Interaction interaction;

        private MeshRenderer _meshRenderer;
        private Color _meshColorBase;
        private Color _meshColorHighlight;
        
        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshColorBase = _meshRenderer.material.color;
            _meshColorHighlight = _meshColorBase + new Color(0.1f, 0.1f, 0.1f, 1f);
            
            InitInteraction();
        }

        private void InitInteraction()
        {
            if (interaction == null)
            {
                Debug.LogError("No interaction set.");
                return;
            }

            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.EnterInteraction, HandleHoverEnter);
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.EnterType, HandleHoverEnter);
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.ExitInteraction, HandleHoverExit);
            interaction.SetAction(Enums.InteractionType.Hover, Enums.InteractionState.ExitType, HandleHoverExit);
            
            interaction.SetAction(Enums.InteractionType.Click, Enums.InteractionState.EnterType, HandleClickEnter);
            interaction.SetAction(Enums.InteractionType.Click, Enums.InteractionState.ExitType, HandleClickExit);
            interaction.SetAction(Enums.InteractionType.Click, Enums.InteractionState.ExitInteraction, HandleClickExit);
        }

        private void HandleHoverEnter(InteractionDataArgs obj) => _meshRenderer.material.color = _meshColorHighlight;
        private void HandleHoverExit(InteractionDataArgs obj) => _meshRenderer.material.color = _meshColorBase;

        private void HandleClickEnter(InteractionDataArgs obj)
        {
            Debug.Log("ClickEnter");
        }

        private void HandleClickExit(InteractionDataArgs obj)
        {
            Debug.Log("ClickExit");
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
            if (!meshCollider)
            {
                Debug.LogError("No mesh collider set.");
                return;
            }

            if (!meshCollider.sharedMesh)
            {
                Debug.LogError("Mesh is not set in the collider.");
                return;
            }
        
            if (!sidePrefab)
            {
                Debug.LogError("No side prefab set.");
                return;
            }

            List<Triangle> triangleData = CreateTriangleData(meshCollider.sharedMesh);
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
    
        [System.Serializable]
        private struct SideData
        {
            [SerializeField] private Side side;
            [SerializeField] private string number;

            public SideData(Side side, string number)
            {
                this.side = side;
                this.number = number;
            }
        
            public void Update() => side.Number = number;
        }
#endif
    }
}
