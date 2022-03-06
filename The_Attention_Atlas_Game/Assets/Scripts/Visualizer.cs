using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DataStructures;
using UnityEngine.SceneManagement;

// plots maps, shuts down game and runs Python/R analysis (R is called from Python)

public class Visualizer : MonoBehaviour
{
    // build with VR turned off in project settings (Player -> Virtual Reality Supported)
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    List<Mapper> mappers;
    List<Screenshot> screenshots;
    int frameCounter = 0; // fix for if called after experiment

    float markerScale = .05f/2;
    Material[] materials;

    Camera cameraFirstPerson;
    Camera cameraThirdPerson;

    class Mapper
    {
        public GameObject parentGameObject;
        public GameObject headsetMap;
        public GameObject controllerMap;
        public GameObject eyeMap;
        public GameObject vertices;

        public Mapper()
        {
            parentGameObject = new GameObject("map");
            headsetMap = new GameObject("headset"); headsetMap.transform.parent = parentGameObject.transform;
            controllerMap = new GameObject("controller"); controllerMap.transform.parent = parentGameObject.transform;
            eyeMap = new GameObject("eye"); eyeMap.transform.parent = parentGameObject.transform;
            vertices = new GameObject("vertices"); vertices.transform.parent = parentGameObject.transform;
        }
    }

    class Screenshot
    {
        public Mapper mapper;
        public string filename;
        public bool isHeadsetMapActve;
        public bool isControllerMapActive;
        public bool isEyeMapActive;
        public bool isVerticesActive;

        public Camera camera;

        public int width;
        public int height;

        public Screenshot(Mapper mapper, string filename, bool isHeadsetMapActve, bool isControllerMapActive, bool isEyeMapActive, bool isVerticesActive, Camera camera, int width = 1000, int height = 1000)
        {
            this.mapper = mapper;
            this.filename = filename;
            this.isHeadsetMapActve = isHeadsetMapActve;
            this.isControllerMapActive = isControllerMapActive;
            this.isEyeMapActive = isEyeMapActive;
            this.isVerticesActive = isVerticesActive;
            this.camera = camera;
            this.width = width;
            this.height = height;
        }

        public void ConfigureSceneAndTakeScreenshot()
        {
            mapper.parentGameObject.SetActive(true);
            mapper.headsetMap.SetActive(isHeadsetMapActve);
            mapper.controllerMap.SetActive(isControllerMapActive);
            mapper.eyeMap.SetActive(isEyeMapActive);
            mapper.vertices.SetActive(isVerticesActive);
            TakeScreenshot(camera, filename);
        }

        public void TakeScreenshot(Camera camera, string filename)
        {
            print(filename);
            RenderTexture rt = new RenderTexture(width, height, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(filename, bytes);
        }
    }

    public static string GetNewestGame(string parentDirectory)
    {
        string startTime = "";
        var observerFolders = new DirectoryInfo(parentDirectory).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc);

        foreach (var item in observerFolders)
        {
            DirectoryInfo[] directoryInfo = item.GetDirectories();

            if (directoryInfo.Count() > 0) // has data
            {
                startTime = item.Name;
                break;
            }
        }
        return startTime;
    }

    public static string GetNewestLevel(string parentDirectory)
    {
        string startTime = "";
        var observerFolders = new DirectoryInfo(parentDirectory).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc);

        foreach (var item in observerFolders)
        {
            FileInfo[] fileInfo = item.GetFiles();

            if (fileInfo.Count() > 0) // has data
            {
                startTime = item.Name;
                break;
            }
        }
        return startTime;
    }

    List<Surfaces.SerializableVertex> verticesSerializable;

    public List<Surfaces.SerializableVertex> LoadData()
    {
        CentralMemory.GamePaths.Setup();

        Debug.LogFormat("CentralMemory.GamePaths.data:{0}", CentralMemory.GamePaths.data);

        string gameStartTime = GetNewestGame(CentralMemory.GamePaths.data);
        string levelStartTime = GetNewestLevel(Path.Combine(CentralMemory.GamePaths.data + gameStartTime + @"\"));
        string filePathJson = CentralMemory.GamePaths.data + gameStartTime + "\\" + levelStartTime + "\\" + levelStartTime + ".filePath.json";

        Debug.LogFormat("gameStartTime:{0}", gameStartTime);
        Debug.LogFormat("levelStartTime:{0}", levelStartTime);
        Debug.LogFormat("filePathJson:{0}", filePathJson);

        // load data
        CentralMemory.filePath = SaveData.ReadJson<FilePath>(filePathJson);
        CentralMemory.observer = SaveData.ReadJson<Observer>(CentralMemory.filePath.observerJson);
        Debug.LogFormat("CentralMemory.observer.orientation{0}:", CentralMemory.observer.orientation);
        CentralMemory.level = SaveData.ReadJson<GameManager.Level>(CentralMemory.filePath.levelJson);

        CentralMemory.trialDataList = BinaryMethods.Read<List<GameRunner.TrialData>>(CentralMemory.filePath.trialDataDat);
        Debug.LogFormat("CentralMemory.trialDataList.Count:{0}", CentralMemory.trialDataList.Count);

        CentralMemory.frameDataList = BinaryMethods.Read<List<FrameRecorder.FrameData>>(CentralMemory.filePath.frameDataDat);
        Debug.LogFormat("CentralMemory.raycastFrameDataList.Count:{0}", CentralMemory.frameDataList.Count);

        List<Surfaces.SerializableVertex> verticesSerializable = BinaryMethods.Read<List<Surfaces.SerializableVertex>>(CentralMemory.filePath.verticesDat);
        Debug.LogFormat("verticesSerializable.Count:{0}", verticesSerializable.Count);

        float experimentDurationInMinutes = CentralMemory.frameDataList.Last().time / 60;
        Debug.LogFormat("experimentDurationInMinutes:{0}", experimentDurationInMinutes);

        return verticesSerializable;
    }

    void SetupScene()
    {
        cameraFirstPerson = GameObject.Find("FirstPersonCamera").GetComponent<Camera>();
        cameraThirdPerson = GameObject.Find("ThirdPersonCamera").GetComponent<Camera>();

        materials = new Material[5] { Resources.Load("Materials/Visualiser/headsetMarker") as Material, 
                                       Resources.Load("Materials/Visualiser/controllerLeftMarker") as Material,
                                       Resources.Load("Materials/Visualiser/controllerRightMarker") as Material,
                                       Resources.Load("Materials/Visualiser/eyeMarker") as Material,
                                       Resources.Load("Materials/Visualiser/vertex") as Material};
    }

    GameObject PlotPoint(string name, Vector3 position, float radius, float markerScale, Material material, Transform parentTransform, float radiusModifier = 1, Surfaces.LookDirection lookDirection = Surfaces.LookDirection.None, PrimitiveType shape = PrimitiveType.Sphere)
    {
        GameObject gameObject = GameObject.CreatePrimitive(shape);
        gameObject.name = name;
        gameObject.transform.position = position - CentralMemory.observer.origin;
        gameObject.transform.position /= radius * radiusModifier; // scale position by radius
        gameObject.transform.localScale *= markerScale;
        gameObject.GetComponent<Renderer>().material = material;
        gameObject.transform.parent = parentTransform;

        if (lookDirection != Surfaces.LookDirection.None)
            Surfaces.LookAtDirection(Vector3.zero, gameObject.transform, lookDirection);
        return gameObject;
    }

    Mapper Scatter3D(List<FrameRecorder.FrameData> listFrameData, Options options, GetOrigin.Orientation orientation, List<Surfaces.SerializableVertex> listSerializableVertices)
    {
        Mapper mapper = new Mapper();
        List<EssentialTransform> headsetTransformList = new List<EssentialTransform>();
        List<EssentialTransform> controllerLeftTransformList = new List<EssentialTransform>();
        List<EssentialTransform> controllerRightTransformList = new List<EssentialTransform>();
        List<EssentialTransform> eyeGazeTransformList = new List<EssentialTransform>();

        foreach (var frameData in listFrameData)
        {
            if (!IsNanVector3(frameData.headsetSurfacePosition))
                PlotPoint(name: "headsetMarker", position: frameData.headsetSurfacePosition, radius: options.radius, markerScale: markerScale, material: materials[0], parentTransform: mapper.headsetMap.transform);

            if (!IsNanVector3(frameData.controller1SurfacePosition))
                PlotPoint(name: "controllerLeftMarker", position: frameData.controller1SurfacePosition, radius: options.radius, markerScale: markerScale, material: materials[1], parentTransform: mapper.controllerMap.transform);

            if (!IsNanVector3(frameData.eyeGazeSurfacePosition))
                PlotPoint(name: "eyeMarker", position: frameData.eyeGazeSurfacePosition, radius: options.radius, markerScale: markerScale, material: materials[3], parentTransform: mapper.eyeMap.transform);

            headsetTransformList.Add(frameData.headset);
            controllerLeftTransformList.Add(frameData.controller0);
            controllerRightTransformList.Add(frameData.controller1);
            eyeGazeTransformList.Add(frameData.eyeGaze);
        }

        foreach (Surfaces.SerializableVertex serializableVertex in listSerializableVertices)
        {
            PlotPoint(name: "vertices", position: serializableVertex.position, radius: options.radius, markerScale: markerScale*2, material: materials[4], parentTransform: mapper.vertices.transform, 1.05f, lookDirection: Surfaces.LookDirection.Origin, shape: PrimitiveType.Sphere);
        }

        // ----- position headset/controller
        //GameObject HMD = Instantiate(Resources.Load("HMD") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);

        //ApplyTransform(HMD.transform, GetMeanEssentialTransform(headsetTransformList));
        //HMD.transform.position -= CentralMemory.observer.origin;
        //HMD.transform.parent = mapper.parentGameObject.transform;

        //GameObject controllerRight = Instantiate(Resources.Load("controllerRight") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
        //ApplyTransform(controllerRight.transform, GetMeanEssentialTransform(controllerRightTransformList));
        //controllerRight.transform.position -= CentralMemory.observer.origin;
        //controllerRight.transform.RotateAround(controllerRight.transform.position, Vector3.up, 180);
        //controllerRight.transform.parent = HMD.transform.parent = mapper.parentGameObject.transform;

        // ----- draw surface
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        surface.name = "surface";
        surface.transform.localScale *= options.radius;
        surface.transform.parent = mapper.parentGameObject.transform;
        surface.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Visualiser/surface.4") as Material;

        if (options.coordinates == Options.Coordinates.icosphere)
            Surfaces.RotateSurfaceDiscreteIcosphere(mapper.parentGameObject, orientation, direction: -1);
        else
            Surfaces.RotateSurfaceDiscreteSpherical(mapper.parentGameObject, orientation, direction: -1);

        return mapper;
    }

    void Start()
    {
        stopwatch.Start();

        mappers = new List<Mapper>();
        screenshots = new List<Screenshot>();

        verticesSerializable = LoadData();
        SetupScene();

        for (int dummyOptionsNumber = -1; dummyOptionsNumber < CentralMemory.level.listOptions.Count; dummyOptionsNumber++)
        {
            Debug.LogFormat("dummyOptionsNumber:{0}", dummyOptionsNumber);
            Options options;
            List<FrameRecorder.FrameData> listFrameData;
            List<Surfaces.SerializableVertex> listSerializableVertices;

            if (dummyOptionsNumber == -1)
            {
                options = CentralMemory.level.listOptions[0]; // for coordinate system and radius - assumes consistency across block
                listFrameData = CentralMemory.frameDataList.Where(o => o.state == GameRunner.TrialData.State.array).ToList();
                listSerializableVertices = verticesSerializable;
            }
            else
            {
                options = CentralMemory.level.listOptions[dummyOptionsNumber];
                listFrameData = CentralMemory.frameDataList.Where(o => o.optionsNumber == options.optionsNumber & o.state == GameRunner.TrialData.State.array).ToList();
                listSerializableVertices = verticesSerializable.Where(o => o.optionsNumber == options.optionsNumber).ToList();
            }
                
            // configure maps
            print("parsing raycastFrameData...");

            Debug.LogFormat("listFrameData.Count:{0}", listFrameData.Count);
            Mapper mapper = Scatter3D(listFrameData, options, CentralMemory.observer.orientation, listSerializableVertices);
            print("done parsing raycast frame data...");
            Debug.LogFormat("stopwatch.Elapsed.TotalMilliseconds:{0}", stopwatch.Elapsed.TotalMilliseconds); // draw time

            mapper.parentGameObject.name = dummyOptionsNumber.ToString();
            mappers.Add(mapper);

            // configure screenshots
            screenshots.Add(new Screenshot(mapper: mapper, filename: CentralMemory.filePath.headsetPng + dummyOptionsNumber.ToString() + ".png", isHeadsetMapActve: true, isControllerMapActive: false, isEyeMapActive: false, isVerticesActive: true, camera: cameraFirstPerson));
            screenshots.Add(new Screenshot(mapper: mapper, filename: CentralMemory.filePath.controllerPng + dummyOptionsNumber.ToString() + ".png", isHeadsetMapActve: false, isControllerMapActive: true, isEyeMapActive: false, isVerticesActive: true, camera: cameraFirstPerson));
            screenshots.Add(new Screenshot(mapper: mapper, filename: CentralMemory.filePath.eyePng + dummyOptionsNumber.ToString() + ".png", isHeadsetMapActve: false, isControllerMapActive: false, isEyeMapActive: true, isVerticesActive: true, camera: cameraFirstPerson));
            screenshots.Add(new Screenshot(mapper: mapper, filename: CentralMemory.filePath.headsetControllerEyeFirstPersonPng + dummyOptionsNumber.ToString() + ".png", isHeadsetMapActve: true, isControllerMapActive: true, isEyeMapActive: true, isVerticesActive: true, camera: cameraFirstPerson));
            screenshots.Add(new Screenshot(mapper: mapper, filename: CentralMemory.filePath.headsetControllerEyeThirdPersonPng + dummyOptionsNumber.ToString() + ".png", isHeadsetMapActve: true, isControllerMapActive: true, isEyeMapActive: true, isVerticesActive: true, camera: cameraThirdPerson));
        }
    }

    void ClearScene()
    {
        foreach (Mapper mapper in mappers)
        {
            mapper.parentGameObject.SetActive(false);
        }
        cameraFirstPerson.enabled = false;
        cameraThirdPerson.enabled = false;
    }

    void Update()
    {
        if(frameCounter < screenshots.Count)
        {
            ClearScene();
            screenshots[frameCounter].ConfigureSceneAndTakeScreenshot();
            frameCounter++;
        }
        else
        {
            cameraFirstPerson.enabled = true; // debugging
            Debug.LogFormat("stopwatch.Elapsed.TotalMilliseconds:{0}", stopwatch.Elapsed.TotalMilliseconds); // save time
            AdvanceToNextOptions();
        }
    }

    public static void AdvanceToNextOptions()
    {
        if (GameOptions.isAutomaticAnalyse)
        {
            print("analysing data...");

            string analysisCommand = "/K " + CentralMemory.GamePaths.pythonAbsolute + " -u " + Path.GetFullPath(CentralMemory.GamePaths.pythonAnalysis) + " -s " + GameOptions.isShowLevelResults.ToString().ToLower();
            Debug.LogFormat("analysisCommand:{0}", analysisCommand);

            if (Application.isEditor)
                System.Diagnostics.Process.Start("CMD.exe", analysisCommand); // doesn't work in builds
            else // workaround - writing command arguments to file and calling OpenURL on a wrapper function
                CentralMemory.StartExternalProcess(analysisCommand);
        }
        else
            print("skipping analysis");

        if (GameManager.game.listLevels.Count > 0 & GameOptions.isQuitGame == false)
        {
            SceneManager.LoadScene("GameManager"); // trouble running first config in sequence in builds
        }
        else
        {
            if (GameOptions.isEndGameAfterAnalyse)
            {
                SaveData.QuitGame();
            }
        }
    }

    void ApplyTransform(Transform transformToChange, EssentialTransform transformToApply)
    {
        if (!IsNanVector3(transformToApply.position) &
            !IsNanVector3(transformToApply.forward))
        {
            transformToChange.position = transformToApply.position;
            transformToChange.rotation = transformToApply.rotation;
            transformToChange.forward = transformToApply.forward;
        }        
    }

    bool IsNanVector3(Vector3 vector3)
    {
        return float.IsNaN(vector3.x) | float.IsNaN(vector3.y) | float.IsNaN(vector3.z);
    }

    private EssentialTransform GetMeanEssentialTransform(List<EssentialTransform> transformDR)
    {

        List<Vector3> positionList = new List<Vector3>();
        List<Vector3> eulerAnglesList = new List<Vector3>();
        List<Vector3> forwardList = new List<Vector3>();
        List<Quaternion> rotationList = new List<Quaternion>();

        foreach (var item in transformDR)
        {
            positionList.Add(item.position);
            eulerAnglesList.Add(item.eulerAngles);
            forwardList.Add(item.forward);
            rotationList.Add(item.rotation);
        }

        return new EssentialTransform(GetMeanVector3(positionList), GetMeanVector3(eulerAnglesList), GetMeanVector3(forwardList), GetMeanQuaternion(rotationList));
    }

    private Vector3 GetMeanVector3(List<Vector3> positions)
    {
        if (positions.Count == 0)
            return Vector3.zero;

        float x = 0f;
        float y = 0f;
        float z = 0f;

        foreach (Vector3 pos in positions)
        {
            x += pos.x;
            y += pos.y;
            z += pos.z;
        }

        return new Vector3(x / positions.Count, y / positions.Count, z / positions.Count);
    }

    private Quaternion GetMeanQuaternion(List<Quaternion> quaternions)
    {
        if (quaternions.Count == 0)
            return new Quaternion();

        float x = 0f;
        float y = 0f;
        float z = 0f;
        float w = 0f;

        foreach (var item in quaternions)
        {
            x += item.x;
            y += item.y;
            z += item.z;
            w += item.w;
        }
        return new Quaternion(x / quaternions.Count, y / quaternions.Count, z / quaternions.Count, w / quaternions.Count);
    }
}

//// rotation
//print(CentralMemory.observer.orientation);

//switch (CentralMemory.observer.orientation)
//{
//    case GetOrigin.Orientation.negativeX: // default orientation
//        break;
//    case GetOrigin.Orientation.positiveX:
//        cameraFirstPerson.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
//        cameraThirdPerson.transform.position = new Vector3(-2.615f, 2.162f, -2.696f);
//        cameraThirdPerson.transform.rotation = Quaternion.Euler(new Vector3(29.737f, 45.549f, 0));
//        break;
//    case GetOrigin.Orientation.negativeZ:
//        cameraFirstPerson.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
//        cameraThirdPerson.transform.position = new Vector3(-2.615f, 2.162f, 2.696f);
//        cameraThirdPerson.transform.rotation = Quaternion.Euler(new Vector3(29.737f, 135.549f, 0));
//        break;
//    case GetOrigin.Orientation.positiveZ:
//        cameraFirstPerson.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
//        cameraThirdPerson.transform.position = new Vector3(2.615f, 2.162f, -2.696f);
//        cameraThirdPerson.transform.rotation = Quaternion.Euler(new Vector3(29.737f, -45.549f, 0));
//        break;
//    default:
//        break;
//}