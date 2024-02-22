using GPNA.Converters.TagValues;
using System.Collections.Generic;

namespace GPNA.gRPCClient.ServiceTagBool_to_delete
{
    public interface IClientServiceBool_
    {
        public TagValueBool? GetTag();

        public IEnumerable<TagValueBool?> GetTags(int chunkSize);
    }
}
