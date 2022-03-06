using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataStructures;
using System;
using System.IO;

public class StimulusPlotter : MonoBehaviour
{
    List<GameObject> surfaces;
    List<Options> listOptions;
    Camera myCamera;

    void Start()
    {
        UnityEngine.Random.InitState(1);

        surfaces = new List<GameObject>();
        listOptions = new List<Options>();
        myCamera = GameObject.Find("Camera").GetComponent<Camera>();

        // lights for 3D objects
        GameObject lightGameObject = new GameObject("The Light");
        Light light = lightGameObject.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));

        Stimuli.LoadSprites();

        CentralMemory.observer = new Observer(orientation: GetOrigin.Orientation.positiveX, origin: Vector3.zero);
        GameRunner.currentTrialData = new GameRunner.TrialData();
        GameRunner.currentTrialData.targetPosition = 0;
        GameRunner.currentTrialData.targetSpriteIndex = 0;

        List<Options.Stimulus> stimuli = new List<Options.Stimulus>() { Options.Stimulus.letters, Options.Stimulus.numbers, Options.Stimulus.balloons, Options.Stimulus.shapes, Options.Stimulus.symbols, Options.Stimulus.food, Options.Stimulus.specOrbs, Options.Stimulus.cards };

        foreach (var stimulus in stimuli)
        {
            //Options options = new Options(
            //stimulus: stimulus,
            //searchMode: Options.SearchMode.serial,
            //coordinates: Options.Coordinates.fullField,
            //radius: 2,
            //keepAngleArray: GameOptions.sphericalSpacing * 2);
            //listOptions.Add(options);

            Options options = new Options(
            stimulus: stimulus,
            searchMode: Options.SearchMode.serial,
            coordinates: Options.Coordinates.fullField,
            radius: 2,
            keepAngleArray: GameOptions.sphericalSpacing * 1);
                        listOptions.Add(options);
        }




        List<Options.SearchMode> searchModes = new List<Options.SearchMode>() { Options.SearchMode.uniqueFeature, Options.SearchMode.conjunction, Options.SearchMode.serial, Options.SearchMode.rainbow };

        foreach (var searchMode in searchModes)
        {
            Options options = new Options(
            stimulus: Options.Stimulus.letters,
            searchMode: searchMode,
            coordinates: Options.Coordinates.fullField,
            radius: 2,
            keepAngleArray: GameOptions.sphericalSpacing * 1);
            listOptions.Add(options);
        }

        List<Options.Coordinates> coordinates = new List<Options.Coordinates>() { Options.Coordinates.pair1, Options.Coordinates.pair2, Options.Coordinates.pair3, Options.Coordinates.pair4, Options.Coordinates.horizontal, Options.Coordinates.vertical, Options.Coordinates.fullField, Options.Coordinates.icosphere, Options.Coordinates.depthConfig1, Options.Coordinates.depthConfig2 };

        foreach (var coordinate in coordinates)
        {
            Options options = new Options(
            stimulus: Options.Stimulus.letters,
            searchMode: Options.SearchMode.serial,
            coordinates: coordinate,
            radius: 2,
            radiusDepth: 4,
            recursionLevel: 2,
            keepAngleArray: GameOptions.sphericalSpacing * 4);
            listOptions.Add(options);
        }

        foreach (var options in listOptions)
        {
            surfaces.Add(BuildSurface(options));
        }
    }

    public static GameObject BuildSurface(Options options)
    {
        GameRunner.currentTrialData.options = options;

        GameObject surface = Surfaces.Build(CentralMemory.observer, GameRunner.currentTrialData, isCue: false);
        surface.name = Enum.GetName(typeof(Options.Stimulus), (int)options.stimulus) + "." + Enum.GetName(typeof(Options.Coordinates), (int)options.coordinates) + "." + Enum.GetName(typeof(Options.SearchMode), (int)options.searchMode) + "." + options.keepAngleArray.ToString();
        surface.SetActive(false);
        return surface;
    }

    int frameCounter = 0;

    void ClearScene()
    {
        foreach (GameObject surface in surfaces)
        {
            surface.SetActive(false);
        }
    }

    public static void TakeScreenshot(Camera camera, string filename, int width=1080, int height=1080)
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

    void Update()
    {
        if (frameCounter < surfaces.Count)
        {
            ClearScene();
            surfaces[frameCounter].SetActive(true);

            if(frameCounter == 12)
            {
                myCamera.fieldOfView = 116;
            }

            if (listOptions[frameCounter].stimulus == Options.Stimulus.specOrbs | listOptions[frameCounter].stimulus == Options.Stimulus.food)
            {
                myCamera.backgroundColor = new Color(50 / 255, 50 / 255, 50 / 255);
            }
            else
            {
                myCamera.backgroundColor = new Color(0, 0, 0);
            }

            TakeScreenshot(myCamera, "..\\screenshots\\" + surfaces[frameCounter].name + ".png");
            frameCounter++;
        }
    }

}
