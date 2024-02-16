using GPNA.Converters.TagValues;

namespace GPNA.WebApiSender.Services
{
    public interface IClientServiceDouble
    {
        public TagValueDouble? GetTag();

        public IEnumerable<TagValueDouble?> GetTags(int chunkSize);
    }
}
