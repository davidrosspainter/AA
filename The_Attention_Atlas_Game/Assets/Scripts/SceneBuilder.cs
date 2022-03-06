using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NumSharp;
using DataStructures;

public class SceneBuilder : MonoBehaviour
{

    static GameObject masterProps;
    static float levelStartTime;

    public static void BuildScene()
    {
        Random.InitState(0);

        List<Asset> assets = new List<Asset>(); // unique objects used
        List<GameObject> go = new List<GameObject>(); // props

        List<List<float>> stimulusData; // from randomisation

        GameObject props = GameObject.Find("props");
        GameObject ground = GameObject.Find("ground") as GameObject;

        LoadAssets(out assets, out go, out stimulusData);

        Dictionary<int, List<Vector3>> sceneConfig = new Dictionary<int, List<Vector3>>(); // scene configuration
        sceneConfig.Add(0, PlantForest(go, assets));

        for (int i = 0; i < go.Count; i++)
        {
            go[i].transform.position = sceneConfig[0][i];
        }

        // trialData
        GameRunner.currentTrialData = new GameRunner.TrialData(trial: 0, options: CentralMemory.level.listOptions[0], state: GameRunner.TrialData.State.array);
        CentralMemory.trialDataNp = np.zeros<float>(1, GameRunner.header.Length);
        GameRunner.trial = 0;
        GameRunner.SaveTrialData();

        // generate vertices to allow analysis to run
        List<Surfaces.Vertex> vertices = SphericalCoordinates.GetSurfaceVerticesSpherical(CentralMemory.level.listOptions[0], isCreateGameObject: false);
        List<Surfaces.Vertex> rotatedVertices = Surfaces.TranslateAndRotateVertices(vertices: vertices, optionsNumber: 0, coordinates: CentralMemory.level.listOptions[0].coordinates, orientation: CentralMemory.observer.orientation, CentralMemory.observer.origin);
        SaveData.SaveVertices(vertices: rotatedVertices, filePath: CentralMemory.filePath);

        // set pointer surface
        GameObject surface = new GameObject("surface");
        Surfaces.SetPointerSurface(surface);
        Surfaces.TranslateAndRotateSurface(surface, CentralMemory.observer.origin, CentralMemory.level.listOptions[0].coordinates, CentralMemory.observer.orientation);
        Surfaces.TranslateAndRotateSurface(props, CentralMemory.observer.origin, CentralMemory.level.listOptions[0].coordinates, CentralMemory.observer.orientation);
        props.transform.position += new Vector3(0, -CentralMemory.observer.origin.y, 0);

        masterProps = new GameObject("masterProps0");
        props.transform.parent = masterProps.transform;

        // balance symmetry!
        List<Vector3> reflections = new List<Vector3>() { new Vector3(-1, +1, -1), new Vector3(-1, +1, +1), new Vector3(+1, +1, -1) };
        int count = 0;
        foreach (Vector3 reflection in reflections)
        {
            count++;
            GameObject propsnew = Instantiate(props);
            propsnew.name = count.ToString();
            propsnew.transform.localScale = reflection;
            propsnew.transform.parent = masterProps.transform;
        }

        //GameObject masterPropsNew = Instantiate(masterProps);
        //masterProps.name = "masterProps1";
        //masterPropsNew.transform.localScale = new Vector3(+1, -1, +1); // upside down!

        levelStartTime = Time.time;
    }

    void Start()
    {
        BuildScene();
    }

    void Update()
    {
        if ((Time.time - levelStartTime) / 60 >= CentralMemory.level.timeLimitMinutes) // quit
        {
            masterProps.SetActive(false);

            if (GameOptions.saveDataThread)
                SaveData.SaveGameThread(CentralMemory.trialDataNp, GameRunner.header, CentralMemory.filePath, CentralMemory.trialDataList, GameRunner.listCriticalTrialData, CentralMemory.frameDataList, CentralMemory.frameDataFloat, GameManager.game, GameManager.currentGameSummaryPath);
            else
                SaveData.SaveGame(isSaveTrialData: true);

            if (GameOptions.isVisualise)
            {
                SaveData.VisualiseData();
            }
            else
            {
                Visualizer.AdvanceToNextOptions(); // deals with running from the Game Manager or The Attention Atlas scene
            }
        }
    }


    public static class Environment
    {
        public static float maxRadius = 100; // where objects are placed
        public static float thetaMin = -Mathf.PI; // where objects are placed
        public static float thetaMax = +Mathf.PI; // where objects are placed
    }

    public class Asset
    {
        public string directory;
        public string name;

        public float minRad;
        public float maxRad;

        public float thetaMin = Environment.thetaMin;
        public float thetaMax = Environment.thetaMax;

        public int count;

        public GameObject go;

        public Asset(string directory, string name, float minRad, float maxRad, int count)
        {
            this.directory = directory;
            this.name = name;
            this.minRad = minRad;
            this.maxRad = maxRad;
            this.count = count;
        }
    }
 
    public static void LoadAssets(out List<Asset> assets, out List<GameObject> go, out List<List<float>> stimulusData )
    {

        stimulusData = new List<List<float>>();

        GameObject props = GameObject.Find("props");

        assets = new List<Asset>(); // unique objects used
        go = new List<GameObject>();

        // forest

        //List<string> prefabs = new List<string>(){
        //    // ground cover
        //    "ForestDRP/Vegetation_Mushroom_Blue_01",
        //    "ForestDRP/Vegetation_Fern_02",
        //    "ForestDRP/Vegetation_Flower_Tulip_02",
        //    "ForestDRP/Vegetation_Grass_2D_Patch_Square_01",

        //    // dead trees
        //    "ForestDRP/Vegetation_Tree_Trunk_05",
        //    "ForestDRP/Vegetation_Tree_Trunk_Fallen_01",
        //    "ForestDRP/Vegetation_Tree_Stump_01",

        //    // trees
        //    "ForestDRP/Vegetation_Tree_Pine_06",
        //    "ForestDRP/Vegetation_Tree_Common_04",

        //    "ForestDRP/tree1",
        //    "ForestDRP/tree2",
        //    "ForestDRP/tree3",
        //    "ForestDRP/tree4",
        //    "ForestDRP/tree5",
        //    "ForestDRP/tree6",
        //    "ForestDRP/tree7",
        //    "ForestDRP/tree8",
        //    "ForestDRP/tree9",
        //    "ForestDRP/tree10",
        //    "ForestDRP/tree11"};

        string parentDirectory = "AxeyWorks/Low Poly v2/";

        List<string> prefabs = new List<string>(){
            // ground cover
            parentDirectory + "Mushroom_Red_05",
            parentDirectory + "Plant_Brush_04",
            parentDirectory + "Plant_Flower_Red_01",
            parentDirectory + "Grass_Patch_Square_01",

            // dead trees
            parentDirectory + "Tree_Dead_02",
            parentDirectory + "Tree_Dead_02",
            parentDirectory + "Vegetation_Tree_Stump_01",

            // trees
            parentDirectory + "Tree_Generic_Winter_Snow_Capped_01",
            parentDirectory + "Tree_Generic_Winter_Snow_Capped_01",

            parentDirectory + "Tree_Asian_Cherry_Blossom_01",
            parentDirectory + "Tree_Asian_Shaped_01",
            parentDirectory + "Tree_Bamboo_01",
            parentDirectory + "Tree_Common_01",
            parentDirectory + "Tree_Conifer_01",
            parentDirectory + "Tree_Generic_Autumn_01",
            parentDirectory + "Tree_Generic_Spring_01",
            parentDirectory + "Tree_Palm_01",
            parentDirectory + "Tree_Pine_03",
            parentDirectory + "Tree_Pine_04",
            parentDirectory + "Tree_Pine_Autumn_01"};

        foreach (var item in prefabs)
        {
            GameObject gameObject = Resources.Load(item) as GameObject;
            print(item);
            Debug.LogFormat("{0}:{1}", item, gameObject == null);
        }

        //// ground cover
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Mushroom_Blue_01", 1, 15, (int)Mathf.Round(20 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Fern_02", 1, 15, (int)Mathf.Round(50 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Flower_Tulip_02", 1, 15, (int)Mathf.Round(50 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Grass_2D_Patch_Square_01", 0, 60, (int)Mathf.Round(500 * 1.2f)));

        //// dead trees
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Tree_Trunk_05", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Tree_Trunk_Fallen_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Tree_Stump_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));

        //// trees
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Tree_Pine_06", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "Vegetation_Tree_Common_04", 3, 50, (int)Mathf.Round(10 * 1.2f)));

        //assets.Add(new Asset("ForestDRP/", "tree1", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree2", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree3", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree4", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree5", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree6", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree7", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree8", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree9", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree10", 3, 50, (int)Mathf.Round(20 * 1.2f)));
        //assets.Add(new Asset("ForestDRP/", "tree11", 3, 50, (int)Mathf.Round(10 * 1.2f)));

        // ground cover
        assets.Add(new Asset(parentDirectory, "Mushroom_Red_05", 1, 15, (int)Mathf.Round(20 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Plant_Brush_04", 1, 15, (int)Mathf.Round(50 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Plant_Flower_Red_01", 1, 15, (int)Mathf.Round(50 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Grass_Patch_Square_01", 0, 60, (int)Mathf.Round(500 * 1.2f)));

        // dead trees
        assets.Add(new Asset(parentDirectory, "Tree_Dead_02", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Dead_02", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Stump_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));

        // trees
        assets.Add(new Asset(parentDirectory, "Tree_Generic_Winter_Snow_Capped_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Generic_Winter_Snow_Capped_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));

        assets.Add(new Asset(parentDirectory, "Tree_Asian_Cherry_Blossom_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Asian_Shaped_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Bamboo_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Common_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Conifer_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Generic_Autumn_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Generic_Spring_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Palm_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Pine_03", 3, 50, (int)Mathf.Round(10 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Pine_04", 3, 50, (int)Mathf.Round(20 * 1.2f)));
        assets.Add(new Asset(parentDirectory, "Tree_Pine_Autumn_01", 3, 50, (int)Mathf.Round(10 * 1.2f)));

        for (int i = 0; i < assets.Count; i++)
        {
            assets[i].go = Resources.Load(assets[i].directory + assets[i].name) as GameObject; // load object

            for (int j = 0; j < assets[i].count; j++)
            {
                go.Add(Instantiate(assets[i].go, new Vector3(0, 0, 0), Quaternion.identity));

                go.Last().transform.SetParent(props.transform);
                go.Last().transform.RotateAround(go[i].transform.position, go[i].transform.up, UnityEngine.Random.Range(0, 360));
                go.Last().name = i.ToString();
                Destroy(go.Last().GetComponent<MeshCollider>());
                stimulusData = ConstructStimulusData(i, j, go.Last().transform.rotation, stimulusData);

            }
        }
    }

    public static List<List<float>> ConstructStimulusData(int ii, int j, Quaternion rotation, List<List<float>> stimulusData)
    {
        List<float> stimulus = new List<float>();

        stimulus.Add(ii);
        stimulus.Add(j);
        stimulus.Add(rotation.w);
        stimulus.Add(rotation.x);
        stimulus.Add(rotation.y);
        stimulus.Add(rotation.z);
        stimulusData.Add(stimulus);

        return stimulusData;
    }
 
    public static List<Vector3> PlantForest( List<GameObject> go, List<Asset> assets )
    {
        List<Vector3> pos = new List<Vector3>();

        for (int j = 0; j < go.Count; j++)
        {
            int key = System.Convert.ToInt32(go[j].name);
            Point point = RandomisePosition(assets[key].minRad, assets[key].maxRad, assets[key].thetaMin, assets[key].thetaMax);
            pos.Add( new Vector3(point.x, 0, point.y) );
        }

        return pos;
    }

    public static Point RandomisePosition(float rMin, float rMax, float thetaMin, float thetaMax)
    {

        Point point = new Point();

        point.theta = UnityEngine.Random.Range(thetaMax, thetaMin);

        float A = 2 / (rMax * rMax - rMin * rMin);
        point.r = Mathf.Sqrt(2 * UnityEngine.Random.Range(0f, 1f) / A + rMin * rMin);

        point.x = point.r * Mathf.Cos(point.theta);
        point.y = point.r * Mathf.Sin(point.theta);

        return point;
    }

    public class Point
    {
        public float theta;
        public float r;
        public float x;
        public float y;
    }
}
