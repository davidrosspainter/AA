using System.Collections.Generic;
using UnityEngine;
using DataStructures;
using System;
using System.IO;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    // lighting mode of target scene should have baked unchecked so that the editor and scene transitions show the same lighting

    public static string gamesLogPath;
    public static string gameStartTime;
    public static string currentGameSummaryPath;
    public static int currentLevel = -1;

    [Serializable]
    public class Level
    {
        public string gameStartTime;
        public string levelStartTime;

        public string descriptor;
        public List<Options> listOptions;
        public bool isFreeViewing;
        public bool isRandomiseTrialOrder;
        public bool isGame; // true for game, false for tutorial - for integration with analyses - to immplement
        public float timeLimitMinutes; // minutes

        public Level(string gameStartTime = "", string levelStartTime = "", string descriptor = "", List<Options> listOptions = null, bool isFreeViewing = false, bool isRandomiseTrialOrder = false, bool isGame = false, float timeLimitMinutes = float.PositiveInfinity)
        {
            this.gameStartTime = gameStartTime;
            this.levelStartTime = levelStartTime;
            this.descriptor = descriptor;

            if (listOptions == null)
                this.listOptions = new List<Options>();
            else
                this.listOptions = listOptions;

            this.isFreeViewing = isFreeViewing;
            this.isRandomiseTrialOrder = isRandomiseTrialOrder;
            this.isGame = isGame;
            this.timeLimitMinutes = timeLimitMinutes;
        }
    }

    [Serializable]
    public class Game
    {
        public string startTime;
        public string descriptor;
        public List<Level> listLevels;

        public Game(string startTime = "", string descriptor = "", List<Level> listLevels = null)
        {
            this.startTime = startTime;
            this.descriptor = descriptor;

            if (listLevels == null)
                this.listLevels = new List<Level>();
            else
                this.listLevels = listLevels;
        }
    }

    public static Game game = new Game();

    // Summmary: all static variables must be manually reset if required to revert to original state!
    // static variables persist when scene in unloaded
    // non-static variables do not persist when scene is unloaded (reverts to original state)
    // script state (enabled/disabled) does not persist when scene is unloaded (reverts to original state)
    // game objects do not persist when scene is unloaded (reverts to original state)

    public static string SetupGamesLogPath(string startTime)
    {
        gamesLogPath = Path.GetFullPath(Path.Combine(@Application.dataPath, CentralMemory.ReturnPathModifier(), @"_DATA\")) + startTime + @"\";
        Debug.LogFormat("gamesLogPath:{0}", gamesLogPath);

        if (!Directory.Exists(gamesLogPath))
        {
            Directory.CreateDirectory(gamesLogPath);
        }
        return gamesLogPath;
    }

    public static string GetNewStartTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff");
    }

    void PopulateRecordings()
    {
        print("GameManager.PopulateRecordings()");

        gameStartTime = GetNewStartTime(); // for folder structure
        Debug.LogFormat("gameStartTime:{0}", gameStartTime);
        gamesLogPath = SetupGamesLogPath(gameStartTime);
        currentGameSummaryPath = gamesLogPath + gameStartTime + ".game.json";

        // Connor instructions
        // letters2 - should be all "Ts" - so maybe make a letter2 options with target upright and everything else rotated.
        // cards2 - make distractor cards randomly selected only from jack, king, and queen - use all suits
        // depth - scale outer depth by outer depth radius so that both depths have same apparent size
        // search mode - check colours are working properly
        // food - make it so that distractors are all unique items (too hard basket)
        // food - outlines as white? (david to do?)
        // run through full version, check game is running properly and analysis runs, that there are no errors - check overall timing - can use phone timer should be 28.7 minutes + origin time + level load time (dependent on game save time) ~ 30 minutes is my guess
        // build the game and run through again - check timing
        // start work on user interface for GameManager config. - please you Windows Forms and the latest .net framework; as a start - the interface generates a .json file with a game structure

        if (GamePanelController.isUseUI)
        {
            game = SaveData.ReadJson<Game>(GamePanelController.ReturnPresetsDirectory() + "last.game.json");
            game.startTime = gameStartTime; // for display in UI panel and for saving within the data structures to disk
        }
        else
        {

            // Tutorial
            Level tutorial = new Level(gameStartTime: gameStartTime, descriptor: "Tutorial", isGame: false, isRandomiseTrialOrder: true, timeLimitMinutes: 0.5f); // 8 targets
            tutorial.listOptions.Add(new Options(
                descriptor: "oneRingUniqueFeature",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.uniqueFeature,
                coordinates: Options.Coordinates.fullField,
                keepAngleArray: GameOptions.sphericalSpacing * 1,
                radius: 2));

            //Level pairs = new Level(gameStartTime: gameStartTime, descriptor: "(1) Pairs", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 1.7f); // 8 targets
            //    pairs.listOptions.Add(new Options(
            //       descriptor: "pair1",
            //       numberOfRepetitions: 1,
            //       stimulus: Options.Stimulus.letters,
            //       searchMode: Options.SearchMode.serial,
            //       coordinates: Options.Coordinates.pair1,
            //       radius: 2));
            //    pairs.listOptions.Add(new Options(
            //        descriptor: "pair2",
            //        numberOfRepetitions: 1,
            //        stimulus: Options.Stimulus.letters,
            //        searchMode: Options.SearchMode.serial,
            //        coordinates: Options.Coordinates.pair2,
            //        radius: 2));
            //    pairs.listOptions.Add(new Options(
            //        descriptor: "pair3",
            //        numberOfRepetitions: 1,
            //        stimulus: Options.Stimulus.letters,
            //        searchMode: Options.SearchMode.serial,
            //        coordinates: Options.Coordinates.pair3,
            //        radius: 2));
            //    pairs.listOptions.Add(new Options(
            //        descriptor: "pair4",
            //        numberOfRepetitions: 1,
            //        stimulus: Options.Stimulus.letters,
            //        searchMode: Options.SearchMode.serial,
            //        coordinates: Options.Coordinates.pair4,
            //        radius: 2));

            Level horizontalVertical = new Level(gameStartTime: gameStartTime, descriptor: "(2) Horizontal/Vertical", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 3.0f); // 16 targets (2)
            horizontalVertical.listOptions.Add(new Options(
               descriptor: "horizontal",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.letters,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.horizontal,
               radius: 2));
            horizontalVertical.listOptions.Add(new Options(
               descriptor: "vertical",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.letters,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.vertical,
               radius: 2));

            //Level searchModes = new Level(gameStartTime: gameStartTime, descriptor: "(3) Search Modes", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 5.1f); // 24 targets
            //    searchModes.listOptions.Add(new Options(
            //        descriptor: "oneRingUniqueFeature",
            //        numberOfRepetitions: 1,
            //        stimulus: Options.Stimulus.letters,
            //        searchMode: Options.SearchMode.uniqueFeature,
            //        coordinates: Options.Coordinates.fullField,
            //        keepAngleArray: GameOptions.sphericalSpacing * 1,
            //        radius: 2));
            //    searchModes.listOptions.Add(new Options(
            //        descriptor: "oneRingConjunction",
            //        numberOfRepetitions: 1,
            //        stimulus: Options.Stimulus.letters,
            //        searchMode: Options.SearchMode.conjunction,
            //        coordinates: Options.Coordinates.fullField,
            //        keepAngleArray: GameOptions.sphericalSpacing * 1,
            //        radius: 2));
            //    searchModes.listOptions.Add(new Options(
            //        descriptor: "oneRingSerial2",
            //        numberOfRepetitions: 1,
            //        stimulus: Options.Stimulus.letters,
            //        searchMode: Options.SearchMode.serial2,
            //        coordinates: Options.Coordinates.fullField,
            //        keepAngleArray: GameOptions.sphericalSpacing * 1,
            //        radius: 2));

            Level stimuli = new Level(gameStartTime: gameStartTime, descriptor: "(4) Stimuli", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 4.5f); // 24 targets
            stimuli.listOptions.Add(new Options(
               descriptor: "oneRingFood",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.food,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.fullField,
               keepAngleArray: GameOptions.sphericalSpacing * 1,
               radius: 2));
            stimuli.listOptions.Add(new Options(
               descriptor: "oneRingCards",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.cards,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.fullField,
               keepAngleArray: GameOptions.sphericalSpacing * 1,
               radius: 2));
            stimuli.listOptions.Add(new Options(
               descriptor: "oneRingBalloons",
               numberOfRepetitions: 1,
               stimulus: Options.Stimulus.balloons,
               searchMode: Options.SearchMode.serial,
               coordinates: Options.Coordinates.fullField,
               keepAngleArray: GameOptions.sphericalSpacing * 1,
               radius: 2));

            Level depth = new Level(gameStartTime: gameStartTime, descriptor: "(5) Depth", isGame: true, isRandomiseTrialOrder: false, timeLimitMinutes: 6.0f); // 32 targets
            depth.listOptions.Add(new Options(
                descriptor: "depthConfig1",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.depthConfig1,
                radius: 2,
                radiusDepth: 4,
                keepAngleArray: GameOptions.sphericalSpacing * 2));
            depth.listOptions.Add(new Options(
                descriptor: "depthConfig2",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.depthConfig2,
                radius: 2,
                radiusDepth: 4,
                keepAngleArray: GameOptions.sphericalSpacing * 2));

            Level fullField = new Level(gameStartTime: gameStartTime, descriptor: "(6) Full Field", isGame: true, isRandomiseTrialOrder: false, timeLimitMinutes: 4.5f); // 24 targets
            fullField.listOptions.Add(new Options(
                descriptor: "fullFieldSerial",
                numberOfRepetitions: 1,
                stimulus: Options.Stimulus.letters,
                searchMode: Options.SearchMode.serial,
                coordinates: Options.Coordinates.fullField,
                radius: 2,
                keepAngleArray: GameOptions.sphericalSpacing * 3));


            Level freeViewing = new Level(gameStartTime: gameStartTime, descriptor: "(7) Free Viewing", isGame: true, isFreeViewing: true, timeLimitMinutes: 1.0f);
            freeViewing.listOptions.Add(new Options(radius: 2, coordinates: Options.Coordinates.fullField, keepAngleArray: GameOptions.sphericalSpacing * 3)); // to generate surfaces and vertices to allow analysis to run

            // game
            game = new Game(startTime: gameStartTime,
                            descriptor: "main game",
                            listLevels: new List<Level> { tutorial, 
                                                      //pairs,
                                                      horizontalVertical,
                                                      //    searchModes,
                                                      stimuli,
                                                      depth,
                                                      fullField,
                                                      freeViewing,
                                                        });
        }
        //foreach (Level level in game.listLevels)
        //{
        //    level.timeLimitMinutes *= GameOptions.durationFactor;
        //}

        //Level food = new Level(gameStartTime: gameStartTime, descriptor: "Level 4: Stimuli", isGame: true, isRandomiseTrialOrder: false, timeLimitMinutes: .5f); // 24 targets
        //food.listOptions.Add(new Options(
        //   descriptor: "oneRingFood",
        //   numberOfRepetitions: 1,
        //   stimulus: Options.Stimulus.food,
        //   searchMode: Options.SearchMode.serial,
        //   coordinates: Options.Coordinates.fullField,
        //   keepAngleArray: GameOptions.sphericalSpacing * 1,
        //   radius: 2));

        //Level cards = new Level(gameStartTime: gameStartTime, descriptor: "(4) Stimuli", isGame: true, isRandomiseTrialOrder: true, timeLimitMinutes: 1f); // 24 targets
        //cards.listOptions.Add(new Options(
        //   descriptor: "oneRingCards",
        //   numberOfRepetitions: 1,
        //   stimulus: Options.Stimulus.cards,
        //   searchMode: Options.SearchMode.serial,
        //   coordinates: Options.Coordinates.fullField,
        //   keepAngleArray: GameOptions.sphericalSpacing * 1,
        //   radius: 2));

        //game = new Game(startTime: gameStartTime,
        //        descriptor: "main game",
        //        listLevels: new List<Level> { cards
        //                                    });

        if(GameOptions.saveDataThread)
            SaveData.SaveGameConfigurationThread(game, currentGameSummaryPath);
        else
            SaveGameConfiguration();
    }

    public static void SaveGameConfiguration()
    {
        Debug.LogFormat("game.listLevels:{0}", game.listLevels.Count); // tests data is saved properly
        game = SaveData.WriteAndReadJson<Game>(game, currentGameSummaryPath); // save settings
        Debug.LogFormat("game.listLevels:{0}", game.listLevels.Count); // tests data is saved properly
    }

    public static void SaveGameConfiguration(Game game, string currentGameSummaryPath)
    {
        Debug.LogFormat("game.listLevels:{0}", game.listLevels.Count); // tests data is saved properly
        game = SaveData.WriteAndReadJson<Game>(game, currentGameSummaryPath); // save settings
        Debug.LogFormat("game.listLevels:{0}", game.listLevels.Count); // tests data is saved properly
    }

    void Start()
    {
        print("GameManager.Start()");

        if (currentLevel == -1) // populate configurations on first run
        {
            CentralMemory.StartExternalProcess("/K taskkill /IM cmd.exe /F");
            PopulateRecordings();
        }

        currentLevel += 1;

        if (currentLevel == game.listLevels.Count)
        {
            SaveData.QuitGame();
            return;
        }

        Debug.LogFormat("currentLevel:{0}, name:{1}", currentLevel, game.listLevels[currentLevel].levelStartTime);

        Debug.LogFormat("listRecordings.recordings[currentRecording].options.name:{0}", game.listLevels[currentLevel].descriptor);
        Debug.LogFormat("listRecordings.recordings[currentRecording].options.isFreeViewing:{0}", game.listLevels[currentLevel].isFreeViewing);

        if (!game.listLevels[currentLevel].isFreeViewing) // first options is free viewing - does handle mixed blocks!
            SceneManager.LoadScene("TheAttentionAtlas");
        else
            SceneManager.LoadScene("FreeViewingScene");
    }


    public static void EscapeGame(bool isSaveData)
    {
        if (Input.GetButton("Cancel"))
        {
            print("GameManager.EscapeGame()");

            if (isSaveData)
            {
                GameOptions.isQuitGame = true;
                print("Saving data!");

                GameRunner.SaveTrialData();

                if (GameOptions.saveDataThread)
                    SaveData.SaveGameThread(CentralMemory.trialDataNp.Clone(), GameRunner.header.DeepClone(), CentralMemory.filePath.DeepClone(), CentralMemory.trialDataList.DeepClone(), GameRunner.listCriticalTrialData.DeepClone(), CentralMemory.frameDataList.DeepClone(), CentralMemory.frameDataFloat.DeepClone(), game.DeepClone(), currentGameSummaryPath.DeepClone());
                else
                    SaveData.SaveGame();

                if (GameOptions.isVisualise)
                    SaveData.VisualiseData();
            }
            else
                SaveData.QuitGame();
        }
    }

}