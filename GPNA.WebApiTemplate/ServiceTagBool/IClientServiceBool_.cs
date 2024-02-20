using GPNA.Converters.TagValues;
using System.Collections.Generic;

namespace GPNA.WebApiSender.ServiceTagBool
{
    public interface IClientServiceBool_
    {
        public TagValueBool? GetTag();

        public IEnumerable<TagValueBool?> GetTags(int chunkSize);
    }
}
