using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public AudioSource audioSourceOrigin;
    public AudioSource audioSourceGameRunner;
    public AudioSource audioSourceFeedback;

    // instructions
    public AudioClip instructions1;
    public AudioClip instructions2;
    public AudioClip instructions3;
    public AudioClip instructions4;
    public AudioClip instructions5;
    public AudioClip instructions6;
    public AudioClip instructions7;
    public AudioClip instructions8;
    public AudioClip instructions9;
    public AudioClip instructions10;
    public AudioClip instructions11;
    public AudioClip instructions12;
    public AudioClip instructions13;
    public AudioClip instructions14;
    public AudioClip instructions15;
    public AudioClip instructions16;

    // game sounds feedback
    public AudioClip levelUp;
    public AudioClip correct;
    public AudioClip incorrect;

    // verbal feedback
    public AudioClip greatJob;
    public AudioClip niceWork;
    public AudioClip wellDone;

    public List<AudioClip> affirmations;

    private void Start()
    {
        audioSourceOrigin = GameObject.Find("AudioSources/GetOrigin").GetComponent<AudioSource>();
        audioSourceGameRunner = GameObject.Find("AudioSources/GameRunner").GetComponent<AudioSource>();
        audioSourceFeedback = GameObject.Find("AudioSources/feedback").GetComponent<AudioSource>();

        levelUp = Resources.Load("sounds/DM-CGS-26") as AudioClip;
        correct = Resources.Load("sounds/DM-CGS-45") as AudioClip;
        incorrect = Resources.Load("sounds/DM-CGS-46") as AudioClip;

        affirmations = new List<AudioClip> { greatJob, niceWork, wellDone };

        audioSourceFeedback.volume = .1f;
    }
}
