using DataStructures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphericalCoordinates : MonoBehaviour
{
    public class Coordinate
    {
        public int ID;

        public float latitude;
        public float longitude;
        public float radius;

        public Vector3 position;

        public Coordinate(float latitude, float longitude, float radius = 1, int ID = 0)
        {
            this.latitude = latitude;
            this.longitude = longitude;

            this.position = ConvertSphericalToCartesian(latitude, longitude, radius);
            this.ID = ID;
        }

        public Coordinate(Vector3 position, int ID = 0)
        {
            this.position = position;
            (latitude, longitude) = ConvertCartesianToSpherical(position);
            this.ID = ID;
        }

        public (float, float) ConvertCartesianToSpherical(Vector3 cartesian)
        {
            var radius = Mathf.Sqrt(cartesian.x * cartesian.y + cartesian.z * cartesian.y + cartesian.z * cartesian.z);
            var latitude = Mathf.Rad2Deg * Mathf.Asin(cartesian.z / radius);
            var longitude = Mathf.Rad2Deg * Mathf.Atan2(cartesian.y, cartesian.x);
            return (latitude, longitude);
        }

        public Vector3 ConvertSphericalToCartesian(float latitude, float longitude, float radius)
        {
            var lat = Mathf.Deg2Rad * latitude;
            var lon = Mathf.Deg2Rad * longitude;
            var x = radius * Mathf.Cos(lat) * Mathf.Cos(lon);
            var y = radius * Mathf.Cos(lat) * Mathf.Sin(lon);
            var z = radius * Mathf.Sin(lat);
            return new Vector3(x, y, z);
        }
    }

    public static List<Surfaces.Vertex> GetSurfaceVerticesSpherical(Options options, bool isCreateGameObject = true)
    {
        (float[], float[]) GetFullField()
        {
            (float[], float[]) GetLongitudeLatitude(float sphericalSpacing, float spacingModifier)
            {

                var x = Mathf.Cos(Mathf.Deg2Rad * 45); //  Get multiplier for 45 deg angle

                sphericalSpacing = sphericalSpacing * spacingModifier;
                float[] lat = new float[] { 0, sphericalSpacing, 0, -sphericalSpacing, sphericalSpacing * x, sphericalSpacing * x, -sphericalSpacing * x, -sphericalSpacing * x };
                float[] lon = new float[] { -sphericalSpacing, 0, sphericalSpacing, 0, sphericalSpacing * x, -sphericalSpacing * x, -sphericalSpacing * x, sphericalSpacing * x };

                return (lat, lon);
            }

            (float[] lat1, float[] lon1) = GetLongitudeLatitude(GameOptions.sphericalSpacing, 1); //Center position and first 15deg circle
            (float[] lat2, float[] lon2) = GetLongitudeLatitude(GameOptions.sphericalSpacing, 2); //Center position and first 30deg circle
            (float[] lat3, float[] lon3) = GetLongitudeLatitude(GameOptions.sphericalSpacing, 3); //Center position and first 45deg circle
            (float[] lat4, float[] lon4) = GetLongitudeLatitude(GameOptions.sphericalSpacing, 4); //Center position and first 60deg circle

            float[] latitude = new float[0]; // x
            float[] longitude = new float[0]; // y

            switch (options.keepAngleArray)
            {
                case GameOptions.sphericalSpacing * 1:
                    latitude = lat1;
                    longitude = lon1;
                    break;
                case GameOptions.sphericalSpacing * 2:
                    latitude = lat1.Concatenate(lat2);
                    longitude = lon1.Concatenate(lon2);
                    break;
                case GameOptions.sphericalSpacing * 3:
                    latitude = lat1.Concatenate(lat2).Concatenate(lat3);
                    longitude = lon1.Concatenate(lon2).Concatenate(lon3);
                    break;
                case GameOptions.sphericalSpacing * 4:
                    latitude = lat1.Concatenate(lat2).Concatenate(lat3).Concatenate(lat4);
                    longitude = lon1.Concatenate(lon2).Concatenate(lon3).Concatenate(lon4);
                    break;
                default:
                    Debug.LogFormat("keepAngleArray {0} does not match pre-defined spacing multiples: {1}, {2}, {3}, {4}", options.keepAngleArray, GameOptions.sphericalSpacing * 1, GameOptions.sphericalSpacing * 2, GameOptions.sphericalSpacing * 3, GameOptions.sphericalSpacing * 4);
                    Debug.LogError("keepAngleArray does not match pre-defined spacing multiples...");
                    break;
            };
            return (latitude, longitude);
        }

        //  Build item location array
        float[] anglesLatitude = new float[] { }; // x
        float[] anglesLongitude = new float[] { }; // y

        float radius1 = options.radius;
        float radius2 = options.radius;

        Debug.LogFormat("radius1:{0}", radius1);
        Debug.LogFormat("radius2:{0}", radius2);

        switch (options.coordinates)
        {
            case Options.Coordinates.pair1:
                anglesLatitude = new float[]{ GameOptions.sphericalSpacing, -GameOptions.sphericalSpacing};
                anglesLongitude = new float[]{ 0, 0 };
                break;
            case Options.Coordinates.pair2:
                anglesLatitude = new float[] { GameOptions.sphericalSpacing * 2, -GameOptions.sphericalSpacing * 2 };
                anglesLongitude = new float[] { 0, 0 };
                break;
            case Options.Coordinates.pair3:
                anglesLatitude = new float[] { GameOptions.sphericalSpacing * 3, -GameOptions.sphericalSpacing * 3 };
                anglesLongitude = new float[] { 0, 0 };
                break;
            case Options.Coordinates.pair4:
                anglesLatitude = new float[] { GameOptions.sphericalSpacing * 4, -GameOptions.sphericalSpacing * 4 };
                anglesLongitude = new float[] { 0, 0 };
                break;
            case Options.Coordinates.vertical:
                anglesLatitude = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                anglesLongitude = new float[] { GameOptions.sphericalSpacing, -GameOptions.sphericalSpacing, GameOptions.sphericalSpacing * 2, -GameOptions.sphericalSpacing * 2, GameOptions.sphericalSpacing * 3, -GameOptions.sphericalSpacing * 3, GameOptions.sphericalSpacing * 4, -GameOptions.sphericalSpacing * 4 };
                break;
            case Options.Coordinates.horizontal:
                anglesLatitude = new float[] { GameOptions.sphericalSpacing, -GameOptions.sphericalSpacing, GameOptions.sphericalSpacing * 2, -GameOptions.sphericalSpacing * 2, GameOptions.sphericalSpacing * 3, -GameOptions.sphericalSpacing * 3, GameOptions.sphericalSpacing * 4, -GameOptions.sphericalSpacing * 4 };
                anglesLongitude = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                break;
            case Options.Coordinates.depthConfig1:
                radius1 = options.radiusDepth;
                (anglesLatitude, anglesLongitude) = GetFullField();
                break;
            case Options.Coordinates.depthConfig2:
                radius2 = options.radiusDepth;
                (anglesLatitude, anglesLongitude) = GetFullField();
                break;
            case Options.Coordinates.fullField:
                (anglesLatitude, anglesLongitude) = GetFullField();
                break;
            //case Options.Coordinates.horizontal360:
            //    int nPoints = 360/
            //    GenericFunctions.LinSpace(0, 360, )

                //break;
        }

        //  Convert to coordinates and then a position array
        List<Surfaces.Vertex> vertices = new List<Surfaces.Vertex>();

        for (int i = 0; i < anglesLatitude.Length; i++)
        {
            Coordinate coord;

            if (options.coordinates == Options.Coordinates.depthConfig1 | options.coordinates == Options.Coordinates.depthConfig2)
            {
                if (new int[] { 0, 1, 2, 3, 12, 13, 14, 15, 16, 17, 18, 19, 28, 29, 30, 31 }.Contains(i)) // apply depth pattern
                {
                    coord = new Coordinate(anglesLatitude[i], anglesLongitude[i], radius2, i);
                }
                else
                {
                    coord = new Coordinate(anglesLatitude[i], anglesLongitude[i], radius1, i);
                }
            }
            else
            {
                coord = new Coordinate(anglesLatitude[i], anglesLongitude[i], radius1, i);
            }

            vertices.Add(new Surfaces.Vertex(optionsNumber: options.optionsNumber,
                                             position: coord.position,
                                             index: i,
                                             isCreateGameObject: isCreateGameObject,
                                             coordinates: options.coordinates,
                                             radius: options.radius,
                                             keepAngleArray: options.keepAngleArray));
        }
        return vertices;
    }
}

public static class Extension
{
    public static T[] Concatenate<T>(this T[] first, T[] second)
    {
        if (first == null)
        {
            return second;
        }
        if (second == null)
        {
            return first;
        }

        return first.Concat(second).ToArray();
    }
}
