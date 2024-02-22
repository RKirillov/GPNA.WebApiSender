using GPNA.Converters.TagValues;
using System.Collections.Generic;

namespace GPNA.gRPCClient.ServiceTagDouble_to_delete
{
    public interface IClientServiceDouble_
    {
        public TagValueDouble? GetTag();

        public IEnumerable<TagValueDouble?> GetTags(int chunkSize);
    }
}
