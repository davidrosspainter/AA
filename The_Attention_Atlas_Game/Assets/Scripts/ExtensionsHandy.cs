using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class ExtensionsHandy
{
    //  This class houses the generic extensions

    /// <summary>
    /// Checks a game object for a component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns>true if the component exists in GameObject</returns>
    public static bool HasComponent<T>(this GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() != null;
    }

    /// <summary>
    /// Returns a list of the sprite names in the sprite atlas
    /// </summary>
    /// <param name="sa"></param>
    /// <returns></returns>
    public static List<string> GetSpriteList(this SpriteAtlas sa)
    {
        List<string> saNames = new List<string>();
        Sprite[] sprites = new Sprite[sa.spriteCount];
        sa.GetSprites(sprites);

        foreach (Sprite sprite in sprites) {
            saNames.Add(sprite.name.RemoveCloneSuffix());
        }

        return saNames;
    }

    /// <summary>
    /// Removes the "(Clone)" suffix that occurs when you reference a sprite name from a spriteatlas
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string RemoveCloneSuffix(this string str)
    {
        if (str.EndsWith("(Clone)"))
            return str.Substring(0, str.Length - "(Clone)".Length);
        else
            return str;
    }

}