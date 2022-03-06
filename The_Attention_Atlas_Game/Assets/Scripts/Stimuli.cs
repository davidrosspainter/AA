using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using DataStructures;

public class Stimuli
{
    public static string letterCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@$%^*()_-+=:;”,./?<>";
    public static Sprite[] charSprites;
    public static Sprite[] foodSprites;
    public static Sprite[] specOrbSprites;

    public static SpriteAtlas cardAtlas; // Using Atlas because it improves runtime performance - really? I thought sprite sheets were best for performance - certainly for convenience?
    public static SpriteAtlas shapeAtlas; // Using Atlas because it improves runtime performance
    public static SpriteAtlas symbolAtlas;
    
    public static System.Random rndSeed = new System.Random(); // Random seed for generic randomization

    // targets
    public static string targetCard = "queen";
    public static string targetSuit = "diamonds";
    public static string targetShape = "square";
    public static string targetSymbol = "1";

    public enum Balloons // to do
    {
        targetWithString,
        targetWithoutString
    }


    public static void LoadSprites()
    {
        // ----- load graphics resoources
        charSprites = Resources.LoadAll<Sprite>("OpenDyslexicMono-01");
        foodSprites = Resources.LoadAll<Sprite>("SpriteSheets/Ghostpixxells_pixelfood/foodSheet_upscaled");
        specOrbSprites = Resources.LoadAll<Sprite>("SpriteSheets/SpecOrbs/AllInPNGWithOL_white");
        cardAtlas = Resources.Load<SpriteAtlas>("PlayingCards/Cards");
        shapeAtlas = Resources.Load<SpriteAtlas>("GeoPrimatives/GeoPrimatives");
        symbolAtlas = Resources.Load<SpriteAtlas>("Symbols/Symbols");
    }

    // text
    static public GameObject SpawnText(string text, Vector3 position, float scale, Color color)
    {
        GameObject spriteGameObject = new GameObject(text);

        int count = 0;

        foreach (var t in text)
        {
            // find sprite
            int index = letterCode.IndexOf(t); //print(index);

            // create gameobject
            GameObject go = new GameObject(letterCode[index].ToString());

            // create spriterender
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = charSprites[index];

            // position
            // length = 4, shift = 1.5
            // length = 3, shift = 1.0; 
            // length = 2, shift = 0.5
            // length = 1, shift = 0.0

            float inc = scale * 3.3f; // spacing betwen letters
            float mod = -inc * (text.Length - 1) / 2; // recentre

            go.transform.position = new Vector3(count * inc + mod, 0, 0);
            go.transform.parent = spriteGameObject.transform;
            go.transform.localScale *= scale;

            // color
            sr.color = color;

            // add box collider
            BoxCollider boxCollider = go.AddComponent<BoxCollider>();
            boxCollider.size *= GameOptions.boxColliderScaleOpenDyslexic;

            go.layer = LayerMask.NameToLayer("sprite");
            count++;
        }

        spriteGameObject.transform.position = position;
        return spriteGameObject;
    }

    public static (string, string) GetRandomCardStrings(string excludingCard = null, string excludingSuit = null)
    {
        string card = "";
        string suit = "";

        //List<string> cards = new List<string> { "ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "jack", "queen", "king" };
        List<string> cards = new List<string> { "jack", "queen", "king" };
        List<string> suits = new List<string> { "spades", "clubs", "diamonds", "hearts" };

        while (true)
        {
            cards.Shuffle();
            suits.Shuffle();

            if(!(cards[0] == "queen" & suits[0] == "diamonds"))
            {
                card = cards[0];
                suit = suits[0];
                break;
            }
        }
        

        //card = cards[rndSeed.Next(cards.Count)];
        //suit = suits[rndSeed.Next(suits.Count)];

        //if (excludingCard == card && excludingSuit == suit)     //  Is this the excluded target
        //{
        //    if (rndSeed.NextDouble() <= 0.5)        // remove either the excluded card or suit from their respective lists and try again
        //    {
        //        cards.Remove(card);
        //    }
        //    else
        //    {
        //        suits.Remove(suit);
        //    }
        //    card = cards[rndSeed.Next(cards.Count)];
        //    suit = suits[rndSeed.Next(suits.Count)];
        //}

        return (card, suit);
    }

    static public GameObject SpawnCard(string text, string card, string suit, Vector3 position, float scale, Color color)
    {
        GameObject spriteGameObject = new GameObject(text);
        GameObject go = new GameObject(text); // is parenting required here (probably?) - hangover from indices stimulus?
        
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cardAtlas.GetSprite(card + "_of_" + suit);
        sr.color = color;

        go.transform.parent = spriteGameObject.transform;
        go.transform.localScale *= scale;
        BoxCollider boxCollider = go.AddComponent<BoxCollider>();
        go.layer = LayerMask.NameToLayer("sprite");

        spriteGameObject.transform.position = position;
        return spriteGameObject;
    }

    public static string GetRandomAtlasStrings(SpriteAtlas sa, string excludingShape = null)
    {
        string rtrn;
        List<string> spriteNames = sa.GetSpriteList();
        rtrn = spriteNames[rndSeed.Next(spriteNames.Count)];

        if (excludingShape == rtrn)     //  Is this the excluded target
        {
            spriteNames.Remove(rtrn);
            rtrn = spriteNames[rndSeed.Next(spriteNames.Count)];
        }
        return rtrn;
    }

    /// <summary>
    /// Returns a game object with a sprite from a given sprite atlas
    /// </summary>
    /// <param name="text">Game Object Name</param>
    /// <param name="spriteName">Sprite Name</param>
    /// <param name="sa">The Sprite Atlas this is a part of</param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    static public GameObject SpawnSpriteFromAtlas(string text, string spriteName, SpriteAtlas sa, Vector3 position, float scale, Color color)
    {
        GameObject spriteGameObject = new GameObject(text);
        GameObject go = new GameObject(text); // is parenting required here (probably?) - hangover from indices stimulus?

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sa.GetSprite(spriteName);
        sr.color = color;

        go.transform.parent = spriteGameObject.transform;
        go.transform.localScale *= scale;
        BoxCollider boxCollider = go.AddComponent<BoxCollider>();
        go.layer = LayerMask.NameToLayer("sprite");

        spriteGameObject.transform.position = position;
        return spriteGameObject;
    }

    static public GameObject SpawnBalloon(string text, Vector3 position, float scale, Color color)
    {
        Object source = Resources.Load("prefabs/BaloonSimplified3");
        GameObject spriteGameObject = new GameObject(text);
        GameObject gameObject = (GameObject)Object.Instantiate(source);

        List<Renderer> meshRenderers = new List<Renderer>();
        foreach (var item in new List<string>() { "root 1", "root 2" })
        {
            meshRenderers.Add(gameObject.transform.Find(item).gameObject.GetComponent<Renderer>());
            meshRenderers[meshRenderers.Count - 1].material.color = color;
        }

        gameObject.name = text;
        gameObject.layer = LayerMask.NameToLayer("sprite");
        gameObject.transform.parent = spriteGameObject.transform;
        gameObject.transform.localScale *= scale;
        spriteGameObject.transform.position = position;

        void HideString()
        {
            meshRenderers[1].enabled = false;
        }

        if (gameObject.name == "targetBalloon") HideString();
        return spriteGameObject;
    }

    static public GameObject SpawnSprite(Sprite[] sprites, int spriteIndex, Vector3 position, float scale, Color? color = null, string text = "")
    {
        GameObject spriteGameObject = new GameObject(text);
        GameObject go = new GameObject(text);

        // create spriterender
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[spriteIndex];
        go.transform.parent = spriteGameObject.transform;
        go.transform.localScale *= scale;

        if (color != null & color != Color.black)
            sr.color = (Color)color;

        BoxCollider boxCollider = go.AddComponent<BoxCollider>();
        boxCollider.size *= GameOptions.boxColliderScaleOpenDyslexic;

        go.layer = LayerMask.NameToLayer("sprite");
        spriteGameObject.transform.position = position;
        return spriteGameObject;
    }

    static Color SetElementColor(GameRunner.TrialData.Element.Category category, Options.SearchMode searchMode)
    {
        Color colorToUse = Color.white;
     
        switch (category)
        {
            case GameRunner.TrialData.Element.Category.target:
                switch (searchMode)
                {
                    case Options.SearchMode.serial:
                        colorToUse = GameOptions.Colors.targetSerial;
                        break;
                    case Options.SearchMode.uniqueFeature:
                        colorToUse = GameOptions.Colors.targetUniqueFeature;
                        break;
                    case Options.SearchMode.conjunction:
                        colorToUse = GameOptions.Colors.targetConjunction;
                        break;
                    case Options.SearchMode.serial2:
                        colorToUse = GameOptions.Colors.targetSerial2;
                        break;
                    case Options.SearchMode.rainbow:
                        //colorToUse = GameOptions.Colors.colorsRainbow[Random.Range(0, GameOptions.Colors.colorsRainbow.Count)];
                        colorToUse = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        break;
                    default:
                        break;
                }
                break;
            case GameRunner.TrialData.Element.Category.distractor:
                switch (searchMode)
                {
                    case Options.SearchMode.serial:
                        colorToUse = GameOptions.Colors.distractorSerial;
                        break;
                    case Options.SearchMode.uniqueFeature:
                        colorToUse = GameOptions.Colors.distractorUniqueFeature;
                        break;
                    case Options.SearchMode.conjunction:
                        if (UnityEngine.Random.Range(0f, 1f) > .5f)
                            colorToUse = GameOptions.Colors.targetConjunction;
                        else
                            colorToUse = GameOptions.Colors.distractorConjunction;
                        break;
                    case Options.SearchMode.rainbow:
                        //colorToUse = GameOptions.Colors.colorsRainbow[Random.Range(0, GameOptions.Colors.colorsRainbow.Count)];
                        colorToUse = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        break;
                    case Options.SearchMode.serial2:
                        colorToUse = GameOptions.Colors.distractorSerial2;
                        break;
                    default:
                        break;
                }
                break;
        }
        return colorToUse;
    }

    public static class GenerateElement
    {
        public static GameRunner.TrialData.Element Target(GameObject vertexGameObject, int vertexIndex, Vector3 position, Options.Stimulus stimulus, Options.SearchMode searchMode)
        {
            Color colorToUse;
            if (searchMode == Options.SearchMode.none)
                colorToUse = Color.black; // dummy color for no colouring, full implemented?
            else
                colorToUse = SetElementColor(GameRunner.TrialData.Element.Category.target, searchMode: searchMode);

            GameRunner.TrialData.Element target = new GameRunner.TrialData.Element();
            int spriteIndex;

            switch (stimulus)
            {
                case Options.Stimulus.undefined:
                    Debug.LogError("undefined");
                    break;
                case Options.Stimulus.letters:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: "T");
                    break;
                case Options.Stimulus.numbers:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: "6");
                    break;
                case Options.Stimulus.indices:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: vertexIndex.ToString());
                    break;
                case Options.Stimulus.dots:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: ".");
                    break;
                case Options.Stimulus.balloons:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: "targetBalloon");
                    break;
                case Options.Stimulus.cards:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, card: new GameRunner.TrialData.Element.Card(Stimuli.targetCard, Stimuli.targetSuit), text: "targetCard"); // text required for some reason, kind of silly
                    break;
                case Options.Stimulus.shapes:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: Stimuli.targetShape);
                    break;
                case Options.Stimulus.symbols:
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, text: Stimuli.targetSymbol);
                    break;
                case Options.Stimulus.food:
                    spriteIndex = Random.Range(0, foodSprites.Length);
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, spriteIndex: spriteIndex, text: vertexIndex.ToString());
                    break;
                case Options.Stimulus.specOrbs:
                    spriteIndex = Random.Range(0, specOrbSprites.Length);
                    target = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.target, rotation: 0, color: colorToUse, spriteIndex: spriteIndex, text: vertexIndex.ToString());
                    break;
                default:
                    Debug.LogError("default");
                    break;
            }
            return target;
        }

        public static GameRunner.TrialData.Element Distractor(GameObject vertexGameObject, int vertexIndex, Vector3 position, Options.Stimulus stimulus, Options.SearchMode searchMode)
        {
            Color colorToUse;
            if (searchMode == Options.SearchMode.none)
                colorToUse = Color.black; // dummy color for no colouring
            else
                colorToUse = SetElementColor(GameRunner.TrialData.Element.Category.distractor, searchMode: searchMode);

            GameRunner.TrialData.Element distractor = new GameRunner.TrialData.Element();
            List<float> rotations;
            int spriteIndex;

            switch (stimulus)
            {
                case Options.Stimulus.undefined:
                    Debug.LogError("undefined");
                    break;
                case Options.Stimulus.letters:
                    rotations = new List<float>() { 0, 90, 180, 270 }; rotations.Shuffle();
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: rotations[0], color: colorToUse, text: "L");
                    break;
                case Options.Stimulus.numbers:
                    rotations = new List<float>() { 0, 90, 270 }; rotations.Shuffle();
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: rotations[0], color: colorToUse, text: "9");
                    break;
                case Options.Stimulus.indices:
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, text: vertexIndex.ToString());
                    break;
                case Options.Stimulus.dots:
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, text: ".");
                    break;
                case Options.Stimulus.balloons:
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, text: "distractorBalloon");
                    break;
                case Options.Stimulus.cards:
                    (string card, string suit) = Stimuli.GetRandomCardStrings(Stimuli.targetCard, Stimuli.targetSuit);
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, card: new GameRunner.TrialData.Element.Card(card, suit), text: "distractorCard");
                    break;
                case Options.Stimulus.shapes:
                    string shape = Stimuli.GetRandomAtlasStrings(Stimuli.shapeAtlas, Stimuli.targetShape);
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, text: shape);
                    break;
                case Options.Stimulus.symbols:
                    string symbol = Stimuli.GetRandomAtlasStrings(Stimuli.symbolAtlas, Stimuli.targetSymbol);
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, text: symbol);
                    break;
                case Options.Stimulus.food:
                    while (true)
                    {
                        spriteIndex = Random.Range(0, foodSprites.Length);
                        if (GameRunner.currentTrialData.targetSpriteIndex != spriteIndex)
                            break;
                    }
                    
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, spriteIndex: spriteIndex, text: vertexIndex.ToString());
                    break;
                case Options.Stimulus.specOrbs:
                    while (true)
                    {
                        spriteIndex = Random.Range(0, specOrbSprites.Length);
                        if (GameRunner.currentTrialData.targetSpriteIndex != spriteIndex)
                            break;
                    }
                    distractor = new GameRunner.TrialData.Element(vertexGameObject: vertexGameObject, vertexIndex: vertexIndex, position: position, category: GameRunner.TrialData.Element.Category.distractor, rotation: 0, color: colorToUse, spriteIndex: spriteIndex, text: vertexIndex.ToString());
                    break;
                default:
                    Debug.LogError("default");
                    break;
            }
            return distractor;
        }
    }


    public static GameObject SpawnStimulus(GameRunner.TrialData.Element element, Options.Stimulus stimulus)
    {
        GameObject spriteGameObject;
        switch (stimulus)
        {
            case Options.Stimulus.undefined:
                spriteGameObject = new GameObject();
                break;
            case Options.Stimulus.letters:
                spriteGameObject = Stimuli.SpawnText(element.text, element.position, GameOptions.spriteScaleOpenDyslexic, element.color);
                break;
            case Options.Stimulus.numbers:
                spriteGameObject = Stimuli.SpawnText(element.text, element.position, GameOptions.spriteScaleOpenDyslexic, element.color);
                break;
            case Options.Stimulus.indices:
                spriteGameObject = Stimuli.SpawnText(element.text, element.position, GameOptions.spriteScaleOpenDyslexic, element.color);
                break;
            case Options.Stimulus.dots:
                spriteGameObject = Stimuli.SpawnText(element.text, element.position, GameOptions.spriteScaleOpenDyslexic, element.color);
                break;
            case Options.Stimulus.balloons:
                spriteGameObject = Stimuli.SpawnBalloon(element.text, element.position, 1, element.color);
                break;
            case Options.Stimulus.cards:
                spriteGameObject = Stimuli.SpawnCard(element.text, element.card.card, element.card.suit, element.position, GameOptions.spriteScaleOpenDyslexic, element.color);
                break;
            case Options.Stimulus.shapes:
                spriteGameObject = Stimuli.SpawnSpriteFromAtlas(element.text, element.text, Stimuli.shapeAtlas, element.position, GameOptions.spriteScaleOpenDyslexic, element.color);
                break;
            case Options.Stimulus.symbols:
                spriteGameObject = Stimuli.SpawnSpriteFromAtlas(element.text, element.text, Stimuli.symbolAtlas, element.position, GameOptions.spriteScaleOpenDyslexic * 5, element.color);
                break;
            case Options.Stimulus.food:
                spriteGameObject = Stimuli.SpawnSprite(Stimuli.foodSprites, element.spriteIndex, element.position, .1f, element.color, element.text);
                break;
            case Options.Stimulus.specOrbs:
                spriteGameObject = Stimuli.SpawnSprite(Stimuli.specOrbSprites, element.spriteIndex, element.position, .1f, element.color, element.text);
                break;
            default:
                spriteGameObject = new GameObject();
                break;
        }
        return spriteGameObject;
    }



}
