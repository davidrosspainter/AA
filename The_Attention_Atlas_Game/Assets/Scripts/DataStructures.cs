using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace DataStructures
{
    [Serializable]
    public class GameOptions
    {
        public static bool isShowLevelResults = true;

        public static bool isPlayAudioInstructions = true;
        public static bool isPlayAffirmations = true;

        public static bool saveDataThread = true; // if true, save methods are threaded to improve performance
        public static bool isQuitGame = false; // stores whether game should now be quit due to escape button being pressed
        public static bool isDisplayConfetti = true;

        // debugging options
        public static bool isAutomate = false; // set automation options in UpdateParameters()
        public static float durationFactor = 1f; // 1 = normal duration of levels, < 1 = shorter duration of levels

        // visualiser options
        public static bool isVisualise = false; // runs visualiser - switching to pure Python visualisation now to improve game performance

        // analysis options
        public static bool isAutomaticAnalyse = true;
        public static bool isEndGameAfterAnalyse = true;

        // surface
        public static RayCastMesh rayCastMesh = RayCastMesh.native;
        public static bool isRotateTarget = false;
        public static int recursionLevelPointerSurface = 3;

        // stimulus scale options
        public static float spriteScaleOpenDyslexic = .036f; // letters, numbers of the OpenDyslexic
        public static float boxColliderScaleOpenDyslexic = .9f; // to avoid overlap

        // balloons settings
        // cards settings

        // icosphere
        public const float sphericalSpacing = 12.5f; // degrees

        // sound
        public static float volume = 1f; // [Range(0, 1)] // not used since moving toward multiple sources to include verbal instructions
        public static bool isPlayAudio = true;

        // origin options
        public static bool isSetOrigin = true;
        public static float originTargetRadius = 5;
        public static float originDistance = 100;
        public static float gazeHoldDuration1 = 1; // for height measurement
        public static float gazeHoldDuration2 = 4; // for position measurement
        public static SerializableVector3 origin = new Vector3(0.8005224f, 1.15599f, 0.7496623f); // for debugging and visualisation when isSetOrigin = False
        public static GetOrigin.Orientation orientation = GetOrigin.Orientation.positiveX; // for debugging and visualisation when isSetOrigin = False

        // timing
        public static bool isSkipCue = false;
        public static float secondsToWait = .1f; // to search array
        public static float timeLimitResponses = float.PositiveInfinity; // maximum RT before advancing to the next trial; results in accuracy = 0 and RT float.nan?

        // responses
        public static ResponseButton responseButton = ResponseButton.trackpad;

        // Colors
        public static class Colors
        {
            public static Color selected = Color.yellow;

            public static Color targetSerial = Color.white;
            public static Color targetSerial2 = Color.red;
            public static Color targetUniqueFeature = Color.red;
            public static Color targetConjunction = Color.red;

            public static Color distractorSerial = Color.white;
            public static Color distractorSerial2 = Color.red;
            public static Color distractorUniqueFeature = Color.blue;
            public static Color distractorConjunction = Color.blue;

            public static List<Color> colorsRainbow = new List<Color>() { Color.red, Color.blue, Color.green, Color.magenta, Color.cyan };

        }

        [Serializable]
        public enum RayCastMesh
        {
            native, // low-res - based on surface icosphere
            blender // high res - recursion level = 6, used because Unity will hang at high recursion levels - code needs optimisation; needs blender installed and Unity restart; works after deleting .meta file associated with the .blend file
        }

        [Serializable]
        public enum ResponseButton
        {
            trackpad,
            trigger
        }

        public static void Setup()
        {
            if (isAutomate) // set automation options here
            {
                isSetOrigin = true;
                isSkipCue = true;
                timeLimitResponses = .5f;
            }
        }
    }

    [Serializable]
    public class Options
    {
        public string descriptor; // must be specified for all - but doesn't break game
        public int optionsNumber = -1; // calculated automatically

        public int numberOfRepetitions; // must be specified for all

        public Stimulus stimulus; // must be specified for all
        public SearchMode searchMode; // must be specified for all
        public Coordinates coordinates; // must be specified for all

        // useful keepAngleArrays for icosphere: 9, 18, 37, 55, 90, 180, 360
        // useful keepAngleArrays for spherical coordinates: 1, 2, 3, & 4 * GameOptions.sphericalSpacing

        public float keepAngleArray; // must be specified for icosphere, fullfield, depthConfig1, depthConfig2 - ignored otherwise
        public int recursionLevel; // must be specified for icosphere

        public float radius; // must be specified for all
        public float radiusDepth; // must be specified for depthConfig1, depthConfig2 - ignored otherwise

        [Serializable]
        public enum Stimulus
        {
            undefined, // for empty initialisation
            letters,
            numbers,
            indices,
            dots,
            balloons,
            cards,
            shapes,
            symbols,
            food,
            specOrbs
        }

        [Serializable]
        public enum SearchMode
        {
            undefined, // for empty initialisation
            serial,
            uniqueFeature,
            conjunction,
            serial2,
            rainbow,
            none // for retaining original sprite color - to do?
        }

        [Serializable]
        public enum Coordinates
        {
            undefined, // for empty initialisation
            pair1, // 1 * GameOptions.spacing
            pair2, // 2 * GameOptions.spacing
            pair3, // 3 * GameOptions.spacing
            pair4, // 4 * GameOptions.spacing
            horizontal,
            vertical,
            depthConfig1,
            depthConfig2,
            fullField,
            icosphere
        };

        public Options(string descriptor = "",
                       int numberOfRepetitions = -1,
                       Stimulus stimulus = Stimulus.undefined,
                       SearchMode searchMode = SearchMode.undefined,
                       Coordinates coordinates = Coordinates.undefined,
                       float radius = float.NaN,
                       float radiusDepth = float.NaN,
                       float keepAngleArray = float.NaN,
                       int recursionLevel = -1)
        {
            this.descriptor = descriptor;
            this.numberOfRepetitions = numberOfRepetitions;
            this.stimulus = stimulus;
            this.searchMode = searchMode;
            this.radius = radius;
            this.radiusDepth = radiusDepth;
            this.keepAngleArray = keepAngleArray;
            this.coordinates = coordinates;
            this.recursionLevel = recursionLevel;
        }
    }

    [Serializable]
    public class Observer
    {
        public string ID; // testing = 0; patents = positive numbers, healthy controls = negative numbers

        public string gameStartTime;
        public string levelStartTime;

        public string dataPath;

        public GetOrigin.Orientation orientation; // default orientation change in central memory!
        public SerializableVector3 origin;
        public SerializableVector3 originHeight; // adjust y and (x or z) displacement of origin target depending on observer starting point and orientation

        public Observer(GetOrigin.Orientation orientation, SerializableVector3 origin)
        {
            this.orientation = orientation;
            this.origin = origin;
        }

        public Observer(string ID = "", 
                        string gameStartTime = null,
                        string levelStartTime = null,
                        GetOrigin.Orientation orientation = GetOrigin.Orientation.undefined,
                        SerializableVector3 origin = null,
                        SerializableVector3 originHeight = null)
        {
            this.ID = ID;
            this.gameStartTime = gameStartTime;
            this.levelStartTime = levelStartTime;
            dataPath = Path.Combine(CentralMemory.GamePaths.data + gameStartTime + @"\" + levelStartTime + @"\");
            this.orientation = orientation;
            this.origin = origin;
            this.originHeight = originHeight;

            CreateOutputDirectory();

            Debug.LogFormat("Observer.name:{0}", gameStartTime);
            Debug.LogFormat("Observer.dataPath:{0}", dataPath);
        }

        void CreateOutputDirectory()
        {
            if (!Directory.Exists(dataPath))
            {
                Debug.LogFormat("{0}: creating", dataPath);
                Directory.CreateDirectory(dataPath);
            }
            Debug.LogFormat("{0}: already exists", dataPath);

            // Issue: current read/write .json and maybe .dat doesn't overwrite existing saves. Solution: delete all files!
            // DANGER! Clears directory of files and folders!

            DirectoryInfo directoryInfo = new DirectoryInfo(dataPath);

            foreach (FileInfo file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directoryInfo.EnumerateDirectories()) // no directories in observer data root yet, but there may be in future
            {
                dir.Delete(true);
            }
        }
    }

    [Serializable]
    public class FilePath
    {
        public string dataPath;
        public string levelStartTime;

        public string observerJson;
        public string filePathJson;
        public string levelJson;
        public string verticesJson;
        public string trialDataJson;

        public string machineNameTxt;

        public string verticesNpz;
        public string trialDataNpz;
        public string frameDataNpz;

        public string observerDat;
        public string filePathDat;
        public string levelDat;
        public string verticesDat;
        public string trialDataDat;
        public string frameDataDat;

        public string headsetPng;
        public string controllerPng;
        public string eyePng;
        public string headsetControllerEyeFirstPersonPng;
        public string headsetControllerEyeThirdPersonPng;

        public string listCriticalTrialDataJson; // minimal data for randomisation

        public FilePath(string dataPath, string levelStartTime)
        {
            this.dataPath = dataPath;
            this.levelStartTime = levelStartTime;

            Debug.LogFormat("FilePath.dataPath:{0}", dataPath);

            observerJson = dataPath + levelStartTime + ".observer.json";
            filePathJson = dataPath + levelStartTime + ".filePath.json";
            levelJson = dataPath + levelStartTime + ".level.json";
            verticesJson = dataPath + levelStartTime + ".vertices.json";
            trialDataJson = dataPath + levelStartTime + ".trialData.json";

            machineNameTxt = dataPath + levelStartTime + ".machineName.txt";

            verticesNpz = dataPath + levelStartTime + ".vertices.npz";
            trialDataNpz = dataPath + levelStartTime + ".trialData.npz";
            frameDataNpz = dataPath + levelStartTime + ".frameData.npz";

            observerDat = dataPath + levelStartTime + ".observer.dat";
            filePathDat = dataPath + levelStartTime + ".filePath.dat";
            levelDat = dataPath + levelStartTime + ".level.dat";
            verticesDat = dataPath + levelStartTime + ".vertices.dat";
            trialDataDat = dataPath + levelStartTime + ".trialData.dat";
            frameDataDat = dataPath + levelStartTime + ".frameData.dat";

            headsetPng = dataPath + levelStartTime + ".Headset."; // need to add .png
            controllerPng = dataPath + levelStartTime + ".Controller.";
            eyePng = dataPath + levelStartTime + ".Eye.";
            headsetControllerEyeFirstPersonPng = dataPath + levelStartTime + ".HeadsetControllerEyeFirstPerson.";
            headsetControllerEyeThirdPersonPng = dataPath + levelStartTime + ".HeadsetControllerEyeThirdPerson.";

            listCriticalTrialDataJson = dataPath + levelStartTime + ".listCriticalTrialData.json";
        }
    }

    [Serializable]
    public class EssentialTransform
    {
        public string name;
        public int index;
        public bool isTracked = false;

        public SerializableVector3 position;
        public SerializableVector3 eulerAngles;
        public SerializableVector3 forward;
        public SerializableQuaternion rotation;

        public void Update(Transform transform)
        {
            if (transform.position != Vector3.zero) // tracked
            {
                position = transform.position;
                eulerAngles = transform.eulerAngles;
                forward = transform.forward;
                rotation = transform.rotation;
                isTracked = true;
            }
            else // not tracked
            {
                Clear();
                isTracked = false;
            }
        }

        public void Clear()
        {
            position = new Vector3(float.NaN, float.NaN, float.NaN);
            eulerAngles = new Vector3(float.NaN, float.NaN, float.NaN);
            forward = new Vector3(float.NaN, float.NaN, float.NaN);
            rotation = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
        }

        public EssentialTransform(Transform transform, string name = "", int index = -1) // headset, controller...
        {
            Update(transform);
            this.name = name;
            this.index = index;
        }

        public EssentialTransform(Vector3 position, Vector3 eulerAngles, Vector3 forward, Quaternion rotation, string name = "", int index = -1) // headset, controller...
        {
            this.position = position;
            this.eulerAngles = eulerAngles;
            this.forward = forward;
            this.rotation = rotation;
            this.name = name;
            this.index = index;
        }

        public EssentialTransform(Vector3 position, Vector3 forward, string name = "", int index = -1) // eye gaze (has no rotation)
        {
            this.position = position;
            eulerAngles = new Vector3(float.NaN, float.NaN, float.NaN);
            this.forward = forward;
            rotation = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            this.name = name;
            this.index = index;
        }

        public EssentialTransform(string name = "", int index = -1)
        {
            Clear();
            this.name = name;
            this.index = index;
        }
    }

    // https://answers.unity.com/questions/956047/serialize-quaternion-or-vector3.html
    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2(float rX, float rY)
        {
            x = rX;
            y = rY;
        }

        public override string ToString()
        {
            return System.String.Format("[{0}, {1}]", x, y);
        }

        public static implicit operator Vector2(SerializableVector2 rValue)
        {
            return new Vector3(rValue.x, rValue.y);
        }

        public static implicit operator SerializableVector2(Vector2 rValue)
        {
            return new SerializableVector2(rValue.x, rValue.y);
        }
    } // props to Answer by Cherno · Apr 28, 2015 at 11:46 PM

    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }
        public override string ToString()
        {
            return System.String.Format("[{0}, {1}, {2}]", x, y, z);
        }
        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }
        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    } // props to Answer by Cherno · Apr 28, 2015 at 11:46 PM

    [Serializable]
    public class SerializableQuaternion
    {
        public float w;
        public float x;
        public float y;
        public float z;
        public SerializableQuaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override string ToString()
        {
            return System.String.Format("[{0}, {1}, {2}, {3}]", w, x, y, z);
        }
        public static implicit operator Quaternion(SerializableQuaternion serializableQuaternion)
        {
            return new Quaternion(serializableQuaternion.w, serializableQuaternion.x, serializableQuaternion.y, serializableQuaternion.z);
        }
        public static implicit operator SerializableQuaternion(Quaternion quaternion)
        {
            return new SerializableQuaternion(quaternion.w, quaternion.x, quaternion.y, quaternion.z);
        }
    }

    [Serializable]
    public class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(float r, float g, float b, float a = 1)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static implicit operator Color(SerializableColor serializableColor)
        {
            return new Color(serializableColor.r, serializableColor.g, serializableColor.b, serializableColor.a);
        }
        public static implicit operator SerializableColor(Color color)
        {
            return new SerializableColor(color.r, color.g, color.b, color.a);
        }

    }
}

