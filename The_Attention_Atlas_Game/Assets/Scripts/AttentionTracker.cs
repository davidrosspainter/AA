using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using DataStructures;
using System.Collections;

public class AttentionTracker : MonoBehaviour
{
    public static class PointerGlobal
    {
        public static PointerSystem.Pointer.PointerID pointerToUse = PointerSystem.Pointer.PointerID.right;
        public static bool isDisplayPointer = true;
        public static bool isLockOn = false;

        public static class Decision
        {
            public static int? number = null;
            public static string name = null;
            public static int? vertexPosition = null;
            public static SerializableVector3 position = new Vector3(float.NaN, float.NaN, float.NaN);
            public static bool? isCorrect = null;
            public static float? reactionTime = null;
            public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        }
        static public void ClearDecision()
        {
            Decision.number = null;
            Decision.name = null;
            Decision.vertexPosition = null;
            Decision.position = Vector3.zero;
            Decision.isCorrect = null;
            Decision.reactionTime = null;
            Decision.stopwatch.Restart();
        }
    }

    static public string[] targetLayers = { "targets" };
    string[] spriteLayers = new string[] { "sprite" };

    public int? currentAttention = null; // none selected
    public int currentAttentionInspector = -1;
    List<int?> AttentionHistory = new List<int?>();

    Stopwatch stopWatch = new Stopwatch();

    public int numberOfAttentions = 0;
    public List<Vector3> positions = new List<Vector3>();

    void Start()
    {
    }

    void Update()
    {
        if (GameRunner.isPaused)
        {
            return;
        }

        EvaluateAttention(InputManager.controllers[(int)PointerGlobal.pointerToUse].transform, spriteLayers, GameRunner.currentTrialData);
    }


    void CreateConfettiOnTargetHit(string vertexName)
    {
        if (GameRunner.currentTrialData.state != GameRunner.TrialData.State.cue)
        {
            GameObject target = GameObject.Find("surface/vertexGroup/vertex." + vertexName);
            UnityEngine.Debug.LogFormat("target == null:{0}", target == null);
            GameObject confetti = Instantiate(Resources.Load("prefabs/Confetti", typeof(GameObject))) as GameObject;
            UnityEngine.Debug.LogFormat("confetti == null:{0}", confetti == null);
            confetti.transform.position = target.transform.position;
            confetti.SetActive(true);
            StartCoroutine(KillConfetti(confetti));
        }

        IEnumerator KillConfetti(GameObject confetti2)
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(confetti2);
        }
    }

    void EvaluateAttention(Transform transform, string[] layerNames, GameRunner.TrialData trialData)
    {
        int index;

        switch (transform.gameObject.name)
        {
            case "Controller (left)": index = 0; break;
            case "Controller (right)": index = 1; break;
            default: index = 0; break;
        }

        (string colliderName, RaycastHit hit) = RaycastDRP(transform, layerNames);

        if (colliderName != "") // something selected
        {

            var parentName = hit.transform.parent.name;
            var grandparentName = hit.transform.parent.parent.name;

            string[] tmp = grandparentName.Split('.');
            //UnityEngine.Debug.LogFormat("grandparentName: {0}", grandparentName);

            currentAttention = System.Convert.ToInt32(tmp[tmp.Length - 1]);

            if (currentAttention == null)
                currentAttentionInspector = -1;
            else
                currentAttentionInspector = (int)currentAttention;

            ColorSprites2(currentAttention, GameOptions.Colors.selected); // color selected

            if (AttentionHistory.Count == 0)
            {
                AttentionHistory.Add(currentAttention);
                numberOfAttentions++;
                stopWatch.Reset(); stopWatch.Start();
            }
            else
            {
                if (currentAttention != AttentionHistory[AttentionHistory.Count - 1])
                {
                    if (AttentionHistory[AttentionHistory.Count - 1] != null)
                    {
                        ColorSprites2(AttentionHistory[AttentionHistory.Count - 1], GameRunner.currentTrialData.elements.listElements[(int)AttentionHistory[AttentionHistory.Count - 1]].color); // uncolor previous
                    }

                    numberOfAttentions++;
                    AttentionHistory.Add(currentAttention);
                    stopWatch.Reset(); stopWatch.Start();
                }
                else
                {
                    if (InputManager.IsButtonPressed(GameOptions.responseButton, index) & !PointerGlobal.isLockOn)
                    {
                        PointerGlobal.Decision.number = currentAttention;
                        PointerGlobal.Decision.name = hit.collider.gameObject.name; // print(PointerGlobal.Decision.name);
                        PointerGlobal.Decision.vertexPosition = currentAttention;

                        PointerGlobal.Decision.reactionTime = (float?)PointerGlobal.Decision.stopwatch.Elapsed.TotalMilliseconds;
 
                        print(Time.time.ToString() + ',' + Time.frameCount.ToString() + ", RT =" + PointerGlobal.Decision.reactionTime.ToString());
                        
                        if (currentAttention == GameRunner.currentTrialData.targetPosition | GameRunner.currentTrialData.state == GameRunner.TrialData.State.cue)
                        {
                            PointerGlobal.Decision.isCorrect = true;
                            print("correct");

                            if (GameOptions.isDisplayConfetti)
                                CreateConfettiOnTargetHit(currentAttention.ToString());
                        }
                        else
                        {
                            PointerGlobal.Decision.isCorrect = false;
                            print("incorrect");
                        }

                        print(Time.time.ToString() + ',' + Time.frameCount.ToString() + ", Accuracy =" + PointerGlobal.Decision.isCorrect.ToString());

                        switch (GameRunner.currentTrialData.state)
                        {
                            case GameRunner.TrialData.State.cue:
                                GameRunner.currentTrialData.cue.timeResponse = Time.time;
                                GameRunner.currentTrialData.cue.frameCountResponse = Time.frameCount;
                                GameRunner.currentTrialData.cue.RT = (float)PointerGlobal.Decision.reactionTime;
                                GameRunner.currentTrialData.cue.isCorrect = (bool)PointerGlobal.Decision.isCorrect;
                                break;
                            case GameRunner.TrialData.State.array:
                                GameRunner.currentTrialData.array.timeResponse = Time.time;
                                GameRunner.currentTrialData.array.frameCountResponse = Time.frameCount;
                                GameRunner.currentTrialData.array.RT = (float)PointerGlobal.Decision.reactionTime;
                                GameRunner.currentTrialData.array.isCorrect = (bool)PointerGlobal.Decision.isCorrect;
                                break;
                            default: throw new System.Exception();
                        }

                        PointerGlobal.Decision.position = hit.collider.transform.position;
                        PointerGlobal.isLockOn = true;
                    }
                }
            }
        }

        else // nothing selected
        {
            stopWatch.Reset();

            if (currentAttention != null)
            {
                //UnityEngine.Debug.LogFormat("(int)currentAttention:{0}", (int)currentAttention);
                ColorSprites2(currentAttention, GameRunner.currentTrialData.elements.listElements[(int)currentAttention].color); // uncolor previous
                currentAttention = null;
                currentAttentionInspector = -1;

                AttentionHistory.Add(null); // nothing selected
            }

        }

        // ----- lock
        // to require users to lift the button
        if (InputManager.AreButtonsUnpressed(GameOptions.responseButton))
        {
            PointerGlobal.isLockOn = false;
        }
    }


    void ColorSprites2(int? vertex, Color spriteColor) // parenting seems needlessly complex with suboptimal performance?
    {
        string gameObjectName = "surface/vertexGroup/vertex." + vertex.ToString();
        GameObject spriteGameObject = GameObject.Find(gameObjectName);
        //UnityEngine.Debug.LogFormat("gameObjectName:{0}", spriteGameObject);

        if (spriteGameObject == null)
        {
            return;
        }
        foreach (Transform child in spriteGameObject.transform)
        {
            foreach (Transform grandchild in child.transform)
            {
                if (grandchild.gameObject.HasComponent<SpriteRenderer>()) // all sprites
                    if (GameRunner.currentTrialData.options.searchMode == Options.SearchMode.none & (GameRunner.currentTrialData.options.stimulus == Options.Stimulus.food | GameRunner.currentTrialData.options.stimulus == Options.Stimulus.food))
                        grandchild.GetComponent<SpriteRenderer>().sprite = Stimuli.foodSprites[GameRunner.currentTrialData.elements.listElements[(int)vertex].spriteIndex]; // revert to original!
                    else
                        grandchild.GetComponent<SpriteRenderer>().color = spriteColor;

                else // balloons
                {
                    if (GameRunner.currentTrialData.options.stimulus == Options.Stimulus.balloons)
                    {
                        foreach (Transform grandGrandchild in grandchild.transform)
                        {
                            grandGrandchild.GetComponent<MeshRenderer>().material.color = spriteColor;
                        }
                    }
                }
            }
        }
    }

    public static (string, RaycastHit) RaycastDRP(Transform transform, string[] layerNames)
    {
        int layerMask = LayerMask.GetMask(layerNames);
        string colliderName = "";

        Ray ray = new Ray(transform.position, transform.forward); // Ray from the controller
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask)) // hit
        {
            colliderName = hit.collider.gameObject.name;
        }
        return (colliderName, hit);
    }
}
