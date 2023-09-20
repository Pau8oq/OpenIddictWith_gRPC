namespace Translating.gRPC.Services
{
    public class ProtoService
    {
        private readonly string _baseDirectory;

        public ProtoService(IWebHostEnvironment webHost)
        {
            _baseDirectory = webHost.ContentRootPath;
        }

        public Dictionary<string, IEnumerable<string>> GetAll()
        {
            var res = Directory.GetDirectories($"{_baseDirectory}/Protos")
                .Select(s => new
                {
                    version = s,
                    protos = Directory.GetFiles(s).Select(Path.GetFileName)
                })
                .ToDictionary(o => Path.GetRelativePath("protos", o.version), o => o.protos);

            return res;
        }

        public string Get(int version, string protoName)
        {
            var filePath = $"{_baseDirectory}/Protos/v{version}/{protoName}";
            var exist = File.Exists(filePath);
            return exist ? filePath : null;
        }
        public async Task<string> ViewAsync(int version, string protoName)
        {
            var filePath = $"{_baseDirectory}/Protos/v{version}/{protoName}";
            var exist = File.Exists(filePath);
            return exist ? await File.ReadAllTextAsync(filePath) :
           string.Empty;
        }
    }
}
