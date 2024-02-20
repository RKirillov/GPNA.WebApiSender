using GPNA.Converters.TagValues;
using System.Collections.Generic;

namespace GPNA.WebApiSender.ServiceTagDouble
{
    public interface IClientServiceDouble_
    {
        public TagValueDouble? GetTag();

        public IEnumerable<TagValueDouble?> GetTags(int chunkSize);
    }
}
