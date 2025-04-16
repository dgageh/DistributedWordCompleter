using Microsoft.Azure.Cosmos;

namespace IngestorService.Services
{
    public class WordIngestorService
    {
        private readonly IPrefixTreeClient _prefixTreeClient;
        private readonly IWordsDbService _wordsDbService;
        private readonly ILogger<WordIngestorService> _logger;

        public WordIngestorService(
            IPrefixTreeClient prefixTreeClient,
            IWordsDbService wordsDbService,
            ILogger<WordIngestorService> logger)
        {
            _prefixTreeClient = prefixTreeClient ?? throw new ArgumentNullException(nameof(prefixTreeClient));
            _wordsDbService = wordsDbService ?? throw new ArgumentNullException(nameof(wordsDbService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProvisionDatabaseAsync()
        {
            _logger.LogInformation("Provisioning database...");
            await _wordsDbService.ProvisionDatabaseAsync();
        }

        public async Task InitializeAsync(IEnumerable<string>? words = null)
        {
            if (words == null)
            {
                string filePath = Path.Combine("Data", "words.txt");

                if (!File.Exists(filePath))
                {
                    _logger.LogError("File {FilePath} not found in container.", filePath);
                    throw new FileNotFoundException($"File {filePath} not found in container.");
                }

                _logger.LogInformation("Reading words from file: {FilePath}", filePath);
                words = await File.ReadAllLinesAsync(filePath);
            }

            _logger.LogInformation("Wiping database...");
            await _wordsDbService.WipeDatabaseAsync();
            await BatchAddWordsAsync(words);
        }

        public async Task StartupAsync()
        {
            var words = await _wordsDbService.GetAllWordsAsync();
            _logger.LogInformation("Loading {WordCount} words into PrefixTree...", words.Count());
            await BatchAddWordsAsync(words);
        }

        public async Task BatchAddWordsAsync(IEnumerable<string> words)
        {
            var uniqueWords = words.Select(w => w.ToLower().Trim()).Where(w => !string.IsNullOrEmpty(w)).Distinct();
            _logger.LogInformation("Adding {UniqueWordCount} unique words...", uniqueWords.Count());

            await Parallel.ForEachAsync(uniqueWords, async (word, _) =>
            {
                try
                {
                    await _prefixTreeClient.InsertWordAsync(word);
                    await _wordsDbService.WriteWordAsync(word);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding word '{Word}'", word);
                }
            });
        }

        public async Task BatchDeleteWordsAsync(IEnumerable<string> words)
        {
            var uniqueWords = words.Select(w => w.ToLower().Trim()).Where(w => !string.IsNullOrEmpty(w)).Distinct();
            _logger.LogInformation("Deleting {UniqueWordCount} words...", uniqueWords.Count());

            await Parallel.ForEachAsync(uniqueWords, async (word, _) =>
            {
                try
                {
                    await _prefixTreeClient.RemoveWordAsync(word);
                    await _wordsDbService.DeleteWordAsync(word);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting word '{Word}'", word);
                }
            });
        }

        public void StartSynchronization()
        {
            _wordsDbService.OnWordAdded += async (sender, word) =>
            {
                _logger.LogInformation("Syncing new word '{Word}'...", word);
                await _prefixTreeClient.InsertWordAsync(word);
            };

            _wordsDbService.OnWordDeleted += async (sender, word) =>
            {
                _logger.LogInformation("Syncing deleted word '{Word}'...", word);
                await _prefixTreeClient.RemoveWordAsync(word);
            };
        }
    }
}
