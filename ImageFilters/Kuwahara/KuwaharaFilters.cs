using ImageFilters.Common;

namespace ImageFilters.Kuwahara
{
    internal class KuwaharaFilters
    {
        public static byte[,,] BaseKuwahara(ByteImage rgbMap, byte kernelSize)
        {
            throw new NotImplementedException();

            ByteImage valueMap = CommonFilters.GreyScale(rgbMap);
        }
    }
}
