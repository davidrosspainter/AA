using System.Collections.Generic;
using UnityEngine;

using DataStructures;
using System;
using System.Collections;

public class GetOrigin : MonoBehaviour
{
    [Serializable]
    public enum Orientation
    {
        negativeX,
        positiveX,
        negativeZ,
        positiveZ,
        undefined
    }

    public static Orientation orientation = Orientation.undefined;
    public Orientation orientationInspector = Orientation.undefined; // for viewing in inspector

    public enum OriginState
    {
        PressButtonToStart,
        getOriginHeight,
        getOrigin,
        beginExperiment,
    }

    public OriginState originState = OriginState.PressButtonToStart;

    MaterialPropertyBlock blockUnselected;
    MaterialPropertyBlock blockSelected;

    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    List<Vector3> origins = new List<Vector3>();

    public static SerializableVector3 origin = Vector3.zero;
    public static SerializableVector3 originHeight = Vector3.zero;
    
    GameObject originTargetsParent;
    List<GameObject> originTargets = new List<GameObject>();
    
    static string hitColliderGameObjectName = null;

    public float rotationY = float.NaN;

    // arrows
    public GameObject arrowsParent;
    Sprite[] arrowSprites;
    string arrowCode = "↑↓←→";
    float arrowScale = .5f; // relative to parent!
    Color arrowColor = Color.magenta;

    public GameObject currentTarget;
    public Vector2 angle = Vector2.zero;
    public float rotation = float.NaN;
    public float newArrowScale = float.NaN;
    float arrowScaleModifier = .5f;

    [Serializable]
    class MinimalObserver
    {
        public Orientation orientation;
        public SerializableVector3 origin;
        public SerializableVector3 originHeight;

        public MinimalObserver(Orientation orientation, SerializableVector3 origin, SerializableVector3 originHeight)
        {
            this.orientation = orientation;
            this.origin = origin;
            this.originHeight = originHeight;
        }

        public MinimalObserver()
        {
        }

        public void DebugMinimalObserver()
        {
            print("DebugMinimalObserver()");
            Debug.LogFormat("orientation: {0}", orientation);
            Debug.LogFormat("origin: {0}", origin);
            Debug.LogFormat("originHeight: {0}", originHeight);
        }
    }

    MinimalObserver minimalObserver = new MinimalObserver();
    bool isUseNewCalibration = true;

    GameAudio gameAudio;

    void Start()
    {
        gameAudio = GetComponent<GameAudio>();

        if (GameOptions.isPlayAudioInstructions)
        {
            PlayOriginInstructions();
        }

        Debug.Log("GetOrigin2...");

        if (GameOptions.isSetOrigin)
        {
            // arrows
            arrowsParent = new GameObject("arrowsParent");
            arrowSprites = Resources.LoadAll<Sprite>("Arrows-01");

            // colours
            blockUnselected = new MaterialPropertyBlock();
            blockSelected = new MaterialPropertyBlock();

            blockUnselected.SetColor("_BaseColor", GameOptions.Colors.targetSerial);
            blockSelected.SetColor("_BaseColor", GameOptions.Colors.selected);

            // something to look at ...
            originTargetsParent = new GameObject();
            originTargetsParent.name = "originTargetsParent";

            Dictionary<Orientation, Vector3> originDictionary = new Dictionary<Orientation, Vector3>();

            originDictionary.Add(Orientation.negativeX, new Vector3(-GameOptions.originDistance, 0, 0));
            originDictionary.Add(Orientation.positiveX, new Vector3(+GameOptions.originDistance, 0, 0));
            originDictionary.Add(Orientation.negativeZ, new Vector3(0, 0, -GameOptions.originDistance));
            originDictionary.Add(Orientation.positiveZ, new Vector3(0, 0, +GameOptions.originDistance));

            foreach (var item in originDictionary)
            {
                GameObject originTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                originTarget.name = item.Key.ToString();

                originTarget.transform.localScale *= GameOptions.originTargetRadius * 2;
                originTarget.transform.position = item.Value;

                originTarget.GetComponent<MeshRenderer>().material = Resources.Load("Materials/originTargetMaterial") as Material;
                originTarget.GetComponent<Renderer>().SetPropertyBlock(blockUnselected);

                originTarget.layer = LayerMask.NameToLayer("fixationTarget");
                originTarget.transform.parent = originTargetsParent.transform;
                originTargets.Add(originTarget);

                DrawArrow(item.Value, arrowsParent, item.Key.ToString());
            }

            // set to gray to start so we know when people press the button
            SetOriginTargetsColor(new Color(.5f, .5f, .5f));

            // arrpws
            void DrawArrow(Vector3 position, GameObject parent, string name)
            {
                Dictionary<string, float> arrowRotationDictionary = new Dictionary<string, float>
                {
                    { "negativeZ", 0 },
                    { "positiveZ", 180 },
                    { "negativeX", 90 },
                    { "positiveX", -90 }
                };

                GameObject go = new GameObject(name);
                go.transform.Rotate(Vector3.up, arrowRotationDictionary[name]);
                go.transform.parent = parent.transform;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = arrowSprites[0];

                go.transform.position = position*.9f;
                go.transform.localScale *= arrowScale;

                sr.material.color = arrowColor;
            }
        }

        Debug.LogFormat("GamePanelController.calibration: {0}", GamePanelController.calibration);

        switch (GamePanelController.calibration)
        {
            case GamePanelController.Calibration.allLevels:
                print("allLevels");
                isUseNewCalibration = true;
                break;
            case GamePanelController.Calibration.firstLevel:
                print("firstLevel");
                if (GameManager.currentLevel != 0)
                {
                    isUseNewCalibration = false;
                    BeginExperiment(isUseNewCalibration);
                }
                else
                {
                    isUseNewCalibration = true;
                }
                break;
            case GamePanelController.Calibration.none: // needs fixing
                print("none");
                isUseNewCalibration = false;
                BeginExperiment(isUseNewCalibration);
                break;
            default:
                break;
        }
    }

    IEnumerator PlayAudioList(AudioSource audioSource, List<AudioClip> playList, float waitTime = 0)
    {
        print("PlayInstructions()");

        foreach (var clip in playList)
        {
            audioSource.PlayOneShot(clip);

            Debug.LogFormat("clip.length: {0}", clip.length);
            Debug.LogFormat("Time.time (start): {0}", Time.time);
            yield return new WaitForSeconds(clip.length + waitTime);
            Debug.LogFormat("Time.time (stop): {0}", Time.time);
        }
    }

    public void PlayOriginInstructions()
    {
        List<AudioClip> playList = new List<AudioClip> { gameAudio.instructions1, gameAudio.instructions2 };
        StartCoroutine(PlayAudioList(gameAudio.audioSourceOrigin, playList, 0f));
    }



    void Update()
    {
        GameRunner.AllKeyboardChecks(isSaveData: false);

        if (GameRunner.isPaused)
        {
            return;
        }

        if (isUseNewCalibration == false)
        {
            return;
        }

        if (GameOptions.isSetOrigin)
        {
            switch (originState)
            {
                case OriginState.PressButtonToStart:
                    PressButtonToStart();
                    break;
                case OriginState.getOriginHeight:
                    GetOriginHeight();
                    break;
                case OriginState.getOrigin:
                    GetOriginPrecise();
                    break;
            }

            if (originState != OriginState.beginExperiment) // prevents null reference error in RotateArrows() when arrowsParent is disabled
            {
                // y rotation
                //    -x
                // -z     +z
                //    +x

                //    270
                // 180    000
                //    090

                rotationY = InputManager.headset.transform.rotation.eulerAngles.y;

                if (IsBetween(rotationY, 270 - 45, 270 + 45))
                    orientation = Orientation.negativeX;
                if (IsBetween(rotationY, 180 - 45, 180 + 45))
                    orientation = Orientation.negativeZ;
                if (IsBetween(rotationY, 90 - 45, 90 + 45))
                    orientation = Orientation.positiveX;
                if (rotationY <= 45 | rotationY >= 270 + 45)
                    orientation = Orientation.positiveZ;

                orientationInspector = orientation;
                RotateArrows();
            }
        }
        else
        {
            BeginExperiment(isUseNewCalibration: isUseNewCalibration);
        }

        GameManager.EscapeGame(isSaveData: false);
    }


    void SetOriginTargetsColor(Color color)
    {
        foreach (var item in originTargets)
        {
            item.GetComponent<Renderer>().material.color = color; // for unlit materials
        }
    }

    void RotateArrows()
    {
        currentTarget = GameObject.Find("originTargetsParent/" + System.Enum.GetName(typeof(Orientation), orientation));
        GameObject arrow = GameObject.Find("arrowsParent/" + System.Enum.GetName(typeof(Orientation), orientation));

        Vector3 referenceForward = InputManager.headset.transform.forward.normalized;
        Vector3 spherePositionVector = currentTarget.transform.position.normalized;

        Dictionary<Orientation, List<Vector3>> rotationDictionary = new Dictionary<Orientation, List<Vector3>>
        {
            { Orientation.negativeZ, new List<Vector3>() { new Vector3(0, -1, 0), new Vector3(-1, 0, 0) } },
            { Orientation.positiveZ, new List<Vector3>() { new Vector3(0, -1, 0), new Vector3(+1, 0, 0) } },
            { Orientation.negativeX, new List<Vector3>() { new Vector3(0, -1, 0), new Vector3(0, 0, +1) } },
            { Orientation.positiveX, new List<Vector3>() { new Vector3(0, -1, 0), new Vector3(0, 0, -1) } }
        };

        angle.x = AngleOffsetAroundAxis(spherePositionVector, referenceForward, rotationDictionary[orientation][0]); // rotate vertical axis
        angle.y = AngleOffsetAroundAxis(spherePositionVector, referenceForward, rotationDictionary[orientation][1]); // rotate around perpendicular horizontal axis

        //rotation = Mathf.Tan(angle.y / angle.x)*Mathf.Rad2Deg; // doesn't work
        //currentTarget.transform.rotation = Quaternion.Euler(0, 0, rotation - 90);

        if (Mathf.Abs(angle.x) > Mathf.Abs(angle.y))
        {
            if (angle.x >= 0)
                arrow.GetComponent<SpriteRenderer>().sprite = arrowSprites[arrowCode.IndexOf("→")];
            else
                arrow.GetComponent<SpriteRenderer>().sprite = arrowSprites[arrowCode.IndexOf("←")];

            newArrowScale = Mathf.Min(arrowScale, Mathf.Abs(angle.x) * arrowScaleModifier);
        }
        else
        {
            if (angle.y >= 0)
                arrow.GetComponent<SpriteRenderer>().sprite = arrowSprites[arrowCode.IndexOf("↓")];
            else
                arrow.GetComponent<SpriteRenderer>().sprite = arrowSprites[arrowCode.IndexOf("↑")];

            newArrowScale = Mathf.Min(arrowScale, Mathf.Abs(angle.y) * arrowScaleModifier);
        }

        arrow.transform.localScale = new Vector3(newArrowScale, newArrowScale, newArrowScale);
    }

    // https://forum.unity.com/threads/is-vector3-signedangle-working-as-intended.694105/ // returns angle between two vectors

    public static float AngleOffsetAroundAxis(Vector3 v, Vector3 forward, Vector3 axis, bool clockwise = false)
    {
        Vector3 right;
        if (clockwise)
        {
            right = Vector3.Cross(forward, axis);
            forward = Vector3.Cross(axis, right);
        }
        else
        {
            right = Vector3.Cross(axis, forward);
            forward = Vector3.Cross(right, axis);
        }
        return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Rad2Deg;
    }

    public bool IsBetween(float testValue, float bound1, float bound2)
    {
        return (testValue >= Mathf.Min(bound1, bound2) && testValue <= Mathf.Max(bound1, bound2));
    }

    void PressButtonToStart()
    {
        if (InputManager.IsButtonPressed(GameOptions.responseButton))
        {
            originState = OriginState.getOriginHeight;
            SetOriginTargetsColor(GameOptions.Colors.targetSerial);
        }
    }

    void GetOriginHeight() // adjust target position height and translation based on observer position
    {
        if (!stopWatch.IsRunning)
        {
            Debug.Log("GetOriginTargetHeight");
            stopWatch.Start();
        }
        else
        {
            if (stopWatch.Elapsed.Seconds >= GameOptions.gazeHoldDuration1)
            {
                stopWatch.Stop();
                originHeight = GetMeanVector3(origins);

                if(new List<Orientation>() { Orientation.negativeX, Orientation.positiveX }.Contains(orientation))
                {
                    originTargetsParent.transform.position += new Vector3(0, originHeight.y, originHeight.z); // use mean y position of the head...
                    arrowsParent.transform.position += new Vector3(0, originHeight.y, originHeight.z); // use mean y position of the head...
                }
                else if (new List<Orientation>() { Orientation.negativeZ, Orientation.positiveZ }.Contains(orientation))
                {
                    originTargetsParent.transform.position += new Vector3(originHeight.x, originHeight.y, 0); // use mean y position of the head...
                    arrowsParent.transform.position += new Vector3(originHeight.x, originHeight.y, 0); // use mean y position of the head...
                }
                    

                CentralMemory.observer.originHeight = originHeight;

                origins = new List<Vector3>();

                stopWatch.Stop(); stopWatch.Reset();
                originState = OriginState.getOrigin;
            }
            else
            {
                origins.Add(InputManager.headset.transform.position);
            }
        }
    }

    void GetOriginPrecise()
    {
        Physics.Raycast(new Ray(InputManager.headset.transform.position, InputManager.headset.transform.forward), out RaycastHit hit, float.PositiveInfinity, LayerMask.GetMask(new string[] { "fixationTarget" }));

        if (hit.collider != null)
        {
            // hit.collider.gameObject.GetComponent<Renderer>().SetPropertyBlock(blockSelected); // for lit materials
            hit.collider.gameObject.GetComponent<Renderer>().material.color = GameOptions.Colors.selected; // for unlit materials

            if (!stopWatch.IsRunning)
            {
                Debug.Log("GetOrigin");
                stopWatch.Start();
            }
            else
            {
                if (stopWatch.Elapsed.Seconds >= GameOptions.gazeHoldDuration2)
                {
                    stopWatch.Reset();
                    Destroy(originTargetsParent);
                    origin = GetMeanVector3(origins);

                    hitColliderGameObjectName = hit.collider.gameObject.name;
                    Debug.LogFormat("hitColliderGameObjectName:{0}", hitColliderGameObjectName);

                    originState = OriginState.beginExperiment;
                    BeginExperiment();
                }
                else
                {
                    origins.Add(InputManager.headset.transform.position);
                }
            }
        }
        else
        {
            // uncolor the origin targets
            foreach (var item in originTargets)
            {
                // item.GetComponent<Renderer>().SetPropertyBlock(blockUnselected); // for lit materials
                item.GetComponent<Renderer>().material.color = GameOptions.Colors.targetSerial; // for unlit materials
            }

            stopWatch.Reset();
            origins = new List<Vector3>();
        }
    }

    void BeginExperiment(bool isUseNewCalibration = true)
    {
        Debug.Log("BeginExperiment()");
        Debug.LogFormat("isUseNewCalibration: {0}", isUseNewCalibration);

        // update origin
        if (GameOptions.isSetOrigin)
        {
            if (isUseNewCalibration)
            {
                orientation = (Orientation)System.Enum.Parse(typeof(Orientation), hitColliderGameObjectName);
                
                CentralMemory.observer.orientation = orientation;
                CentralMemory.observer.origin = origin;
                CentralMemory.observer.originHeight = originHeight;

                // save calibration
                minimalObserver = new MinimalObserver(orientation: orientation, origin: origin, originHeight: originHeight);
                SaveData.WriteAndReadJson<MinimalObserver>(minimalObserver, GamePanelController.ReturnPresetsDirectory() + "last.minimalObserver.json");
            }
            else
            {
                Destroy(originTargetsParent);

                // load calibration
                minimalObserver = SaveData.ReadJson<MinimalObserver>(GamePanelController.ReturnPresetsDirectory() + "last.minimalObserver.json");
                minimalObserver.DebugMinimalObserver();

                orientation = minimalObserver.orientation;
                origin = minimalObserver.origin;
                originHeight = minimalObserver.originHeight;

                CentralMemory.observer.orientation = minimalObserver.orientation;
                CentralMemory.observer.origin = minimalObserver.origin;
                CentralMemory.observer.originHeight = minimalObserver.originHeight;
            }

        }
        else // debugging
        {
            CentralMemory.observer.orientation = GameOptions.orientation;
            CentralMemory.observer.origin = GameOptions.origin;
            CentralMemory.observer.originHeight = new Vector3(float.NaN, float.NaN, float.NaN);
        }
        
        Debug.LogFormat("originToUse:{0}", originHeight);
        Debug.LogFormat("CentralMemory.observer.origin:{0}", CentralMemory.observer.origin);

        if (GameOptions.saveDataThread)
            SaveData.SaveObserverFilePathLevelThread(CentralMemory.observer, CentralMemory.filePath, CentralMemory.level);
        else
            (CentralMemory.observer, CentralMemory.filePath, CentralMemory.level) = SaveData.SaveObserverFilePathLevel(CentralMemory.observer, CentralMemory.filePath, CentralMemory.level);

        Debug.LogFormat("CentralMemory.observer.origin:{0}", CentralMemory.observer.origin);

        if (CentralMemory.level.isFreeViewing == false)
        {
            GetComponent<PointerSystem>().enabled = true;
            GetComponent<GameRunner>().enabled = true;
            GetComponent<AttentionTracker>().enabled = true;
        }
        else
        {
            GameObject.Find("props/ground").GetComponent<MeshRenderer>().enabled = true;
            GetComponent<SceneBuilder>().enabled = true;
        }

        if (gameAudio.audioSourceOrigin.isPlaying)
        {
            print("audioInstructions.audioSourceOrigin.isPlaying");
            gameAudio.audioSourceOrigin.Stop();
        }

        this.enabled = false;

        if (GameOptions.isSetOrigin)
            arrowsParent.SetActive(false);
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
}