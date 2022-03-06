using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Valve.VR;
using DataStructures;
using System;

public class PointerSystem : MonoBehaviour //
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // InputManager must precede InputManager in the script execution order, or the pointer will be transform

    static MaterialPropertyBlock blockPointer;

    GameObject pointersParent;

    string[] surfaceLayers = new string[] { "mainSurface" };
    string[] spriteLayers = new string[] { "sprite" };

    static string shaderString = "Shader Graphs/glow"; // DRP

    public List<Pointer> pointers = new List<Pointer>();

    void Start()
    {
        blockPointer = new MaterialPropertyBlock();
        pointersParent = new GameObject("POINTERS");

        pointers.Add(new Pointer(Pointer.PointerID.left));
        pointers.Add(new Pointer(Pointer.PointerID.right));
    }

    void Update()
    {
        if (AttentionTracker.PointerGlobal.isDisplayPointer)
        {
            if (!pointers[(int)AttentionTracker.PointerGlobal.pointerToUse].TryPointing(spriteLayers))
            {
                pointers[(int)AttentionTracker.PointerGlobal.pointerToUse].TryPointing(surfaceLayers);
            }

            if (AttentionTracker.PointerGlobal.pointerToUse == Pointer.PointerID.left)
                pointers[(int)Pointer.PointerID.right].parent.SetActive(false);
            else
                pointers[(int)Pointer.PointerID.left].parent.SetActive(false);
        }
    }

    // pointers
    [Serializable]
    public class Pointer
    {
        [Serializable]
        public enum PointerID
        {
            left,
            right,
        }

        public PointerID ID;
        public string name;

        public GameObject parent;

        public bool hasPosition = false;
        public Material material = new Material(Shader.Find(shaderString));

        public Transform transform;
        public bool isTracked;

        public string colliderName = "";
        public RaycastHit hit = new RaycastHit();

        public Pointer(PointerID ID)
        {
            this.ID = ID;
            UpdateTracking();
            Configure();
        }

        void UpdateTracking()
        {
            transform = InputManager.controllers[(int)ID].transform;
            isTracked = InputManager.controllers[(int)ID].essentialTransform.isTracked;
        }

        public void Configure()
        {
            parent = new GameObject(); // Make a gameobject that we will put the ring on
            parent.name = name;
            parent.transform.parent = GameObject.Find("POINTERS").GetComponent<Transform>().transform;

            // cylinder
            // https://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
            // https://answers.unity.com/questions/514293/changing-a-gameobjects-primitive-mesh.html

            Mesh cylinderMesh = new Mesh();
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinderMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Destroy(gameObject);

            float cylinderRadius = 0.005f;

            GameObject pointerCylinder = new GameObject(); // We make a offset gameobject to counteract the default cylindermesh pivot/origin being in the middle
            pointerCylinder.name = "pointerCylinder";
            pointerCylinder.transform.parent = parent.transform;
            pointerCylinder.layer = 10; // controller layer not visible to "screenshot" camera

            pointerCylinder.transform.localPosition = new Vector3(0f, 1f, 0f); // Offset the cylinder so that the pivot/origin is at the bottom in relation to the outer ring gameobject.
            pointerCylinder.transform.localScale = new Vector3(cylinderRadius, 1f, cylinderRadius); // Set the radius

            MeshFilter ringMesh = pointerCylinder.AddComponent<MeshFilter>(); // Create the the Mesh and renderer to show the connecting ring
            ringMesh.mesh = cylinderMesh;

            MeshRenderer ringRenderer = pointerCylinder.AddComponent<MeshRenderer>();
            ringRenderer.material = material;
            ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ringRenderer.receiveShadows = false;

            ringRenderer.SetPropertyBlock(blockPointer);
            parent.SetActive(false);
        }

        // Raycast
        public static (string, RaycastHit) RaycastDRP(string[] layerNames, Transform t)
        {
            int layerMask = LayerMask.GetMask(layerNames);
            string colliderName = "";

            Ray ray = new Ray(t.position, t.forward); // Ray from the controller
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask)) // hit
            {
                colliderName = hit.collider.gameObject.name;
            }
            return (colliderName, hit);
        }

        public bool TryPointing(string[] layerNames)
        {
            UpdateTracking();

            if (!isTracked)
            {
                parent.SetActive(false);
                return false;
            }

            (colliderName, hit) = RaycastDRP(layerNames, transform);

            if (colliderName != "")
            {
                hasPosition = true;

                // cylinder
                parent.transform.position = hit.point; // Move the ring to the point
                float cylinderDistance = 0.5f * Vector3.Distance(transform.position, hit.point); // Match the scale to the distance
                parent.transform.localScale = new Vector3(parent.transform.localScale.x, cylinderDistance, parent.transform.localScale.z);
                parent.transform.LookAt(transform.position, Vector3.up); // Make the cylinder look at the main point.
                parent.transform.rotation *= Quaternion.Euler(90, 0, 0); // Since the cylinder is pointing up(y) and the forward is z, we need to offset by 90 degrees.

            }
            else
            {
                hasPosition = false;
            }
            parent.SetActive(hasPosition);
            return hasPosition;
        }
    }
}
