using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System;

public static class GenericFunctions {

    public static void CreateOutFile(string FNAME, bool delete)
    {
        if (File.Exists(FNAME) & delete)
        {
            File.Delete(FNAME);
            UnityEngine.Debug.Log(FNAME);
            using (StreamWriter sw = File.CreateText(FNAME)) { };
        }
        else if (!File.Exists(FNAME))
        {
            using (StreamWriter sw = File.CreateText(FNAME)) { };
        }
    }

    public static void WriteString(string fname, string array)
    {
        using (StreamWriter sw = new StreamWriter(fname, true))
        {
            sw.Write(array+"\n");
        }
    }

    public static void WriteString(string fname, List<List<float>> data)
    {
        using (StreamWriter sw = new StreamWriter(fname, true))
        {
            for (int i = 0; i < data.Count; i++)
            {
                sw.WriteLine(string.Join<float>(",", data[i]));
            }
        }
    }

    public static void WriteString(string fname, float[,] array, string header)
    {
        int nRows = array.GetUpperBound(0) - array.GetLowerBound(0) + 1;
        int nCols = array.GetUpperBound(1) - array.GetLowerBound(1) + 1;

        using (StreamWriter sw = File.CreateText(fname))
        {
            sw.Write(header);

            for (int i = 0; i < nRows; i++)
            {
                float[] vector = array.GetRow(i);
                sw.WriteLine(string.Join(",", vector)+"\n");
            }
        }
    }

    public static float[] LinSpace(float d1, float d2, int n)
    {
        int nl = n - 1;

        float[] y = new float[nl + 1];

        for (int i = 0; i <= nl; i++)
        {
            y[i] = d1 + i * (d2 - d1) / nl;
        }

        return y;
    }

    public static int[] RandPerm(int n, System.Random rnd)
    {
        //Debug.Log("RandPerm");
        var idx = Enumerable.Range(1, n).OrderBy(r => rnd.Next()).ToArray();
        return idx;
    }

    static System.Random rnd = new System.Random(DateTime.Now.Millisecond);

    static public List<int> RandPerm(int n)
    {
        return Enumerable.Range(0, n).OrderBy(r => rnd.Next()).ToList();
    }

    static public double CalculateSD(List<float> values)
    {
        double ret = double.NaN;
        if (values.Count() > 0)
        {
            double avg = values.Average(); //Compute the Average 
            double sum = values.Sum(d => System.Math.Pow(d - avg, 2)); //Perform the Sum of (value-avg)_2_2 
            ret = Math.Sqrt((sum) / (values.Count() - 1)); //Put it all together     
        }
        return ret; // props to Daniel Super
    }

    public static void InvertMeshNormals(Mesh mesh)
    {
        // ----- invert normals

        Vector3[] normals = mesh.normals;

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -1 * normals[i];
        }

        mesh.normals = normals;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] tris = mesh.GetTriangles(i);

            for (int j = 0; j < tris.Length; j += 3)
            {
                //swap order of tri vertices
                int temp = tris[j];
                tris[j] = tris[j + 1];
                tris[j + 1] = temp;
            }

            mesh.SetTriangles(tris, i);
        }
    }


    static public float BoolToFloat(bool Bool)
    {
        float Float;

        if (Bool)
            Float = 1;
        else
            Float = 0;

        return Float;
    }

    static public float BoolToFloat(bool? Bool)
    {
        float Float = float.NaN;

        switch (Bool)
        {
            case true: Float = 1; break;
            case false: Float = 0; break;
            case null: Float = 0; break;
        }

        return Float;
    }

}



public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static class ThreadSafeRandom
    {
        [System.ThreadStatic] private static System.Random Local;

        public static System.Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new System.Random(unchecked(System.Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId))); }
        }
    }



}

public static class UtilityLibrary
{
    public static bool IsSerializable(this object obj)
    {
        if (obj == null)
            return false;

        Type t = obj.GetType();
        return t.IsSerializable;
    }
}


public static class MatrixExtensions // https://stackoverflow.com/questions/16636019/how-to-get-1d-column-array-and-1d-row-array-from-2d-array-c-net
{
    // Usage example:
    //double[,] myMatrix = ... // Initialize with desired size and values.
    //double[] myRowVector = myMatrix.GetRow(2); // Gets the third row.
    //double[] myColVector = myMatrix.GetCol(1); // Gets the second column.
    //myMatrix.SetCol(2, myColVector); // Sets the third column to the second column.

    /// <summary>
    /// Returns the row with number 'row' of this matrix as a 1D-Array.
    /// </summary>
    public static T[] GetRow<T>(this T[,] matrix, int row)
    {
        var rowLength = matrix.GetLength(1);
        var rowVector = new T[rowLength];

        for (var i = 0; i < rowLength; i++)
            rowVector[i] = matrix[row, i];

        return rowVector;
    }

    /// <summary>
    /// Sets the row with number 'row' of this 2D-matrix to the parameter 'rowVector'.
    /// </summary>
    public static void SetRow<T>(this T[,] matrix, int row, T[] rowVector)
    {
        var rowLength = matrix.GetLength(1);

        for (var i = 0; i < rowLength; i++)
            matrix[row, i] = rowVector[i];
    }

    /// <summary>
    /// Returns the column with number 'col' of this matrix as a 1D-Array.
    /// </summary>
    public static T[] GetCol<T>(this T[,] matrix, int col)
    {
        var colLength = matrix.GetLength(0);
        var colVector = new T[colLength];

        for (var i = 0; i < colLength; i++)
            colVector[i] = matrix[i, col];

        return colVector;
    }

    /// <summary>
    /// Sets the column with number 'col' of this 2D-matrix to the parameter 'colVector'.
    /// </summary>
    public static void SetCol<T>(this T[,] matrix, int col, T[] colVector)
    {
        var colLength = matrix.GetLength(0);

        for (var i = 0; i < colLength; i++)
            matrix[i, col] = colVector[i];
    }
} // props to Timitry



public static class RandomExtensions
{

    // https://stackoverflow.com/questions/19512210/how-to-save-the-state-of-a-random-generator-in-c

    //void Main()
    //{
    //    var r = new Random();

    //    Enumerable.Range(1, 5).Select(idx => r.Next()).Dump("before save");
    //    var s = r.Save();
    //    Enumerable.Range(1, 5).Select(idx => r.Next()).Dump("after save");
    //    r = s.Restore();
    //    Enumerable.Range(1, 5).Select(idx => r.Next()).Dump("after restore");

    //    s.Dump();
    //}


    public static RandomState Save(this System.Random random)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var temp = new MemoryStream())
        {
            binaryFormatter.Serialize(temp, random);
            return new RandomState(temp.ToArray());
        }
    }

    public static System.Random Restore(this RandomState state)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var temp = new MemoryStream(state.State))
        {
            return (System.Random)binaryFormatter.Deserialize(temp);
        }
    }

}

public struct RandomState
{
    public readonly byte[] State;
    public RandomState(byte[] state)
    {
        State = state;
    }
}

public static class CastExtensions
{
    public static bool TryCast<T>(this object obj, out T result)
    {
        if (obj is T)
        {
            result = (T)obj;
            return true;
        }

        result = default(T);
        return false;
    }
}

public class PlayerPrefsExtensions
{
    public static void SetBool(string name, bool booleanValue)
    {
        PlayerPrefs.SetInt(name, booleanValue ? 1 : 0);
    }

    public static bool GetBool(string name)
    {
        return PlayerPrefs.GetInt(name) == 1 ? true : false;
    }

    public static bool GetBool(string name, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(name))
        {
            return GetBool(name);
        }

        return defaultValue;
    }
}


public static class PerformanceInfo
{
    // https://stackoverflow.com/questions/10027341/c-sharp-get-used-memory-in

    [DllImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

    [StructLayout(LayoutKind.Sequential)]
    public struct PerformanceInformation
    {
        public int Size;
        public IntPtr CommitTotal;
        public IntPtr CommitLimit;
        public IntPtr CommitPeak;
        public IntPtr PhysicalTotal;
        public IntPtr PhysicalAvailable;
        public IntPtr SystemCache;
        public IntPtr KernelTotal;
        public IntPtr KernelPaged;
        public IntPtr KernelNonPaged;
        public IntPtr PageSize;
        public int HandlesCount;
        public int ProcessCount;
        public int ThreadCount;
    }

    public static Int64 GetPhysicalAvailableMemoryInMiB()
    {
        PerformanceInformation pi = new PerformanceInformation();
        if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
        {
            return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
        }
        else
        {
            return -1;
        }
    }

    public static Int64 GetTotalMemoryInMiB()
    {
        PerformanceInformation pi = new PerformanceInformation();
        if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
        {
            return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
        }
        else
        {
            return -1;
        }

    }
}