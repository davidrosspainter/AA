using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataStructures;
using NumSharp;
using System;

public class GameRunner : MonoBehaviour
{
    public static bool isPaused = false;

    public static string[] header = {
            "trialData.trial",
            "trialData.optionsNumber",
            "trialData.recursionLevel",
            "trialData.radius",

            "trialData.cue.timeOnset",
            "(float) trialData.cue.frameCountOnset",
            "trialData.cue.timeResponse",
            "(float) trialData.cue.frameCountResponse",
            "(float) trialData.cue.RT",
            "GeneralMethods.BoolToFloat(trialData.cue.isCorrect)",

            "trialData.array.timeOnset",
            "(float) trialData.array.frameCountOnset",
            "trialData.array.timeResponse",
            "(float) trialData.array.frameCountResponse",
            "(float) trialData.array.RT",
            "GeneralMethods.BoolToFloat(trialData.array.isCorrect)",

            "(float) trialData.targetPosition",
            "(float) trialData.targetRotation" };


    [Serializable]
    public class TrialData
    {
        public int trial;
        public int optionsNumber;
        public int targetPosition;
        public float targetRotation;

        public State state;

        public Options options;

        public SpatioTemporalEvent cue = new SpatioTemporalEvent(SpatioTemporalEvent.Type.cue);
        public SpatioTemporalEvent array = new SpatioTemporalEvent(SpatioTemporalEvent.Type.array);

        public ListElements elements = new ListElements(); // for maintaining colour and other stimulus properties
        public int targetSpriteIndex = -1;

        [Serializable]
        public enum State
        {
            start,
            cue,
            array,
            wait,
            feedback,
            undefined
        }

        [Serializable]
        public class Element
        {
            public GameObject vertexGameObject;

            public int vertexIndex;
            public SerializableVector3 position;
            public Category category;

            public float rotation;
            public SerializableColor color;

            public string text;
            public Card card;
            public int spriteIndex;

            [Serializable]
            public class Card
            {
                public string card;
                public string suit;

                public Card(string card, string suit)
                {
                    this.card = card;
                    this.suit = suit;
                }
            }

            [Serializable]
            public enum Category
            {
                undefined,
                target,
                distractor
            }

            public Element(GameObject vertexGameObject, int vertexIndex, Vector3 position, Category category, float rotation, Color color, string text = "", Card card = null, int spriteIndex = -1)
            {
                this.vertexGameObject = vertexGameObject;
                this.vertexIndex = vertexIndex;
                this.position = position;
                this.category = category;
                this.rotation = rotation;
                this.color = color;
                this.text = text;
                this.card = card;
                this.spriteIndex = spriteIndex;
            }

            public Element()
            {

            }

        }

        public TrialData(int trial = -1, int optionsNumber = -1, State state = State.undefined, int targetPosition = -1, float targetRotation = float.NaN, Options options = null)
        {
            this.trial = trial;
            this.optionsNumber = optionsNumber;
            this.state = state;
            this.targetPosition = targetPosition;
            this.targetRotation = targetRotation;
            this.options = options;
        }
    }

    [Serializable]
    public class ListTrialData
    {
        public List<TrialData> listTrialData;

        public ListTrialData(List<TrialData> listTrialData)
        {
            this.listTrialData = listTrialData;
        }

        public ListTrialData() { }
    }

    [Serializable]
    public class ListElements
    {
        public List<TrialData.Element> listElements = new List<TrialData.Element>();

        public ListElements(List<TrialData.Element> listElements)
        {
            this.listElements = listElements;
        }

        public ListElements() { }
    }


    [Serializable]
    public class SpatioTemporalEvent
    {
        public Type type;

        public int vertex = -1;

        public float timeOnset = float.NaN;
        public int frameCountOnset = -1;

        public float RT = float.NaN;
        public bool? isCorrect = null;

        public float timeResponse = float.NaN;
        public int frameCountResponse = -1;

        [Serializable]
        public enum Type
        {
            cue,
            array
        }

        public SpatioTemporalEvent(Type type)
        {
            this.type = type;
        }
    }

    [Serializable]
    public class CriticalTrialData
    {
        public int optionsNumber;
        public int targetPosition;
        public float targetRotation;

        public CriticalTrialData(int optionsNumber, int targetPosition, float targetRotation)
        {
            this.optionsNumber = optionsNumber;
            this.targetPosition = targetPosition;
            this.targetRotation = targetRotation;
        }
    }

    [Serializable]
    public class ListCriticalTrialData
    {
        public List<CriticalTrialData> listCriticalTrialData;
        public ListCriticalTrialData(List<CriticalTrialData> listCriticalTrialData)
        {
            this.listCriticalTrialData = listCriticalTrialData;
        }
    }


    // trial data
    static int totalNumberOfTargets = 0; // in the level
    public static int trial = 0;
    public static TrialData currentTrialData;
    public static List<TrialData> listTrialData;
    public static List<CriticalTrialData> listCriticalTrialData; // minimal data for randomisation
    public static float levelStartTime = float.NaN;
    List<int> numberOfVertices;

    // gameObjects
    GameObject surface;

    public (List<int>, List<float>) BuildBlock(int numberOfVertices, int numberOfRepetitions)
    {
        int numberOfTargets = numberOfVertices * numberOfRepetitions;

        List<int> targetPositions = new List<int>(numberOfTargets);
        List<float> targetRotations = new List<float>(numberOfTargets);

        for (int i = 0; i < numberOfRepetitions; i++)
        {
            targetPositions.AddRange(GenericFunctions.RandPerm(numberOfVertices));
        }

        if (numberOfVertices == 2)
        {
            targetPositions.Shuffle();
        }

        // target rotation
        if (GameOptions.isRotateTarget == true)
        {
            for (int i = 0; i < numberOfTargets; i++)
            {
                List<float> rotations = new List<float>() { 0, 90, 180, 270 }; rotations.Shuffle();
                targetRotations.Add(rotations[0]);
                Debug.LogFormat("{0}:{1}:{2}", i, targetPositions[i], targetRotations[i]);
            }
        }
        else
        {
            for (int i = 0; i < numberOfTargets; i++)
            {
                targetRotations.Add(0);
            }
        }
        return (targetPositions, targetRotations);
    }

    void ResetStaticVariables() // for reloading with new configuation
    {
        totalNumberOfTargets = 0;
        trial = 0;
        currentTrialData = new TrialData();
        listTrialData = new List<TrialData>();
        listCriticalTrialData = new List<CriticalTrialData>();
        levelStartTime = float.NaN;
        numberOfVertices = new List<int>();
    }

    public List<int> GenerateAndSaveVertices()
    {
        List<Surfaces.Vertex> allVertices = new List<Surfaces.Vertex>();
        List<int> numberOfVertices = new List<int>();

        for (int optionsNumber = 0; optionsNumber < CentralMemory.level.listOptions.Count; optionsNumber++)
        {
            List<Surfaces.Vertex> vertices;

            Debug.LogFormat("CentralMemory.level.listOptions[optionsNumber].coordinates == Options.Coordinates.icosphere:{0}", CentralMemory.level.listOptions[optionsNumber].coordinates == Options.Coordinates.icosphere);
            if (CentralMemory.level.listOptions[optionsNumber].coordinates == Options.Coordinates.icosphere) // icosphere
            {
                vertices = Surfaces.GetSurfaceVerticesIcosphere(CentralMemory.level.listOptions[optionsNumber], isCreateGameObject: false);
                numberOfVertices.Add(vertices.Count);
            }
            else // spherical coordinates
            {
                vertices = SphericalCoordinates.GetSurfaceVerticesSpherical(CentralMemory.level.listOptions[optionsNumber], isCreateGameObject: false);
                numberOfVertices.Add(vertices.Count);
            }

            Debug.LogFormat("numberOfVertices:{0}", numberOfVertices);
            List<Surfaces.Vertex> rotatedVertices = Surfaces.TranslateAndRotateVertices(vertices: vertices, optionsNumber: optionsNumber, coordinates: CentralMemory.level.listOptions[optionsNumber].coordinates, orientation: CentralMemory.observer.orientation, origin: CentralMemory.observer.origin);
            allVertices.AddRange(rotatedVertices);
        }

        SaveData.SaveVertices(vertices: allVertices, filePath: CentralMemory.filePath);
        return (numberOfVertices);
    }

    public (List<CriticalTrialData>, int) GenerateCriticalTrialData(List<int> numberOfVertices)
    {
        int totalNumberOfTargets = 0; // reset!
        List<CriticalTrialData> listCriticalTrialData = new List<CriticalTrialData>();

        for (int optionsNumber = 0; optionsNumber < CentralMemory.level.listOptions.Count; optionsNumber++)
        {
            CentralMemory.level.listOptions[optionsNumber].optionsNumber = optionsNumber;

            Debug.LogFormat("optionsNumber:{0}", optionsNumber);
            Debug.LogFormat("CentralMemory.level.listOptions[optionsNumber].optionsNumber:{0}", CentralMemory.level.listOptions[optionsNumber].optionsNumber);

            int numberOfTargets = CentralMemory.level.listOptions[optionsNumber].numberOfRepetitions * numberOfVertices[optionsNumber]; print(numberOfTargets); // equivalent to number of trials
            (List<int> targetPositions, List<float> targetRotations) = BuildBlock(numberOfVertices[optionsNumber], CentralMemory.level.listOptions[optionsNumber].numberOfRepetitions);

            for (int target = 0; target < numberOfTargets; target++)
            {
                listCriticalTrialData.Add(new CriticalTrialData(optionsNumber: optionsNumber, targetPosition: targetPositions[target], targetRotation: targetRotations[target]));
            }
            totalNumberOfTargets += numberOfTargets; // for this options
        }

        if (CentralMemory.level.isRandomiseTrialOrder)
        {
            listCriticalTrialData.Shuffle(); // randomise options (for cases when listOptions.Count > 1)!
        }
        SaveData.SaveLevel(CentralMemory.level, CentralMemory.filePath); // save level options - maybe not the best place for it, but convenient enough
        return (listCriticalTrialData, totalNumberOfTargets);
    }

    int GetRandomTargetPosition()
    {
        // select target positions at random on each trial rather than iterating all target positions - increases apparent target randomness for low vertex counts (e.g., pairs)
        int numberOfVertices;

        if (CentralMemory.level.listOptions[0].coordinates == Options.Coordinates.icosphere) // icosphere
        {
            List<Surfaces.Vertex> vertices = Surfaces.GetSurfaceVerticesIcosphere(CentralMemory.level.listOptions[0], isCreateGameObject: false);
            numberOfVertices = vertices.Count;
        }
        else // spherical coordinates
        {
            List<Surfaces.Vertex> vertices = SphericalCoordinates.GetSurfaceVerticesSpherical(CentralMemory.level.listOptions[0], isCreateGameObject: false);
            numberOfVertices = vertices.Count;
        }

        int targetPosition = UnityEngine.Random.Range(0, numberOfVertices);
        return targetPosition;
    }

    void PopulateTrial()
    {
        if (trial == listCriticalTrialData.Count)
        {
            (List<CriticalTrialData> listCriticalTrialDataNew, int totalNumberOfTargetsNew) = GenerateCriticalTrialData(numberOfVertices);
            listCriticalTrialData.AddRange(listCriticalTrialDataNew);
            totalNumberOfTargets += totalNumberOfTargetsNew;
            CentralMemory.trialDataNp = np.vstack(CentralMemory.trialDataNp, np.zeros<float>(totalNumberOfTargets, header.Length));
        }

        listTrialData.Add(new TrialData(trial: trial,
                   state: TrialData.State.start,
                   options: CentralMemory.level.listOptions[listCriticalTrialData[trial].optionsNumber],
                   optionsNumber: listCriticalTrialData[trial].optionsNumber,
                   targetPosition: listCriticalTrialData[trial].targetPosition,
                   targetRotation: listCriticalTrialData[trial].targetRotation));

        currentTrialData = listTrialData[trial];
        LogCurrentTrialData();

        if (currentTrialData.options.stimulus == Options.Stimulus.food) // prevents error messages for automation
            currentTrialData.targetSpriteIndex = UnityEngine.Random.Range(0, Stimuli.foodSprites.Length);
        else if (currentTrialData.options.stimulus == Options.Stimulus.specOrbs)
            currentTrialData.targetSpriteIndex = UnityEngine.Random.Range(0, Stimuli.specOrbSprites.Length);

    }

    static global::GameAudio gameAudio;

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

    public void PlayGameInstructions()
    {
        AudioClip audioClip;

        switch (GameManager.currentLevel)
        {
            case 0:
                audioClip = gameAudio.instructions5;
                break;
            case 1:
                audioClip = gameAudio.instructions6;
                break;
            case 2:
                audioClip = gameAudio.instructions7;
                break;
            case 3:
                audioClip = gameAudio.instructions8;
                break;
            case 4:
                audioClip = gameAudio.instructions9;
                break;
            case 5:
                audioClip = gameAudio.instructions10;
                break;
            case 6:
                audioClip = gameAudio.instructions11;
                break;
            case 7:
                audioClip = gameAudio.instructions12;
                break;
            default:
                audioClip = gameAudio.instructions2;
                break;
        }

        List<AudioClip> playList = new List<AudioClip> { gameAudio.instructions3, audioClip };
        StartCoroutine(PlayAudioList(gameAudio.audioSourceGameRunner, playList, 0));
    }

    Dictionary<int, (bool, AudioClip)> timeRemainingMessageMemory = new Dictionary<int, (bool, AudioClip)>(); // (time remaining in minutes, hasMessagePlayed)

    void Start()
    {
        gameAudio = GetComponent<global::GameAudio>();

        timeRemainingMessageMemory.Add(4, (false, gameAudio.instructions13));
        timeRemainingMessageMemory.Add(3, (false, gameAudio.instructions14));
        timeRemainingMessageMemory.Add(2, (false, gameAudio.instructions15));
        timeRemainingMessageMemory.Add(1, (false, gameAudio.instructions16));

        if (GameOptions.isPlayAudioInstructions)
        {
            PlayGameInstructions();
        }
        
        Stimuli.LoadSprites();

        ResetStaticVariables();

        Debug.LogFormat("CentralMemory.observer.origin:{0}", CentralMemory.observer.origin);
        Debug.LogFormat("CentralMemory.level.descriptor:{0}", CentralMemory.level.descriptor);
        Debug.LogFormat("CentralMemory.level.isRandomiseTrialOrder:{0}", CentralMemory.level.isRandomiseTrialOrder);

        numberOfVertices = GenerateAndSaveVertices();
        (listCriticalTrialData, totalNumberOfTargets) = GenerateCriticalTrialData(numberOfVertices: numberOfVertices);
        PopulateTrial();
        CentralMemory.trialDataNp = np.zeros<float>(totalNumberOfTargets, header.Length);

        levelStartTime = Time.time;

        // lights for 3D objects
        GameObject lightGameObject = new GameObject("The Light");
        Light light = lightGameObject.AddComponent<Light>();
        light.type = LightType.Directional;

        switch (CentralMemory.observer.orientation)
        {
            case GetOrigin.Orientation.negativeX:
                lightGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
            case GetOrigin.Orientation.positiveX:
                lightGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, +90, 0));
                break;
            case GetOrigin.Orientation.negativeZ:
                lightGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case GetOrigin.Orientation.positiveZ:
                lightGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case GetOrigin.Orientation.undefined:
                break;
            default:
                break;
        }
    }

    void Update()
    {
        gameAudio.audioSourceOrigin.Stop(); // fix to stop playing levelling instructions
        AllKeyboardChecks();

        if (GameRunner.isPaused)
        {
            return;
        }

        switch (currentTrialData.state)
        {
            case TrialData.State.start:
                PresentCue();

                break;
            case TrialData.State.cue:
                if (AttentionTracker.PointerGlobal.Decision.number != null || GameOptions.isSkipCue)
                {                
                    Destroy(surface);
                    AttentionTracker.PointerGlobal.ClearDecision();
                    currentTrialData.state = TrialData.State.wait;

                    StartCoroutine(WaitAdvance(GameOptions.secondsToWait, TrialData.State.array));
                }
                break;
            case TrialData.State.array:
                if(AttentionTracker.PointerGlobal.Decision.number != null | AttentionTracker.PointerGlobal.Decision.stopwatch.Elapsed.TotalSeconds >= GameOptions.timeLimitResponses)
                {
                    Destroy(surface);
                    currentTrialData.state = TrialData.State.wait;
                    StartCoroutine(WaitAdvance(GameOptions.secondsToWait, TrialData.State.feedback));

                    if(AttentionTracker.PointerGlobal.Decision.stopwatch.Elapsed.TotalSeconds >= GameOptions.timeLimitResponses)
                    {
                        AttentionTracker.PointerGlobal.Decision.number = -1;
                    }
                }
                break;
            case TrialData.State.feedback:
                if(AttentionTracker.PointerGlobal.Decision.number != null)
                {
                    PresentFeedback();
                }
                break;
            default: // State.wait
                break;
        }

        if (trial == 0) // requires one trial of RT data to successfully save
        {
            GameManager.EscapeGame(isSaveData: false);
        }
        else
        {
            GameManager.EscapeGame(isSaveData: true);
        }

        // time remaining feedback message

        if (GameOptions.isPlayAudioInstructions)
        {
            float levelTimeRemaining = CentralMemory.level.timeLimitMinutes - ((Time.time - GameRunner.levelStartTime) / 60);
            List<int> keyList = new List<int>(timeRemainingMessageMemory.Keys);

            foreach (int key in keyList)
            {
                if (gameAudio.audioSourceGameRunner.isPlaying)
                {
                    break; // only checks to play message if not playing instructions and not playing error feedback, delays until next opportunity for clean transmission!
                }

                if (timeRemainingMessageMemory[key].Item1 == false) // only play message if not already played
                {
                    if (CentralMemory.level.timeLimitMinutes < key)
                    {
                        // don't play messages if level time limit < time remaining message
                        timeRemainingMessageMemory[key] = (true, timeRemainingMessageMemory[key].Item2); // change tuple status from not played (false) to played (true) to avoid checking/playing twice
                        continue;
                    }
                    else
                    {
                        if (levelTimeRemaining < key)
                        {
                            // play message if levelTimeRemaining < time remaining message
                            gameAudio.audioSourceGameRunner.PlayOneShot(timeRemainingMessageMemory[key].Item2); // play time remaining message
                            timeRemainingMessageMemory[key] = (true, timeRemainingMessageMemory[key].Item2); // change tuple status from not played (false) to played (true) to avoid checking/playing twice
                        }
                    }
                }
            }

        }
            
    }

    void PresentCue()
    {
        print("*************************************");
        print(string.Format("PresentCue:{0}:{1}", Time.time, Time.frameCount));

        // prepare trial data
        currentTrialData.trial = trial;
        currentTrialData.cue.timeOnset = Time.time;
        currentTrialData.cue.frameCountOnset = Time.frameCount;

        if (CentralMemory.level.listOptions.Count == 1 & CentralMemory.level.isRandomiseTrialOrder == true)
        {
            // randomise target position at probability of 1/n for each vertex position
            currentTrialData.targetPosition = GetRandomTargetPosition();
            listCriticalTrialData[trial].targetPosition = currentTrialData.targetPosition;
        }

        AttentionTracker.PointerGlobal.ClearDecision();
        Destroy(surface);

        currentTrialData.state = TrialData.State.cue;
        surface = Surfaces.Build(CentralMemory.observer, currentTrialData, isCue: true);
    }

    void PresentArray()
    {
        print(string.Format("PresentArray:{0}:{1}", Time.time, Time.frameCount));

        AttentionTracker.PointerGlobal.ClearDecision();
        AttentionTracker.PointerGlobal.Decision.stopwatch.Restart();

        Destroy(surface);

        currentTrialData.state = TrialData.State.array;
        surface = Surfaces.Build(CentralMemory.observer, currentTrialData, isCue: false);

        currentTrialData.array.timeOnset = Time.time;
        currentTrialData.array.frameCountOnset = Time.frameCount;
    }

    void PresentFeedback()
    {
        if (AttentionTracker.PointerGlobal.Decision.number != null)
        {
            switch (AttentionTracker.PointerGlobal.Decision.isCorrect)
            {
                case true:
                    gameAudio.audioSourceFeedback.PlayOneShot(gameAudio.correct);

                    if (!gameAudio.audioSourceGameRunner.isPlaying & GameOptions.isPlayAffirmations){
                        float diceRoll = UnityEngine.Random.Range(0, 1f);
                        Debug.LogFormat("diceRoll{0}", diceRoll);
                        if (diceRoll < 0.2f)
                        {
                            gameAudio.affirmations.Shuffle();
                            gameAudio.audioSourceGameRunner.PlayOneShot(gameAudio.affirmations[0]);
                        }
                    }
                    break;
                default:
                    if (!gameAudio.audioSourceGameRunner.isPlaying & GameOptions.isPlayAffirmations)
                    {
                        gameAudio.audioSourceGameRunner.PlayOneShot(gameAudio.instructions4);
                    }
                    break;
            }

            SaveTrialData();

            StartCoroutine(WaitAdvance(GameOptions.secondsToWait, TrialData.State.cue));
            AttentionTracker.PointerGlobal.ClearDecision();

            // advance to next trial!
            trial++;

            if ((trial == totalNumberOfTargets & CentralMemory.level.timeLimitMinutes == float.NegativeInfinity) |
                (Time.time - levelStartTime)/60 >= CentralMemory.level.timeLimitMinutes)
            {
                if (GameOptions.saveDataThread)
                    SaveData.SaveGameThread(CentralMemory.trialDataNp, GameRunner.header, CentralMemory.filePath, CentralMemory.trialDataList, GameRunner.listCriticalTrialData, CentralMemory.frameDataList, CentralMemory.frameDataFloat, GameManager.game, GameManager.currentGameSummaryPath);
                else
                    SaveData.SaveGame();

                if (GameOptions.isVisualise)
                {
                    SaveData.VisualiseData();
                }
                else
                {
                    Visualizer.AdvanceToNextOptions(); // deals with running from the Game Manager or The Attention Atlas scene
                }
                return;
            }
            else // problem here?
            {
                PopulateTrial();
            }
        }
    }

    public static void SkipLevel(bool isSaveData = false)
    {
        if (isSaveData)
        {
            if (GameOptions.saveDataThread)
                SaveData.SaveGameThread(CentralMemory.trialDataNp, GameRunner.header, CentralMemory.filePath, CentralMemory.trialDataList, GameRunner.listCriticalTrialData, CentralMemory.frameDataList, CentralMemory.frameDataFloat, GameManager.game, GameManager.currentGameSummaryPath);
            else
                SaveData.SaveGame();
        }

        if (GameOptions.isVisualise)
        {
            SaveData.VisualiseData();
        }
        else
        {
            Visualizer.AdvanceToNextOptions(); // deals with running from the Game Manager or The Attention Atlas scene
        }
    }

    public static void LogCurrentTrialData()
    {
        Debug.LogFormat("*******************");
        Debug.LogFormat("currentTrialData.trial:{0}", currentTrialData.trial);
        Debug.LogFormat("currentTrialData.options.descriptor:{0}", currentTrialData.options.descriptor);
        Debug.LogFormat("currentTrialData.options.numberOfRepetitions:{0}", currentTrialData.options.numberOfRepetitions);
        Debug.LogFormat("currentTrialData.options.stimulus:{0}", currentTrialData.options.stimulus);
        Debug.LogFormat("currentTrialData.options.searchMode:{0}", currentTrialData.options.searchMode);
        Debug.LogFormat("currentTrialData.options.coordinates:{0}", currentTrialData.options.coordinates);
        Debug.LogFormat("currentTrialData.options.radius:{0}", currentTrialData.options.radius);
        Debug.LogFormat("currentTrialData.options.keepAngleArray:{0}", currentTrialData.options.keepAngleArray);
        Debug.LogFormat("currentTrialData.options.radiusDepth:{0}", currentTrialData.options.radiusDepth);
    }


    public static void SaveTrialData()
    {
        print("SaveTrialData()");
        CentralMemory.trialDataList.Add(currentTrialData); // duplicated in GameRunner and CentralMemory, but fine for now

        List<float> data = new List<float>()
        {
            currentTrialData.trial,
            currentTrialData.optionsNumber,
            float.NaN, // recursion level 
            currentTrialData.options.radius,

            currentTrialData.cue.timeOnset,
            (float)currentTrialData.cue.frameCountOnset,

            currentTrialData.cue.timeResponse,
            (float)currentTrialData.cue.frameCountResponse,
            (float)currentTrialData.cue.RT,
            GenericFunctions.BoolToFloat(currentTrialData.cue.isCorrect),
 
            currentTrialData.array.timeOnset,
            (float)currentTrialData.array.frameCountOnset,

            currentTrialData.array.timeResponse,
            (float)currentTrialData.array.frameCountResponse,
            (float)currentTrialData.array.RT,
            GenericFunctions.BoolToFloat(currentTrialData.array.isCorrect),

            (float)currentTrialData.targetPosition,
            (float)currentTrialData.targetRotation
        };

        CentralMemory.trialDataNp[trial, Slice.All] = data.ToArray();
        CentralMemory.trialDataFloat.Add(data);

        //if (data.Count != header.Length)
        //{
        //    Debug.LogError("mismatched data and header length");
        //}

        //float[,] dataToPut = new float[1, NetCode.buffer[(int)NetCode.BID.trialData].nChans];

        //for (int i = 0; i < data.Count; i++)
        //{
        //    dataToPut[0, i] = data[i];
        //}
        //NetCode.PutDataThread((int)NetCode.BID.trialData, dataToPut);
    }

    //public void GetTrialDataSamples()
    //{
    //    RealtimeBuffer.Header header = NetCode.GetHeader(NetCode.buffer[(int)NetCode.BID.trialData]);

    //    if (header != null) // potential problems?
    //        nSamples = header.nSamples;
    //}

    public int nSamples;

    private IEnumerator WaitAdvance(float WaitAdvance, TrialData.State nextState)
    {
        yield return new WaitForSeconds(WaitAdvance);

        switch (nextState)
        {
            case TrialData.State.start:
                break;
            case TrialData.State.cue:
                //PresentCue();
                break;
            case TrialData.State.array:
                PresentArray();
                break;
            case TrialData.State.wait:
                break;
            case TrialData.State.feedback:
                PresentFeedback();
                break;
            default:
                break;
        }
    }

    static void PauseGame()
    {
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public static void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isPaused = !isPaused;
            PauseGame();
        }
    }

    public static void CheckToggleAudioInstructions(GameAudio gameAudio)
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            GameOptions.isPlayAudioInstructions = !GameOptions.isPlayAudioInstructions;

            if (GameOptions.isPlayAudioInstructions == false)
            {
                gameAudio.audioSourceOrigin.Stop();
                gameAudio.audioSourceGameRunner.Stop();
            }
        }
    }

    public static void CheckToggleAffirmations(GameAudio gameAudio)
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameOptions.isPlayAffirmations = !GameOptions.isPlayAffirmations;
            
            if (GameOptions.isPlayAffirmations == false)
            {
                gameAudio.audioSourceGameRunner.Stop();
            }
        }
    }

    public static void CheckToggleShowLevelResults()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameOptions.isShowLevelResults = !GameOptions.isShowLevelResults;
        }
    }

    public static void CheckSkipLevel(bool isSaveData = true)
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            print("Input.GetKeyDown(KeyCode.S)");
            SkipLevel(isSaveData: isSaveData);
        }
    }

    public static void AllKeyboardChecks(bool isSaveData = true)
    {
        CheckToggleAudioInstructions(gameAudio);
        CheckToggleAffirmations(gameAudio);
        CheckToggleShowLevelResults();
        CheckPause();
        CheckSkipLevel(isSaveData: isSaveData);
    }


}


