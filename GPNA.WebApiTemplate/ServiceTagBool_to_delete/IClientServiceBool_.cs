using GPNA.Converters.TagValues;
using System.Collections.Generic;

namespace GPNA.gRPCClient.ServiceTagBool
{
    public interface IClientServiceBool_
    {
        public TagValueBool? GetTag();

        public IEnumerable<TagValueBool?> GetTags(int chunkSize);
    }
}
