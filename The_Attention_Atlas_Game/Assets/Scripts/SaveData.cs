using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;
using System.IO;

using DataStructures;

using System.Collections.Generic;
using NumSharp;

using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Newtonsoft.Json;


public static class ExtensionMethods
{
    public static T DeepClone<T>(this T obj) // allows threaded saving while variables are being updated
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }

    public static T CloneJson<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        //if (ReferenceEquals(self, null)) return default;

        // initialize inner objects individually
        // for example in default constructor some list property initialized with some values,
        // but in 'source' these items are cleaned -
        // without ObjectCreationHandling.Replace default constructor values will be added to result
        var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
    }

}


public class SaveData : MonoBehaviour
{

    // new save methods for better performance - warning error checking on binaries through load has been removed in interest of maximum performance
    public static void SaveObserverThread(Observer observer, string filePath)
    {
        //new Thread(() => // Create a new Thread
        //{
            WriteJson(observer, filePath);
        //}).Start(); // Start the Thread
    }

    public static void SaveFilePathThread(FilePath filePath)
    {
        //new Thread(() => // Create a new Thread
        //{
            WriteJson(filePath, filePath.filePathJson);
        //}).Start(); // Start the Thread
    }

    public static void SaveLevelThread(GameManager.Level level, string filePath)
    {
        //new Thread(() => // Create a new Thread
        //{
            WriteJson(level, filePath);
            BinaryMethods.Write(level, filePath);

        //}).Start(); // Start the Thread
    }

    public static void SaveObserverFilePathLevelThread(Observer observer, FilePath filePath, GameManager.Level level)
    {
        //new Thread(() => // Create a new Thread
        //{
            WriteJson(observer, filePath.observerJson);
            WriteJson(filePath, filePath.filePathJson);
            WriteJson(level, filePath.levelJson);
            BinaryMethods.Write(level, filePath.levelDat);
            WriteTextFile(fname: filePath.machineNameTxt, data: Environment.MachineName);

        //}).Start(); // Start the Thread
    }

    public static void SaveTrialDataThread(NDArray trialDataNp, string[] header, FilePath filePath, List<GameRunner.TrialData> trialDataList, List<GameRunner.CriticalTrialData> listCriticalTrialData)
    {
        //new Thread(() => // Create a new Thread
        //{
            SaveNpz(header: header, data: trialDataNp.ToMuliDimArray<float>(), filePath.trialDataNpz);
            GameRunner.ListTrialData trialDataCollection = new GameRunner.ListTrialData(trialDataList);
            WriteJson(trialDataCollection, filePath.trialDataJson);
            GameRunner.ListCriticalTrialData listCriticalTrialData2 = new GameRunner.ListCriticalTrialData(listCriticalTrialData);
            WriteJson(listCriticalTrialData2, filePath.listCriticalTrialDataJson);
        //}).Start(); // Start the Thread
    }

    public static void SaveFrameData(List<FrameRecorder.FrameData> frameDataList, FilePath filePath, List<List<float>> frameDataFloat)
    {
        //new Thread(() => // Create a new Thread
        //{
            BinaryMethods.Write(frameDataList, CentralMemory.filePath.frameDataDat);
            SaveNpz(header: FrameRecorder.header, data: ListListFloatToArray(frameDataFloat), filePath.frameDataNpz);
        //}).Start(); // Start the Thread
    }

    public static void SaveVerticesThread(List<Surfaces.Vertex> vertices, FilePath filePath)
    {
        new Thread(() => // Create a new Thread
        {
            Debug.LogFormat("vertices.Count:{0}", vertices.Count);
            Surfaces.ListSerializableVertex listSerializableVertex = new Surfaces.ListSerializableVertex(vertices);
            Debug.LogFormat("listSerializableVertex.listSerializableVertexCount:{0}", listSerializableVertex.listSerializableVertex.Count);

            List<List<float>> data = new List<List<float>>();

            string[] header = {
                    "serializableVertex.optionsNumber",
                    "serializableVertex.index",

                    "serializableVertex.position.x",
                    "serializableVertex.position.y",
                    "serializableVertex.position.z",

                    "serializableVertex.angle.x",
                    "serializableVertex.angle.y",
                    "serializableVertex.angle.z",

                    "serializableVertex.distance",

                    "(float)serializableVertex.coordinates",
                    "serializableVertex.radius",
                    "serializableVertex.keepAngleArray"};

            foreach (Surfaces.SerializableVertex serializableVertex in listSerializableVertex.listSerializableVertex)
            {
                data.Add(new List<float>()
                {
                    serializableVertex.optionsNumber,
                    serializableVertex.index,

                    serializableVertex.position.x,
                    serializableVertex.position.y,
                    serializableVertex.position.z,

                    serializableVertex.angle.x,
                    serializableVertex.angle.y,
                    serializableVertex.angle.z,

                    serializableVertex.distance,

                    (float)serializableVertex.coordinates,
                    serializableVertex.radius,
                    serializableVertex.keepAngleArray,
                });
            }

            // save python
            SaveNpz(header: header, data: ListListFloatToArray(data), filePath: filePath.verticesNpz);
            // save json
            WriteJson(listSerializableVertex, filePath.verticesJson);
            // save binary
            List<Surfaces.SerializableVertex> verticesSerializable = new List<Surfaces.SerializableVertex>(Surfaces.ListSerializableVertex.MakeSerializable(vertices));
            BinaryMethods.Write(verticesSerializable, filePath.verticesDat);
        }).Start(); // Start the Thread
    }

    public static void SaveGameThread(NDArray trialDataNp, string[] header, FilePath filePath, List<GameRunner.TrialData> trialDataList, List<GameRunner.CriticalTrialData> listCriticalTrialData,
                                      List<FrameRecorder.FrameData> frameDataList, List<List<float>> frameDataFloat,
                                      GameManager.Game game, string currentGameSummaryPath){
        //new Thread(() => // Create a new Thread
        //{
            WriteJson(game, currentGameSummaryPath);
            SaveTrialDataThread(trialDataNp, header, filePath, trialDataList, listCriticalTrialData);
            SaveFrameData(frameDataList, filePath, frameDataFloat);
            WriteJson(game, currentGameSummaryPath);
        //}).Start(); // Start the Thread
    }

    public static void SaveGameConfigurationThread(GameManager.Game game, string currentGameSummaryPath)
    {
        //new Thread(() => // Create a new Thread
        //{
            WriteJson(game, currentGameSummaryPath);
        //}).Start(); // Start the Thread
    }

    // old save methods for poorer performance
    public static Observer SaveObserver(Observer observer, FilePath filePath)
    {
        observer = WriteAndReadJson<Observer>(observer, filePath.observerJson);
        observer = BinaryMethods.WriteAndReadBinary<Observer>(observer, filePath.observerDat);
        return observer;
    }

    public static FilePath SaveFilePath(FilePath filePath)
    {
        filePath = WriteAndReadJson<FilePath>(filePath, filePath.filePathJson);
        filePath = BinaryMethods.WriteAndReadBinary<FilePath>(filePath, filePath.filePathDat);
        return filePath;
    }

    public static GameManager.Level SaveLevel(GameManager.Level level, FilePath filePath)
    {
        level = WriteAndReadJson<GameManager.Level>(level, filePath.levelJson);
        level = BinaryMethods.WriteAndReadBinary<GameManager.Level>(level, filePath.levelDat);
        return level;
    }

    public static (Observer, FilePath, GameManager.Level) SaveObserverFilePathLevel(Observer observer, FilePath filePath, GameManager.Level level)
    {
        observer = SaveObserver(observer, filePath);
        filePath = SaveFilePath(filePath);
        level = SaveLevel(level, filePath);
        WriteTextFile(fname: filePath.machineNameTxt, data: Environment.MachineName);
        return (observer, filePath, level);
    }

    static void SaveTrialData()
    {
        //CentralMemory.trialDataList = BinaryMethods.WriteAndReadBinary<List<GameRunner.TrialData>>(CentralMemory.trialDataList, CentralMemory.filePath.trialDataDat);
        //print(string.Format("CentralMemory.trialDataList.Count: {0}", CentralMemory.trialDataList.Count));

        SaveNpz(header: GameRunner.header, data: CentralMemory.trialDataNp.ToMuliDimArray<float>(), CentralMemory.filePath.trialDataNpz);

        GameRunner.ListTrialData trialDataCollection = new GameRunner.ListTrialData(CentralMemory.trialDataList);
        trialDataCollection = WriteAndReadJson<GameRunner.ListTrialData>(trialDataCollection, CentralMemory.filePath.trialDataJson);

        GameRunner.ListCriticalTrialData listCriticalTrialData = new GameRunner.ListCriticalTrialData(GameRunner.listCriticalTrialData);
        listCriticalTrialData = WriteAndReadJson<GameRunner.ListCriticalTrialData>(listCriticalTrialData, CentralMemory.filePath.listCriticalTrialDataJson);
    }

    static void SaveFrameData()
    {
        CentralMemory.frameDataList = BinaryMethods.WriteAndReadBinary<List<FrameRecorder.FrameData>>(CentralMemory.frameDataList, CentralMemory.filePath.frameDataDat);
        print(string.Format("raycastFrameData.Count: {0}", CentralMemory.frameDataList.Count));

        //CentralMemory.frameDataArray = NetCode.GetBufferData(NetCode.buffer[(int)NetCode.BID.frameData]);
        CentralMemory.frameDataArray = ListListFloatToArray(CentralMemory.frameDataFloat);
        SaveNpz(header: FrameRecorder.header, data: CentralMemory.frameDataArray, CentralMemory.filePath.frameDataNpz);
    }

    public static void SaveVertices(List<Surfaces.Vertex> vertices, FilePath filePath)
    {

        Debug.LogFormat("vertices.Count:{0}", vertices.Count);
        Surfaces.ListSerializableVertex listSerializableVertex = new Surfaces.ListSerializableVertex(vertices);
        Debug.LogFormat("listSerializableVertex.listSerializableVertexCount:{0}", listSerializableVertex.listSerializableVertex.Count);

        List<List<float>> data = new List<List<float>>();

        string[] header = {
                    "serializableVertex.optionsNumber",
                    "serializableVertex.index",

                    "serializableVertex.position.x",
                    "serializableVertex.position.y",
                    "serializableVertex.position.z",

                    "serializableVertex.angle.x",
                    "serializableVertex.angle.y",
                    "serializableVertex.angle.z",

                    "serializableVertex.distance",

                    "(float)serializableVertex.coordinates",
                    "serializableVertex.radius",
                    "serializableVertex.keepAngleArray"};

        foreach (Surfaces.SerializableVertex serializableVertex in listSerializableVertex.listSerializableVertex)
        {
            data.Add(new List<float>()
                {
                    serializableVertex.optionsNumber,
                    serializableVertex.index,

                    serializableVertex.position.x,
                    serializableVertex.position.y,
                    serializableVertex.position.z,

                    serializableVertex.angle.x,
                    serializableVertex.angle.y,
                    serializableVertex.angle.z,

                    serializableVertex.distance,

                    (float)serializableVertex.coordinates,
                    serializableVertex.radius,
                    serializableVertex.keepAngleArray,
                });
        }

        // save python
        SaveNpz(header: header, data: ListListFloatToArray(data), filePath: filePath.verticesNpz);

        // save json
        listSerializableVertex = WriteAndReadJson<Surfaces.ListSerializableVertex>(listSerializableVertex, filePath.verticesJson);
        Debug.LogFormat("listSerializableVertex.listSerializableVertexCount:{0}", listSerializableVertex.listSerializableVertex.Count);

        // save binary
        List<Surfaces.SerializableVertex> verticesSerializable = new List<Surfaces.SerializableVertex>(Surfaces.ListSerializableVertex.MakeSerializable(vertices));
        verticesSerializable = BinaryMethods.WriteAndReadBinary<List<Surfaces.SerializableVertex>>(verticesSerializable, filePath.verticesDat);
        Debug.LogFormat("verticesSerializable.Count:{0}", verticesSerializable.Count);
    }

    public static void SaveGame(bool isSaveTrialData= true)
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch(); stopWatch.Start();

        Debug.LogFormat(string.Format("SaveGame: {0}", CentralMemory.observer.levelStartTime));
        if (isSaveTrialData) SaveTrialData();
        SaveFrameData();
        GameManager.SaveGameConfiguration();

        Debug.LogFormat("SaveGame, stopWatch.ElapsedMilliseconds:{0}", stopWatch.ElapsedMilliseconds);
    }
 
    public static void VisualiseData()
    {
        SceneManager.LoadScene("VisualiserScene");
    }

    public static float[,] ListListFloatToArray(List<List<float>> data)
    {
        
        int nRows = data.Count;
        int nCols = data[0].Count;

        float[,] dataArray = new float[nRows, nCols];

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].Count; j++)
            {
                dataArray[i, j] = data[i][j];
            }
        }

        return dataArray;
    }

    public static float[,] RandomFloatArray(int nRows, int nCols)
    {
        System.Random rnd = new System.Random();
        float[,] data = new float[nRows, nCols];

        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
                data[i, j] = (float)rnd.NextDouble();
        }

        return data;
    }

    static void SaveNpz(string[] header, Array data, string filePath)
    {
        Dictionary<string, Array> data_dictionary = new Dictionary<string, Array>();
        data_dictionary.Add("header", header);
        data_dictionary.Add("data", data);
        np.Save_Npz(data_dictionary, filePath);
    }

    public void SaveNpz(string[] header, float[,] data, string filePath)
    {
        Dictionary<string, Array> data_dictionary = new Dictionary<string, Array>();
        data_dictionary.Add("header", header);
        data_dictionary.Add("data", data);
        np.Save_Npz(data_dictionary, filePath);
    }

    public static T WriteAndReadJson<T>(object instance, string filePath)
    {
        //WriteJson(Object: Activator.CreateInstance<T>(), filePath: filePath);
        WriteJson(Object: instance, filePath: filePath);
        T data = ReadJson<T>(filePath); 
        return data;
    }

    static void WriteJson(object Object, string filePath)
    {
        string json = JsonUtility.ToJson(Object);

        using (StreamWriter sw = File.CreateText(filePath))
            sw.WriteLine(json);

        Debug.Log(filePath);
        Debug.Log(json);
    }

    static public T ReadJson<T>(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        Debug.Log(lines[0]);

        T data = JsonUtility.FromJson<T>(lines[0]);
        GetPropertyNamesValuesInstance<T>(data);

        return data;
    }

    static public void GetPropertyNamesValuesInstance<T>(T obj) // uses instance to check if binary is correctly loading
    {
        FieldInfo[] properties = obj.GetType().GetFields();

        foreach (FieldInfo property in properties)
            Debug.LogFormat("\t{0}={1}", property.Name, property.GetValue(obj));
    }

    public static void WriteTextFile(string fname, string data)
    {
        using (StreamWriter sw = File.CreateText(fname))
        {
            sw.WriteLine(data);
        }
    }

    public static void QuitGame()
    {
        print("QuitGame");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
    }

}

public static class BinaryMethods
{
    public static T WriteAndReadBinary<T>(object Object, string fileName)
    {
        Write(Object, fileName);
        T data = Read<T>(fileName);
        return data;
    }

    static public void Write(object obj, string fName)
    {
        try
        {
            FileStream file = new FileStream(fName, FileMode.Append);

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                formatter.Serialize(file, obj);
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                Debug.Log("There was an issue serializing the data " + e.Message);
            }
            finally
            {
                file.Close();
            }
        }
        catch (Exception e) { Debug.Log(e); }
        finally { }
    }

    static public T Read<T>(string fName)
    {
        try
        {
            FileStream file = new FileStream(fName, FileMode.Open);

            try
            {
                BinaryFormatter formatter = new BinaryFormatter(); formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                T data = (T)formatter.Deserialize(file);
                return data;
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                Debug.Log("There was an issue serializing the data " + e.Message);
                return default(T);
            }
            finally
            {
                file.Close();
            }
        }
        catch (Exception e) { Debug.Log(e); return default(T); }
        finally { }
    }
}
