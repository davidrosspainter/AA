using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Valve.VR;
using DataStructures;
using System;
using VacuumShaders.TheAmazingWireframeShader;

public class PointerSystemDemo : MonoBehaviour //
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // InputManager must precede InputManager in the script execution order, or the pointer will be transform

    Camera myCamera;

    static MaterialPropertyBlock blockPointer;

    GameObject pointersParent;

    string[] surfaceLayers = new string[] { "mainSurface" };
    string[] spriteLayers = new string[] { "sprite" };

    static string shaderString = "Shader Graphs/glow"; // DRP

    public List<Pointer> pointers = new List<Pointer>();
    public static List<GameObject> raycastSources = new List<GameObject>();

    public Material surfaceMaterial;
    Vector3 origin = new Vector3(0.009703472f, 1.242f, -0.213f);
    float pointerRadius = 1.5f;

    void Start()
    {
        myCamera = GameObject.Find("Cameras/thirdPersonFar").GetComponent<Camera>();

        blockPointer = new MaterialPropertyBlock();
        blockPointer.SetColor("Fresnel_Color", Color.red);

        pointersParent = new GameObject("POINTERS");

        raycastSources.Add(GameObject.Find("VR/HMD_preconfigured"));
        raycastSources.Add(GameObject.Find("VR/vr_controller_vive_1_5"));
        raycastSources.Add(GameObject.Find("VR/eye"));

        pointers.Add(new Pointer(Pointer.PointerID.headset));
        pointers.Add(new Pointer(Pointer.PointerID.controller));
        pointers.Add(new Pointer(Pointer.PointerID.eye));

        var icosphere = new IcoSphere(3, 1.5f, surfaceMaterial, "isosphere");
        icosphere.gameObject.transform.position = origin;
        Mesh mesh = WireframeGenerator.Generate(icosphere.meshFilter.mesh);
        icosphere.meshFilter.mesh = mesh;


        GameObject pointerSurface = new GameObject("pointerSurface");
        pointerSurface.name = "pointerSurface";
        pointerSurface.layer = LayerMask.NameToLayer("mainSurface");
        pointerSurface.AddComponent<MeshFilter>().mesh = IcoSphere.CreateMesh(radius: pointerRadius, recursionLevel: 3); // warning hard coded
        Surfaces.InvertMeshNormals(pointerSurface.GetComponent<MeshFilter>().mesh);
        pointerSurface.AddComponent<MeshCollider>();
        pointerSurface.transform.position = origin;
    }

    void Update()
    {
        foreach (var pointer in pointers)
        {
            pointers[(int)pointer.ID].TryPointing(surfaceLayers);
            pointers[(int)pointer.ID].parent.SetActive(true);
            print("here");
        }

        if (Time.frameCount == 1)
        {
            StimulusPlotter.TakeScreenshot(myCamera, "..\\screenshots\\raycastPanel.png");
        }
    }

    // pointers
    [Serializable]
    public class Pointer
    {
        [Serializable]
        public enum PointerID
        {
            headset,
            controller,
            eye
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

        public float transformModifier = 1;
        MaterialPropertyBlock materialPropertyBlock;

        public Pointer(PointerID ID)
        {
            this.ID = ID;

            if (ID == PointerID.controller){
                transformModifier = -1;
            }

            UpdateTracking();
            Configure();
        }

        void UpdateTracking()
        {
            transform = raycastSources[(int)ID].transform;
            isTracked = true;
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

            //ringRenderer.SetPropertyBlock(blockPointer);
            parent.SetActive(false);
        }

        // Raycast
        public static (string, RaycastHit) RaycastDRP(string[] layerNames, Transform t, float transformModifier)
        {
            int layerMask = LayerMask.GetMask(layerNames);
            string colliderName = "";

            Ray ray = new Ray(t.position, t.forward*transformModifier); // Ray from the controller
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

            (colliderName, hit) = RaycastDRP(layerNames, transform, this.transformModifier);

            if (colliderName != "")
            {
                hasPosition = true;

                // cylinder
                parent.transform.position = hit.point; // Move the ring to the point
                float cylinderDistance = 0.5f * Vector3.Distance(transform.position, hit.point); // Match the scale to the distance
                parent.transform.localScale = new Vector3(parent.transform.localScale.x, cylinderDistance, parent.transform.localScale.z);
                parent.transform.LookAt(transform.position, Vector3.up); // Make the cylinder look at the main point.
                parent.transform.rotation *= Quaternion.Euler(90, 0, 0); // Since the cylinder is pointing up(y) and the forward is z, we need to offset by 90 degrees.
                print("hit");

            }
            else
            {
                print("miss");
                hasPosition = false;
            }
            parent.SetActive(hasPosition);
            return hasPosition;
        }
    }
}
