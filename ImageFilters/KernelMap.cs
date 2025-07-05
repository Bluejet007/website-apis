using System.Numerics;

namespace ImageFilters
{
    public class KernelMap
    {
        public int[,] Map { get; set; }
        public int MapHeight { get { return Map.GetLength(0); } }
        public int MapWidth { get { return Map.GetLength(1); } }
        public byte KernelSize { get; }
        public int ImageHeight { get { return Map.GetLength(0) - 2 * (KernelSize - 1); } }
        public int ImageWidth { get { return Map.GetLength(1) - 2 * (KernelSize - 1); } }

        public KernelMap(int height, int width, byte kernelSize, Func<int, int, int> kernelFunc)
        {
            this.KernelSize = kernelSize;
            this.Map = new int[height + 2 * (kernelSize - 1), width + 2 * (kernelSize - 1)];

            for (int i = this.Map.GetLowerBound(0); i < this.Map.GetUpperBound(0); i++)
            {
                Parallel.For(this.Map.GetLowerBound(1), this.Map.GetUpperBound(1), (j) =>
                    this.Map[i, j] = kernelFunc(i - kernelSize + 1, j - kernelSize + 1));
            }
        }

        public int Get(int i, int j)
        {
            return this.Map[i + KernelSize - 1, j + KernelSize - 1];
        }

        public void Set(int i, int j, int value)
        {
            this.Map[i + KernelSize - 1, j + KernelSize - 1] = value;
        }

        public static T BoundChecked<T>(T x, T max) where T : INumber<T>
        {
            if (x < T.Zero || x.CompareTo(max) >= 0)
            {
                throw new IndexOutOfRangeException();
            }

            return x;
        }

        public static T BoundReflected<T>(T x, T max) where T : IBinaryNumber<T>
        {
            if (x < T.Zero)
            {
                x -= T.CreateChecked(2) * x + T.One;
            }
            else if (x.CompareTo(max) >= 0)
            {
                x = T.CreateChecked(2) * max - x - T.One;
            }

            return x;
        }

        public static T BoundTruncated<T>(T x, T max) where T : IBinaryNumber<T>
        {
            x = T.Max(x, T.Zero);
            x = T.Min(x, max - T.One);

            return x;
        }
    }
}
