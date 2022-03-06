using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataStructures;

public class ScenePlotter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CentralMemory.observer = new Observer(orientation: GetOrigin.Orientation.positiveX, origin: Vector3.zero);
        GameManager.Level level05Game = new GameManager.Level(gameStartTime: "", descriptor: "level05Game", isGame: true, isFreeViewing: true, timeLimitMinutes: .5f);
        level05Game.listOptions.Add(new Options(radius: 2, coordinates: Options.Coordinates.fullField, keepAngleArray: GameOptions.sphericalSpacing * 3)); // to generate surfaces and vertices to allow analysis to run
        CentralMemory.level = level05Game;

        SceneBuilder.BuildScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
