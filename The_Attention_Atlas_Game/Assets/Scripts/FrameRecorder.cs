using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using DataStructures;
using System;

public class FrameRecorder : MonoBehaviour
{
    [Serializable]
    public class FrameData
    {
        public float time;
        public float frameCount;

        public int trial;
        public int optionsNumber;
        public GameRunner.TrialData.State state;

        public int trialframeCount; /// ????
        public float trialTime;

        public SerializableVector3 origin;

        public EssentialTransform headset;
        public EssentialTransform eyeGaze;
        public EssentialTransform controller0;
        public EssentialTransform controller1;

        public SerializableVector3 headsetSurfacePosition;
        public SerializableVector3 eyeGazeSurfacePosition;
        public SerializableVector3 controller0SurfacePosition;
        public SerializableVector3 controller1SurfacePosition;

        public FrameData(int trial,
                         GameRunner.TrialData.State state,
                         int optionsNumber,
                         SerializableVector3 origin,
                         EssentialTransform headset,
                         EssentialTransform eyeGaze,
                         EssentialTransform controller0,
                         EssentialTransform controller1,
                         SerializableVector3 headsetSurfacePosition,
                         SerializableVector3 eyeGazeSurfacePosition,
                         SerializableVector3 controller0SurfacePosition,
                         SerializableVector3 controller1SurfacePosition)
        {
            time = Time.time;
            frameCount = Time.frameCount;

            this.trial = trial;
            this.state = state;
            this.optionsNumber = optionsNumber;

            this.origin = origin;

            this.headset = headset;
            this.eyeGaze = eyeGaze;
            this.controller0 = controller0;
            this.controller1 = controller1;

            this.headsetSurfacePosition = headsetSurfacePosition;
            this.eyeGazeSurfacePosition = eyeGazeSurfacePosition;
            this.controller0SurfacePosition = controller0SurfacePosition;
            this.controller1SurfacePosition = controller1SurfacePosition;
        }

        public FrameData()
        {
            time = float.NaN;
            frameCount = -1;

            trial = -1;
            state = GameRunner.TrialData.State.wait;

            origin = new Vector3(float.NaN, float.NaN, float.NaN);

            headset = new EssentialTransform();
            eyeGaze = new EssentialTransform();
            controller0 = new EssentialTransform();
            controller1 = new EssentialTransform();

            headsetSurfacePosition = new Vector3(float.NaN, float.NaN, float.NaN);
            eyeGazeSurfacePosition = new Vector3(float.NaN, float.NaN, float.NaN);
            controller0SurfacePosition = new Vector3(float.NaN, float.NaN, float.NaN);
            controller1SurfacePosition = new Vector3(float.NaN, float.NaN, float.NaN);
        }
    }


    public static string[] header = {
            "Time.time",
            "Time.frameCount",

            // ---- eye tracking general

            "eyeTrackingData.Timestamp",

            "GeneralMethods.BoolToFloat(eyeTrackingData.IsLeftEyeBlinking)",
            "GeneralMethods.BoolToFloat(eyeTrackingData.IsRightEyeBlinking)",
            "eyeTrackingData.ConvergenceDistance",
            "GeneralMethods.BoolToFloat(eyeTrackingData.ConvergenceDistanceIsValid)",
            "GeneralMethods.BoolToFloat(eyeTrackingData.GazeRay.IsValid)",
            
            // ---- eye tracking raycasts
            
            "eyeTrackingData.GazeRay.Origin.x",
            "eyeTrackingData.GazeRay.Origin.y",
            "eyeTrackingData.GazeRay.Origin.z",

            "eyeTrackingData.GazeRay.Direction.x",
            "eyeTrackingData.GazeRay.Direction.y",
            "eyeTrackingData.GazeRay.Direction.z",

            "eyeGazeSurfacePosition.x",
            "eyeGazeSurfacePosition.y",
            "eyeGazeSurfacePosition.z",

            // ---- headset raycasts

            "ViveInput1.Transforms.headset.transform.position.x",
            "ViveInput1.Transforms.headset.transform.position.y",
            "ViveInput1.Transforms.headset.transform.position.z",

            "ViveInput1.Transforms.headset.transform.rotation.w",
            "ViveInput1.Transforms.headset.transform.rotation.x",
            "ViveInput1.Transforms.headset.transform.rotation.y",
            "ViveInput1.Transforms.headset.transform.rotation.z",

            "ViveInput1.Transforms.headset.transform.forward.x",
            "ViveInput1.Transforms.headset.transform.forward.y",
            "ViveInput1.Transforms.headset.transform.forward.z",

            "headsetSurfacePosition.x",
            "headsetSurfacePosition.y",
            "headsetSurfacePosition.z",

            // ---- controller 0 raycasts

            "ViveInput1.Transforms.controllers[0].transform.position.x",
            "ViveInput1.Transforms.controllers[0].transform.position.y",
            "ViveInput1.Transforms.controllers[0].transform.position.z",

            "ViveInput1.Transforms.controllers[0].transform.rotation.w",
            "ViveInput1.Transforms.controllers[0].transform.rotation.x",
            "ViveInput1.Transforms.controllers[0].transform.rotation.y",
            "ViveInput1.Transforms.controllers[0].transform.rotation.z",

            "ViveInput1.Transforms.controllers[0].transform.forward.x",
            "ViveInput1.Transforms.controllers[0].transform.forward.y",
            "ViveInput1.Transforms.controllers[0].transform.forward.z",

            "controller0SurfacePosition.x",
            "controller0SurfacePosition.y",
            "controller0SurfacePosition.z",

            // ---- controller 1 raycasts

            "ViveInput1.Transforms.controllers[1].transform.position.x",
            "ViveInput1.Transforms.controllers[1].transform.position.y",
            "ViveInput1.Transforms.controllers[1].transform.position.z",

            "ViveInput1.Transforms.controllers[1].transform.rotation.w",
            "ViveInput1.Transforms.controllers[1].transform.rotation.x",
            "ViveInput1.Transforms.controllers[1].transform.rotation.y",
            "ViveInput1.Transforms.controllers[1].transform.rotation.z",

            "ViveInput1.Transforms.controllers[1].transform.forward.x",
            "ViveInput1.Transforms.controllers[1].transform.forward.y",
            "ViveInput1.Transforms.controllers[1].transform.forward.z",

            "controller1SurfacePosition.x",
            "controller1SurfacePosition.y",
            "controller1SurfacePosition.z",

            // ---- trial data

            "GameRunner.trialData.trial",
            "(float)GameRunner.currentTrialData.optionsNumber",
            "(float)GameRunner.currentTrialData.options.recursionLevel",
            "GameRunner.trialData.radius",

            "(float) GameRunner.trialData.state",

            "(float) GameRunner.trialData.origin.x",
            "(float) GameRunner.trialData.origin.y",
            "(float) GameRunner.trialData.origin.z",

            "(float) GameRunner.trialData.yRotation",

            "(float) PointerGlobal.Decision.stopwatch.Elapsed.TotalMilliseconds",

            // ----- new
            "GenericFunctions.BoolToFloat(InputManager.headset.isTracked)",
            "GenericFunctions.BoolToFloat(InputManager.headset.isProximitySensorActivated)",
            "GenericFunctions.BoolToFloat(InputManager.controllers[0].essentialTransform.isTracked)",
            "GenericFunctions.BoolToFloat(InputManager.controllers[1].essentialTransform.isTracked)",
            "(float) PointerGlobal.pointerToUse",
            };

    public int nFrames = 0;
    public int nSamples = 0;

    public static string[] layerNames = new string[] { "mainSurface" };

    GameObject sphereGaze;

    void Start()
    {
        var settings = new TobiiXR_Settings();
        TobiiXR.Start(settings);
        
        sphereGaze = new GameObject("sphere");
    }

    void GetVeboseEyeData() // to implement
    {
        ViveSR.anipal.Eye.EyeData_v2 eye_data = new ViveSR.anipal.Eye.EyeData_v2();
        ViveSR.anipal.Eye.SRanipal_Eye_API.GetEyeData_v2(ref eye_data);
        ViveSR.anipal.Eye.SRanipal_Eye_v2.GetVerboseData(out ViveSR.anipal.Eye.VerboseData verbose_data);

        float pupilDiameterLeft = verbose_data.left.pupil_diameter_mm;
        float pupilDiameterRight = verbose_data.left.pupil_diameter_mm;

        float eyeOpennessLeft = verbose_data.left.eye_openness;
        float eyeOpennessRight = verbose_data.right.eye_openness;
    }

    void GetRayCastFrameData()
    {
        // ----- Raycast - eye gaze...
        TobiiXR_EyeTrackingData eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World); // Get eye tracking data in world space

        Vector3 rayOrigin = new Vector3(float.NaN, float.NaN, float.NaN); // The origin of the gaze ray is a 3D point
        Vector3 rayDirection = new Vector3(float.NaN, float.NaN, float.NaN); // The direction of the gaze ray is a normalized direction vector
        Vector3 eyeGazeSurfacePosition = new Vector3(float.NaN, float.NaN, float.NaN);

        if (eyeTrackingData.GazeRay.IsValid) // Check if gaze ray is valid
        {
            rayOrigin = eyeTrackingData.GazeRay.Origin; // The origin of the gaze ray is a 3D point
            rayDirection = eyeTrackingData.GazeRay.Direction; // The direction of the gaze ray is a normalized direction vector
            eyeGazeSurfacePosition = GetSurfacePosition(rayOrigin, rayDirection, false, sphereGaze);
        }

        // ---- Raycasts: headsets and controllers
        Vector3 headsetSurfacePosition = GetSurfacePosition(InputManager.headset.transform.position, InputManager.headset.transform.forward, false, new GameObject());
        Vector3 controller0SurfacePosition = GetSurfacePosition(InputManager.controllers[0].transform.position, InputManager.controllers[0].transform.forward, false, new GameObject());
        Vector3 controller1SurfacePosition = GetSurfacePosition(InputManager.controllers[1].transform.position, InputManager.controllers[1].transform.forward, false, new GameObject());

        // save as frame data...
        CentralMemory.frameDataList.Add(new FrameData(trial: GameRunner.currentTrialData.trial,
                                               state: GameRunner.currentTrialData.state,
                                               optionsNumber: GameRunner.currentTrialData.optionsNumber,

                                               origin: CentralMemory.observer.origin, // CentralMemory.observer.origin

                                               headset: new EssentialTransform(InputManager.headset.transform),
                                               eyeGaze: new EssentialTransform(position: rayOrigin, forward: rayDirection),
                                               controller0: new EssentialTransform(InputManager.controllers[0].transform),
                                               controller1: new EssentialTransform(InputManager.controllers[1].transform),

                                               headsetSurfacePosition: headsetSurfacePosition,
                                               eyeGazeSurfacePosition: eyeGazeSurfacePosition,
                                               controller0SurfacePosition: controller0SurfacePosition,
                                               controller1SurfacePosition: controller1SurfacePosition));

        //Debug.LogFormat("GameRunner.currentTrialData.optionsNumber{0}:", GameRunner.currentTrialData.optionsNumber);

        nFrames = CentralMemory.frameDataList.Count;

        List<float> attentionData = new List<float>
        {

            Time.time,
            Time.frameCount,

            // ----

            eyeTrackingData.Timestamp,
            GenericFunctions.BoolToFloat(eyeTrackingData.IsLeftEyeBlinking),
            GenericFunctions.BoolToFloat(eyeTrackingData.IsRightEyeBlinking),
            eyeTrackingData.ConvergenceDistance,
            GenericFunctions.BoolToFloat(eyeTrackingData.ConvergenceDistanceIsValid),
            GenericFunctions.BoolToFloat(eyeTrackingData.GazeRay.IsValid),
            
            // ----
            
            eyeTrackingData.GazeRay.Origin.x,
            eyeTrackingData.GazeRay.Origin.y,
            eyeTrackingData.GazeRay.Origin.z,

            eyeTrackingData.GazeRay.Direction.x,
            eyeTrackingData.GazeRay.Direction.y,
            eyeTrackingData.GazeRay.Direction.z,

            eyeGazeSurfacePosition.x,
            eyeGazeSurfacePosition.y,
            eyeGazeSurfacePosition.z,

            // ----

            InputManager.headset.transform.position.x,
            InputManager.headset.transform.position.y,
            InputManager.headset.transform.position.z,

            InputManager.headset.transform.rotation.w,
            InputManager.headset.transform.rotation.x,
            InputManager.headset.transform.rotation.y,
            InputManager.headset.transform.rotation.z,

            InputManager.headset.transform.forward.x,
            InputManager.headset.transform.forward.y,
            InputManager.headset.transform.forward.z,

            headsetSurfacePosition.x,
            headsetSurfacePosition.y,
            headsetSurfacePosition.z,

            // ----

            InputManager.controllers[0].transform.position.x,
            InputManager.controllers[0].transform.position.y,
            InputManager.controllers[0].transform.position.z,

            InputManager.controllers[0].transform.rotation.w,
            InputManager.controllers[0].transform.rotation.x,
            InputManager.controllers[0].transform.rotation.y,
            InputManager.controllers[0].transform.rotation.z,

            InputManager.controllers[0].transform.forward.x,
            InputManager.controllers[0].transform.forward.y,
            InputManager.controllers[0].transform.forward.z,

            controller0SurfacePosition.x,
            controller0SurfacePosition.y,
            controller0SurfacePosition.z,

            // ----

            InputManager.controllers[1].transform.position.x,
            InputManager.controllers[1].transform.position.y,
            InputManager.controllers[1].transform.position.z,

            InputManager.controllers[1].transform.rotation.w,
            InputManager.controllers[1].transform.rotation.x,
            InputManager.controllers[1].transform.rotation.y,
            InputManager.controllers[1].transform.rotation.z,

            InputManager.controllers[1].transform.forward.x,
            InputManager.controllers[1].transform.forward.y,
            InputManager.controllers[1].transform.forward.z,

            controller1SurfacePosition.x,
            controller1SurfacePosition.y,
            controller1SurfacePosition.z,

            // ----

            (float)GameRunner.currentTrialData.trial, // cast is required?
            (float)GameRunner.currentTrialData.optionsNumber,
            (float)GameRunner.currentTrialData.options.recursionLevel,
            GameRunner.currentTrialData.options.radius,
            (float)GameRunner.currentTrialData.state,

            CentralMemory.observer.origin.x,
            CentralMemory.observer.origin.y,
            CentralMemory.observer.origin.z,

            float.NaN, // yrotation

            (float)AttentionTracker.PointerGlobal.Decision.stopwatch.Elapsed.TotalMilliseconds,

            // new
            GenericFunctions.BoolToFloat(InputManager.headset.isTracked),
            GenericFunctions.BoolToFloat(InputManager.headset.isProximitySensorActivated),
            GenericFunctions.BoolToFloat(InputManager.controllers[0].essentialTransform.isTracked),
            GenericFunctions.BoolToFloat(InputManager.controllers[1].essentialTransform.isTracked),
            (float)AttentionTracker.PointerGlobal.pointerToUse,
            
        };

        CentralMemory.frameDataFloat.Add(attentionData);

        //float[,] dataToPut = new float[1, NetCode.buffer[(int)NetCode.BID.frameData].nChans];

        //for (int i = 0; i < attentionData.Count; i++)
        //{
        //    dataToPut[0, i] = attentionData[i];
        //}

        //NetCode.PutDataNew((int)NetCode.BID.frameData, dataToPut);

        //NetCode.PutDataThread((int)NetCode.BID.frameData, dataToPut); // source of errors!?
        //NetCode.PutDataNew((int)NetCode.BID.frameData, dataToPut);
        //NetCode.PutDataThreadNew((int)NetCode.BID.frameData, dataToPut);

        //try
        //{
        //    RealtimeBuffer.Header header = NetCode.GetHeader(NetCode.buffer[(int)NetCode.BID.frameData]);
        //    nSamples = header.nSamples;
        //}
        //catch
        //{
        //    print("Failed to get raycastFrameData header"); // problem?
        //}

    }

    void Update()
    {
        if (!GetComponent<GetOrigin>().enabled & !GameRunner.isPaused) // for performance reasons, exclude origin period...?
        {
            GetRayCastFrameData();
        }
    }

    Vector3 GetSurfacePosition(Vector3 rayOrigin, Vector3 rayDirection, bool isVisualise, GameObject sphere)
    {
        Destroy(sphere);
        
        string colliderName = "";
        RaycastHit hit = new RaycastHit();

        (colliderName, hit) = RayCastNew(layerNames, rayOrigin, rayDirection);

        Vector3 position = Vector3.zero;

        if (colliderName != "")
        {
            //print(colliderName);

            position = hit.point;

            if (isVisualise)
            {
                Destroy(sphere);
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "sphere";
                sphere.transform.position = position;
            }
        }
        return position;
    }

    public static (string, RaycastHit) RayCastNew(string[] layerNames, Vector3 rayOrigin, Vector3 rayDirection)
    {
        int layerMask = LayerMask.GetMask(layerNames);
        string colliderName = "";

        Ray ray = new Ray(rayOrigin, rayDirection); // Ray from the controller
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask)) // hit
        {
            colliderName = hit.collider.gameObject.name;
        }

        return (colliderName, hit);
    }
}
