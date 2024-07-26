using System.Windows.Media;
using System.IO;
using static wpfpixpile.Draw;
using System.Windows;

namespace wpfpixpile
{
    public class Object3D(Vector3D _Pos, Vector3D _Scale, int _RotY = 0, int _RotX = 0, bool _InFront = false, bool _InvertNormals = false)
    {
        public int RotX = _RotX;
        public int RotY = _RotY;
        public Vector3D Scale = _Scale;
        public Vector3D Pos = _Pos;
        public bool InFront = _InFront;
        public bool InvertNormals = _InvertNormals;

        public Vector3D[] ObjectVertices = [];
        public int[][] ObjectIndices = [];
        public Color[] ObjectColors = [];
        public Vector3D[] ObjectNormals = [];
        public Point[] ObjectTextureCoordinates = [];

        public Vector3D[] BakedObjectVertices = [];

        public Object3D ChangeToAxis()
        {
            ObjectVertices = [
                new Vector3D(0, 0, 0, 1), // 0 0 8
                new Vector3D(1, 0, 0, 1), // +x 1 9
                new Vector3D(0, 1, 0, 1), // +y 2 10
                new Vector3D(0, 0, 1, 1), // +z 3 11
                new Vector3D(-1, 0, 0, 1), // -x 4 12
                new Vector3D(0, -1, 0, 1), // -y 5 13
                new Vector3D(0, 0, -1, 1), // -z 6 14
            ];
            ObjectIndices = [
                [1, 0, 0], // axis +x [9, 8, 8]
                [2, 0, 0], // axis +y [10, 8, 8]
                [3, 0, 0], // axis +Z [11, 8, 8]
                [4, 0, 0], // axis -x [12, 8, 8]
                [5, 0, 0], // axis -y [13, 8, 8]
                [6, 0, 0], // axis -Z [14, 8, 8]
            ];
            ObjectColors = [
                Color.FromRgb(255, 0, 0),
                Color.FromRgb(0, 255, 0),
                Color.FromRgb(0, 0, 255),
                Color.FromRgb(128, 0, 0),
                Color.FromRgb(0, 128, 0),
                Color.FromRgb(0, 0, 128),
            ];
            BakedObjectVertices = new Vector3D[ObjectVertices.Length];
            return this;
        }

        public Object3D ChangeToCube(Color clr)
        {
            ObjectVertices = [
                new Vector3D(-1, 1, 1, 1),
                new Vector3D(-1, 1, -1, 1),
                new Vector3D(1, 1, -1, 1),
                new Vector3D(1, 1, 1, 1),
                new Vector3D(-1, -1, 1, 1),
                new Vector3D(-1, -1, -1, 1),
                new Vector3D(1, -1, -1, 1),
                new Vector3D(1, -1, 1, 1),
            ];
            ObjectIndices = [
                [0, 1, 2], // 0
                [0, 2, 3], // 1
                [4, 6, 5], // 2
                [4, 7, 6], // 3
                [0, 5, 1], // 4
                [0, 4, 5], // 5
                [1, 5, 2], // 6
                [6, 2, 5], // 7
                [3, 2, 6], // 8
                [3, 6, 7], // 9
                [3, 4, 0], // 10
                [4, 3, 7], // 11
                [4, 3, 7], // 12
            ];
            ObjectColors = [
                clr, clr, clr, clr, clr, clr,
                clr, clr, clr, clr, clr, clr,
                clr
            ];
            BakedObjectVertices = new Vector3D[ObjectVertices.Length];
            return this;
        }

        public Object3D ChangeToPiramid()
        {
            ObjectVertices = [
                new Vector3D(-0.5, 1, 0.5, 1),
                new Vector3D(-0.5, 1, -0.5, 1),
                new Vector3D(0.5, 1, -0.5, 1),
                new Vector3D(0.5, 1, 0.5, 1),
                new Vector3D(-1, -1, 1, 1),
                new Vector3D(-1, -1, -1, 1),
                new Vector3D(1, -1, -1, 1),
                new Vector3D(1, -1, 1, 1),
            ];
            ObjectIndices = [
                [0, 1, 2], // 0
                [0, 2, 3], // 1
                [4, 6, 5], // 2
                [4, 7, 6], // 3
                [0, 5, 1], // 4
                [0, 4, 5], // 5
                [1, 5, 2], // 6
                [6, 2, 5], // 7
                [3, 2, 6], // 8
                [3, 6, 7], // 9
                [3, 4, 0], // 10
                [4, 3, 7], // 11
                [4, 3, 7], // 11
            ];
            ObjectColors = [
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
                Color.FromRgb(255, 255, 128),
            ];
            BakedObjectVertices = new Vector3D[ObjectVertices.Length];
            return this;
        }

        public Object3D ChangeToLoadFromObj(string Path)
        {
            using (StreamReader reader = new StreamReader(Path))
            {
                string? line;
                int vCount = 0;
                int iCount = 0;
                int vnCount = 0;
                int vtCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line[0] == 'v' && line[1] == ' ') { vCount++; }; // vertice x y z
                    if (line[0] == 'f' && line[1] == ' ') { iCount++; }; // indice 0 3 6
                    if (line[0] == 'v' && line[1] == 'n' && line[2] == ' ') { vnCount++; }; // normals x y z
                    if (line[0] == 'v' && line[1] == 't' && line[2] == ' ') { vtCount++; }; // texture u v
                }

                // raw values
                ObjectVertices = new Vector3D[vCount];
                ObjectNormals = new Vector3D[vnCount];
                ObjectTextureCoordinates = new Point[vtCount];
                ObjectColors = new Color[iCount];

                // maps
                ObjectIndices = new int[iCount][];
            }

            using (StreamReader reader = new StreamReader(Path))
            {
                string? line;
                string[] lineSplit;
                int vCount = 0;
                int iCount = 0;
                int vnCount = 0;
                int vtCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line[0] == 'v' && line[1] == ' ')
                    {
                        lineSplit = line.Replace("v ", "").Replace(".", ",").Split(' ');
                        ObjectVertices[vCount] = new Vector3D(double.Parse(lineSplit[0]), double.Parse(lineSplit[1]), double.Parse(lineSplit[2]));
                        vCount++;
                    };
                    if (line[0] == 'f' && line[1] == ' ')
                    {
                        lineSplit = line.Replace("f ", "").Split(new char[] {'/', ' '});
                        ObjectIndices[iCount] = new int[] { int.Parse(lineSplit[0]) - 1, int.Parse(lineSplit[3]) - 1, int.Parse(lineSplit[6]) - 1 };
                        iCount++;
                    };
                    if (line[0] == 'v' && line[1] == 'n' && line[2] == ' ')
                    {
                        lineSplit = line.Replace("vn ", "").Replace(".", ",").Split(' ');
                        ObjectNormals[vnCount] = new Vector3D(double.Parse(lineSplit[0]), double.Parse(lineSplit[1]), double.Parse(lineSplit[2]));
                        vnCount++;
                    };
                    if (line[0] == 'v' && line[1] == 't' && line[2] == ' ')
                    {
                        lineSplit = line.Replace("vt ", "").Replace(".", ",").Split(' ');
                        ObjectTextureCoordinates[vtCount] = new Point(double.Parse(lineSplit[0]), double.Parse(lineSplit[1]));
                        vtCount++;
                    };
                }
            }
            
            for (int i = 0; i < ObjectIndices.Length; i++)
            {
                ObjectColors[i] = Color.FromRgb(
                    (byte)(Math.Abs(ObjectVertices[(ObjectIndices[i][0])].X) * 255 > 255 ? 255 : Math.Abs(ObjectVertices[(ObjectIndices[i][0])].X) * 255),
                    (byte)(Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Y) * 255 > 255 ? 255 : Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Y) * 255),
                    (byte)(Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Z) * 255 > 255 ? 255 : Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Z) * 255)
                    // (byte)(Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Len()) * 255 > 255 ? 255 : Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Len()) * 255),
                    // (byte)(Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Len()) * 255 > 255 ? 255 : Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Len()) * 255),
                    // (byte)(Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Len()) * 255 > 255 ? 255 : Math.Abs(ObjectVertices[(ObjectIndices[i][0])].Len()) * 255)
                    // 128, 128, 128
                );
            }
            BakedObjectVertices = new Vector3D[ObjectVertices.Length];
            return this;
        }

        public void PushToScene(ref Object3D[] SceneObjects)
        {
            Array.Resize(ref SceneObjects, SceneObjects.Length + 1);
            SceneObjects[SceneObjects.Length-1] = this;
        }
    }
}
