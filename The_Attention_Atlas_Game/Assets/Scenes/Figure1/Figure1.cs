using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using VacuumShaders.TheAmazingWireframeShader;
using DataStructures;

public class Figure1 : MonoBehaviour
{
    Vector3 origin = new Vector3(0.009703472f, 1.242f, -0.213f);
    static int recursionLevel = 3;
    static float radius = 1.5f;
    
    string screenshotDirectory;
    int resWidth = 1080;
    int resHeight = 1080;

    List<GameObject> cameraList = new List<GameObject>();

    IcoSphere icosphere;
    public Material surfaceMaterial;

    void Start()
    {

        Stimuli.LoadSprites();

        screenshotDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, @"..\..\screenshots\"));
        print(screenshotDirectory);

        if (!System.IO.Directory.Exists(screenshotDirectory))
        {
            System.IO.Directory.CreateDirectory(screenshotDirectory);
        }

        //// cameras
        float cameraRadius = 2f;
        List<CameraPerspectives> cameraPerspectives = new List<CameraPerspectives>(){ new CameraPerspectives(new Vector3(-cameraRadius, 0, 0), "left", new Vector3(0,90,0)),
                                                                                      new CameraPerspectives(new Vector3(0, 0, +cameraRadius), "front", new Vector3(0,-180,0)),
                                                                                      new CameraPerspectives(new Vector3(+cameraRadius, 0, 0), "right", new Vector3(0,-90,0)),
                                                                                      new CameraPerspectives(new Vector3(0, 0, -cameraRadius), "back", new Vector3(0,0,0)),
                                                                                      new CameraPerspectives(new Vector3(0, -cameraRadius, 0), "top", new Vector3(-90,0,0)),
                                                                                      new CameraPerspectives(new Vector3(0, +cameraRadius, 0), "bottom", new Vector3(90,0,0)) };

        cameraList.Add(GameObject.Find("Cameras/thirdPersonFar"));
        cameraList.Add(GameObject.Find("Cameras/thirdPersonNear"));
        cameraList.Add(GameObject.Find("Cameras/firstPerson"));

        foreach (var item in cameraPerspectives)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = item.name;
            gameObject.transform.position = item.position + origin;
            gameObject.transform.eulerAngles = item.rotation;
            gameObject.transform.parent = GameObject.Find("Cameras").transform;
            
            Camera camera = gameObject.AddComponent<Camera>();
            camera.stereoTargetEye = StereoTargetEyeMask.None;

            //camera.fieldOfView = 102; // perspective

            camera.orthographic = true;
            camera.orthographicSize = 1.6f;

            camera.backgroundColor = Color.black;
            camera.clearFlags = CameraClearFlags.SolidColor;

            cameraList.Add(gameObject);

            // spot lights
            GameObject lightGameObject = new GameObject();
            lightGameObject.name = item.name;
            lightGameObject.transform.position = item.position + origin;
            lightGameObject.transform.eulerAngles = item.rotation;
            lightGameObject.transform.parent = GameObject.Find("Lights").transform;

            var light = lightGameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 4;
            light.intensity = 2;
            light.innerSpotAngle = 0;
            light.spotAngle = 90;

        }

        icosphere = new IcoSphere(recursionLevel, radius, surfaceMaterial, "isosphere");
        icosphere.gameObject.transform.position = origin;
        Mesh mesh = WireframeGenerator.Generate(icosphere.meshFilter.mesh);
        icosphere.meshFilter.mesh = mesh;
    }

    public ActiveCamera activeCamera = ActiveCamera.thirdPersonFar;

    public enum ActiveCamera
    {
        thirdPersonFar,
        thirdPersonNear,
        firstPerson,
        left,
        front,
        right,
        back,
        top,
        bottom
    }

    class CameraPerspectives
    {
        public Vector3 position;
        public string name;
        public Vector3 rotation;

        public CameraPerspectives(Vector3 position, string name, Vector3 rotation)
        {
            this.position = position;
            this.name = name;
            this.rotation = rotation;
        }
    }

    void Update()
    {

        if (Time.frameCount-1 <= (int)Enum.GetValues(typeof(ActiveCamera)).Cast<ActiveCamera>().Last())
        {
            print(Time.frameCount - 1);
            activeCamera = (ActiveCamera)(Time.frameCount-1);
            print(activeCamera);
        }

        if (Time.frameCount - 1 == 9) // search surface
        {
            CentralMemory.observer = new Observer(orientation: GetOrigin.Orientation.negativeZ, origin: origin);
            GameRunner.currentTrialData = new GameRunner.TrialData();
            GameRunner.currentTrialData.targetPosition = 0;

            Options options = new Options(
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.fullField,
                radius: radius,
                keepAngleArray: GameOptions.sphericalSpacing * 4);

            GameObject searchSurface = StimulusPlotter.BuildSurface(options);
            searchSurface.SetActive(true);

            activeCamera = ActiveCamera.thirdPersonFar;

            foreach (var item in cameraList)
            {
                item.SetActive(item.name == Enum.GetName(typeof(ActiveCamera), activeCamera));
            }

            icosphere.gameObject.SetActive(false);

            string cameraName = Enum.GetName(typeof(ActiveCamera), activeCamera);
            print(cameraName);
            TakeScreenShot(GameObject.Find("Cameras/" + cameraName).GetComponent<Camera>(), resWidth, resHeight, screenshotDirectory + @"\Figure 1.search." + cameraName + ".png");
        }


        if (Time.frameCount - 1 == 10) {
            activeCamera = ActiveCamera.firstPerson;

            foreach (var item in cameraList)
            {
                item.SetActive(item.name == Enum.GetName(typeof(ActiveCamera), activeCamera));
            }

            string cameraName = Enum.GetName(typeof(ActiveCamera), activeCamera);
            TakeScreenShot(GameObject.Find("Cameras/" + cameraName).GetComponent<Camera>(), resWidth, resHeight, screenshotDirectory + @"\Figure 1.search." + cameraName + ".png");


        } // search surface




        foreach (var item in cameraList)
        {
            item.SetActive(item.name == Enum.GetName(typeof(ActiveCamera), activeCamera));
        }

        if (Time.frameCount - 1 <= (int)Enum.GetValues(typeof(ActiveCamera)).Cast<ActiveCamera>().Last())
        {
            string cameraName = Enum.GetName(typeof(ActiveCamera), activeCamera);
            print(cameraName);
            TakeScreenShot(GameObject.Find("Cameras/" + cameraName).GetComponent<Camera>(), resWidth, resHeight, screenshotDirectory + @"\Figure 1." + cameraName + ".png");
        }

    }

    private void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
    }

    void TakeScreenShot(Camera camera, int resWidth, int resHeight, string filename)
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }
}

