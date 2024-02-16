using GPNA.Converters.TagValues;

namespace GPNA.WebApiSender.Services
{
    public interface IClientServiceBool
    {
        public TagValueBool? GetTag();

        public IEnumerable<TagValueBool?> GetTags(int chunkSize);
    }
}
