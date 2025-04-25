using IdGen;

namespace API.Services
{
    public class IdGeneratorService
    {
        private readonly IdGenerator _idGenerator;

        public IdGeneratorService()
        {
            // Initialize IdGenerator with a GeneratorId (e.g., 0)
            var epoch = new DateTime(2025, 3, 21); // Custom epoch
            var timeSource = new DefaultTimeSource(epoch);
            var options = new IdGeneratorOptions(timeSource: timeSource);
            _idGenerator = new IdGenerator(0, options); // Machine ID: 0
        }

        public long GenerateId()
        {
            return _idGenerator.CreateId();
        }
    }
}