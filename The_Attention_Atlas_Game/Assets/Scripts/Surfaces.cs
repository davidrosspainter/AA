using System.Collections.Generic;
using UnityEngine;
using DataStructures;
using System;

public class Surfaces
{
    [Serializable]
    public class Vertex
    {
        public int optionsNumber;

        public GameObject gameObject;
        public Vector3 position;
        public int index;

        public Options.Coordinates coordinates;
        public float radius;
        public float keepAngleArray;

        public SerializableVector3 angle; // (0-180) angle with respect to origin
        public float distance;

        public Vertex(int optionsNumber = -1, 
                      SerializableVector3 position = null,
                      int index = -1,
                      bool isCreateGameObject = true,
                      Options.Coordinates coordinates = Options.Coordinates.undefined,
                      float radius = float.NaN,
                      float keepAngleArray = float.NaN)
        {
            this.optionsNumber = optionsNumber;
            this.position = position;
            this.index = index;
            this.coordinates = coordinates;
            this.radius = radius;
            this.keepAngleArray = keepAngleArray;

            if (isCreateGameObject)
            {
                gameObject = new GameObject("vertex." + index.ToString());
                gameObject.transform.position = position;
            }
            else
            {
                gameObject = null;
            }

            (angle, distance) = CalculateVertexAngle(position);
        }
        public static (Vector3, float) CalculateVertexAngle(SerializableVector3 position) // 0-180 degrees
        {
            float distance = Vector3.Distance(Vector3.zero, position);

            Vector3 angle = Vector3.zero;

            angle.x = Mathf.Acos(position.x / distance);
            angle.y = Mathf.Acos(position.y / distance);
            angle.z = Mathf.Acos(position.z / distance);

            angle = 57.29578F * angle; // radians to degrees

            return (angle, distance);
        }

        static public Vector3 CalculateVertexPositionFromAngle(SerializableVector3 angle, float distance) // x, y, z angle
        {
            Vector3 position = Vector3.zero;
            position.x = Mathf.Cos((angle.x * Mathf.PI) / 180) * distance;
            position.y = Mathf.Cos((angle.y * Mathf.PI) / 180) * distance;
            position.z = Mathf.Cos((angle.z * Mathf.PI) / 180) * distance;
            return (position);
        }
    }

    [Serializable]
    public class SerializableVertex // same except using SerializableVector3 and excluding game object
    {
        public int optionsNumber;

        public SerializableVector3 position;
        public int index;

        public Options.Coordinates coordinates;
        public float radius;
        public float keepAngleArray;

        public SerializableVector3 angle; // (0-180) angle with respect to origin
        public float distance;

        public SerializableVertex(Vertex vertex)
        {
            optionsNumber = vertex.optionsNumber;
            position = vertex.position;
            index = vertex.index;
            coordinates = vertex.coordinates;
            radius = vertex.radius;
            keepAngleArray = vertex.keepAngleArray;
            angle = vertex.angle;
            distance = vertex.distance;
        }
    }

    [Serializable]
    public class ListSerializableVertex
    {
        public List<SerializableVertex> listSerializableVertex;
        public ListSerializableVertex(List<SerializableVertex> listSerializableVertex)
        {
            this.listSerializableVertex = listSerializableVertex;
        }

        public ListSerializableVertex(List<Vertex> listVertex) // convert to serializable
        {
            listSerializableVertex = MakeSerializable(listVertex);
        }

        public static List<SerializableVertex> MakeSerializable(List<Vertex> listVertex)
        {
            List<SerializableVertex> listSerializableVertex = new List<SerializableVertex>();

            foreach (var vertex in listVertex)
            {
                listSerializableVertex.Add(new SerializableVertex(vertex));
            }
            return listSerializableVertex;
        }
    }

    public static GameObject VisualizeVertices(List<Vertex> vertices)
    {
        float markerScale = .1f;
        GameObject surface = new GameObject("surface");

        foreach (var item in vertices)
        {
            if (item.gameObject == null)
            {
                item.gameObject = new GameObject("vertex." + item.index.ToString());
                item.gameObject.transform.position = item.position;
            }
            item.gameObject.transform.parent = surface.transform;
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.localScale = new Vector3(markerScale, markerScale, markerScale);
            marker.transform.position = item.gameObject.transform.position;
            marker.transform.parent = item.gameObject.transform;          
        }
        return surface;
    }

    static public List<Vertex> GetSurfaceVerticesIcosphere(Options options, bool isCreateGameObject = true)
    {

        Mesh mesh = IcoSphere.CreateMesh(options.radius, options.recursionLevel);
        List<Vertex> vertices = new List<Vertex>();

        int index = 0;
      
        foreach (var position in mesh.vertices)
        {
            Vector3 angle;
            float distance;

            (angle, distance) = Vertex.CalculateVertexAngle(position);

            if (angle.z <= options.keepAngleArray)
            {
                vertices.Add(new Vertex(options.optionsNumber, position, index, isCreateGameObject, coordinates: options.coordinates, radius: options.radius, keepAngleArray: options.keepAngleArray));
                index++;
            }
        }

        return vertices;
    }

    public enum LookDirection
    {
        None,
        Inward,
        Outward,
        Origin
    }

    public static void LookAtDirection(Vector3 origin, Transform transform, LookDirection lookDirection)
    {
        switch (lookDirection)
        {
            case LookDirection.Inward: transform.LookAt(transform.position * 2); break;
            case LookDirection.Outward: transform.LookAt(new Vector3(0, 0, 0)); break;
            case LookDirection.Origin: transform.LookAt(origin); break;
            case LookDirection.None: break;
            default: break; // needed?
        }
    }

    public static List<Vertex> GetCuePosition(float radius)
    {
        SphericalCoordinates.Coordinate coordinate = new SphericalCoordinates.Coordinate(latitude: 0, longitude: 0, radius: radius, ID: 0);
        List<Vertex> vertices = new List<Vertex>() { new Vertex(position: coordinate.position, index: 0) };
        return vertices;
    }

    public static GameObject Build(Observer observer, GameRunner.TrialData trialData, bool isCue = false)
    {  
        // ---- generate surface
        GameObject surface = new GameObject("surface");
        GameObject vertexGroup = new GameObject("vertexGroup");
        vertexGroup.transform.parent = surface.transform;

        List<Vertex> vertices;

        Debug.LogFormat("trialData.options.coordinates == Options.Coordinates.icosphere:{0}", trialData.options.coordinates == Options.Coordinates.icosphere);
        Debug.LogFormat("isCue:{0}", isCue);

        if (isCue)
        {
            vertices = GetCuePosition(trialData.options.radius);
        }
        else
        {
            if (trialData.options.coordinates == Options.Coordinates.icosphere) // icosphere
            {
                vertices = GetSurfaceVerticesIcosphere(trialData.options);
            }
            else // spherical coordinates
            {
                vertices = SphericalCoordinates.GetSurfaceVerticesSpherical(trialData.options);
            }
        }

        Debug.LogFormat("vertices.Count:{0}", vertices.Count);

        void DebugElement(string name, GameRunner.TrialData.Element element, Vertex vertex)
        {
            Debug.LogFormat("{0}:, vertex.index:{1}, element.spriteIndex:{2}", name, vertex.index, element.spriteIndex);
        }

        // ----- assign elements
        trialData.elements = new GameRunner.ListElements();
 
        if (isCue) // cue
        {
            if (trialData.options.stimulus == Options.Stimulus.food || trialData.options.stimulus == Options.Stimulus.specOrbs)
            {
                var target = Stimuli.GenerateElement.Target(vertexGameObject: vertices[0].gameObject, vertexIndex: vertices[0].index, position: vertices[0].position, stimulus: trialData.options.stimulus, searchMode: trialData.options.searchMode);
                target.spriteIndex = GameRunner.currentTrialData.targetSpriteIndex;
                Debug.LogFormat("GameRunner.currentTrialData.targetSpriteIndex:{0}", GameRunner.currentTrialData.targetSpriteIndex);
                trialData.elements.listElements.Add(target);
                DebugElement("cue_target", trialData.elements.listElements[trialData.elements.listElements.Count - 1], vertices[0]);
            }
            else
            {
                trialData.elements.listElements.Add(Stimuli.GenerateElement.Target(vertexGameObject: vertices[0].gameObject, vertexIndex: vertices[0].index, position: vertices[0].position, stimulus: trialData.options.stimulus, searchMode: trialData.options.searchMode));
            }

            
            DebugElement("Cue", trialData.elements.listElements[trialData.elements.listElements.Count - 1], vertices[0]);
        }
        else // array
        {
            foreach (Vertex vertex in vertices)
            {
                if (vertex.index == trialData.targetPosition)
                {
                    var target = Stimuli.GenerateElement.Target(vertexGameObject: vertex.gameObject, vertexIndex: vertex.index, position: vertex.position, stimulus: trialData.options.stimulus, searchMode: trialData.options.searchMode);
                    target.spriteIndex = GameRunner.currentTrialData.targetSpriteIndex;
                    Debug.LogFormat("GameRunner.currentTrialData.targetSpriteIndex:{0}", GameRunner.currentTrialData.targetSpriteIndex);
                    trialData.elements.listElements.Add(target);
                    DebugElement("Target", target, vertex);
                }
                else
                {
                    trialData.elements.listElements.Add(Stimuli.GenerateElement.Distractor(vertexGameObject: vertex.gameObject, vertexIndex: vertex.index, position: vertex.position, stimulus: trialData.options.stimulus, searchMode: trialData.options.searchMode));
                    DebugElement("Distractor", trialData.elements.listElements[trialData.elements.listElements.Count - 1], vertex);
                }
            }
        }

        Debug.LogFormat("trialData.elements.Count:{0}", trialData.elements.listElements.Count);

        // ----- display sprites
        int index = 0;

        foreach (GameRunner.TrialData.Element element in trialData.elements.listElements)
        {
            element.vertexGameObject.gameObject.transform.parent = vertexGroup.transform;
            GameObject spriteGameObject = Stimuli.SpawnStimulus(element, trialData.options.stimulus);
            LookAtDirection(observer.origin, spriteGameObject.transform, LookDirection.Inward);
            spriteGameObject.transform.Rotate(new Vector3(0, 0, trialData.elements.listElements[index].rotation));
            spriteGameObject.transform.parent = element.vertexGameObject.transform;
            index++;
        }

        SetPointerSurface(surface);
        TranslateAndRotateSurface(surface, CentralMemory.observer.origin, trialData.options.coordinates, observer.orientation, isCue);
        return surface;
    }

   

 

    public static void TranslateAndRotateSurface(GameObject surface, Vector3 origin, Options.Coordinates coordinates, GetOrigin.Orientation orientation, bool isCue = false)  // ----- egocentric coordinates!
    {
        surface.transform.position = origin;

        if (coordinates == Options.Coordinates.icosphere)
            if (isCue) // calculated using spherical coordinates
                RotateSurfaceDiscreteSpherical(surface, orientation);
            else
                RotateSurfaceDiscreteIcosphere(surface, orientation);
        else
            RotateSurfaceDiscreteSpherical(surface, orientation);
    }

    static public List<Vertex> TranslateAndRotateVertices(List<Vertex> vertices, int optionsNumber, Options.Coordinates coordinates, GetOrigin.Orientation orientation, Vector3 origin)
    {
        GameObject surface = VisualizeVertices(vertices);
        surface.transform.position = origin;

        if (coordinates == Options.Coordinates.icosphere)
            RotateSurfaceDiscreteIcosphere(surface, orientation);
        else
            RotateSurfaceDiscreteSpherical(surface, orientation);

        List<Vertex> rotatedVertices = new List<Vertex>();

        int i = 0;

        foreach (Vertex vertex in vertices)
        {
            Debug.LogFormat("vertex.gameObject.transform.position:{0}", vertex.gameObject.transform.position);
            rotatedVertices.Add(new Vertex(optionsNumber: optionsNumber, position: vertex.gameObject.transform.position, index: i, isCreateGameObject: false, coordinates: vertex.coordinates, radius: vertex.radius, keepAngleArray: vertex.keepAngleArray));
            i++;
        }

        GameObject.Destroy(surface);
        return rotatedVertices;
    }

    public static void SetPointerSurface(GameObject surface)
    {
        // ---- pointer surface
        GameObject pointerSurface = new GameObject("pointerSurface");

        if (GameObject.Find("SCRIPTS").activeInHierarchy == false)
            Debug.LogError("GameObject.Find(CentralMemory.SCRIPTS_gameObjectName).activeInHierarchy == false"); // SCRIPTS not active

        float pointerRadius = GameRunner.currentTrialData.options.radius;

        if (GameRunner.currentTrialData.options.coordinates == Options.Coordinates.depthConfig1 ||
            GameRunner.currentTrialData.options.coordinates == Options.Coordinates.depthConfig2)
        { pointerRadius = (GameRunner.currentTrialData.options.radius + GameRunner.currentTrialData.options.radiusDepth)/2; }; // average of the two to minimise bias

        switch (GameOptions.rayCastMesh)
        {
            case GameOptions.RayCastMesh.native:
                pointerSurface.name = "pointerSurface";
                pointerSurface.layer = LayerMask.NameToLayer("mainSurface");
                pointerSurface.AddComponent<MeshFilter>().mesh = IcoSphere.CreateMesh(radius: pointerRadius, recursionLevel: 3); // warning hard coded
                InvertMeshNormals(pointerSurface.GetComponent<MeshFilter>().mesh);
                pointerSurface.AddComponent<MeshCollider>();
                pointerSurface.transform.parent = surface.transform; // already set to origin
                break;
            case GameOptions.RayCastMesh.blender:
                GameObject blendObject = Resources.Load<GameObject>("icosphere6"); // .blend file (Blender)
                pointerSurface.AddComponent<MeshRenderer>();
                pointerSurface.AddComponent<MeshFilter>().mesh = blendObject.GetComponent<MeshFilter>().sharedMesh;
                pointerSurface.layer = LayerMask.NameToLayer("mainSurface");
                //Surfaces.InvertMeshNormals(pointerSurface.GetComponent<MeshFilter>().mesh); // doesn't work with .blend files
                pointerSurface.AddComponent<MeshCollider>();
                GameObject.Destroy(pointerSurface.GetComponent<MeshRenderer>());
                pointerSurface.transform.parent = surface.transform;
                pointerSurface.transform.localScale *= pointerRadius;
                break;
        }
    }

    static public void RotateSurfaceDiscreteIcosphere(GameObject surface, GetOrigin.Orientation orientation, int direction = +1)
    {
        switch (orientation)
        {
            case GetOrigin.Orientation.negativeX:
                surface.transform.Rotate(Vector3.up, -90 * direction, Space.World);
                break;
            case GetOrigin.Orientation.positiveX:
                surface.transform.Rotate(Vector3.up, +90 * direction, Space.World);
                break;
            case GetOrigin.Orientation.negativeZ:
                surface.transform.Rotate(Vector3.up, -180 * direction, Space.World);
                break;
            case GetOrigin.Orientation.positiveZ:
                surface.transform.Rotate(Vector3.up, 0 * direction, Space.World);
                break;
            default:
                break;
        }
    }

    static public void RotateSurfaceDiscreteSpherical(GameObject surface, GetOrigin.Orientation orientation, int direction = +1)
    {
        switch (orientation)
        {
            case GetOrigin.Orientation.negativeX:
                surface.transform.Rotate(Vector3.up, 180 * direction, Space.World);
                break;
            case GetOrigin.Orientation.positiveX:
                surface.transform.Rotate(Vector3.up, 0 * direction, Space.World);
                break;
            case GetOrigin.Orientation.negativeZ:
                surface.transform.Rotate(Vector3.up, +90 * direction, Space.World);
                break;
            case GetOrigin.Orientation.positiveZ:
                surface.transform.Rotate(Vector3.up, -90 * direction, Space.World);
                break;
            default:
                break;
        }
    }

    public static void InvertMeshNormals(Mesh mesh)
    {
        // ----- invert normals

        Vector3[] normals = mesh.normals;

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -1 * normals[i];
        }

        mesh.normals = normals;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] tris = mesh.GetTriangles(i);

            for (int j = 0; j < tris.Length; j += 3)
            {
                //swap order of tri vertices
                int temp = tris[j];
                tris[j] = tris[j + 1];
                tris[j + 1] = temp;
            }

            mesh.SetTriangles(tris, i);
        }
    }
}
