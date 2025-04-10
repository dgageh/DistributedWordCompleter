using Microsoft.Azure.Cosmos;

namespace IngestorService.Services
{
    public class WordIngestorService
    {
        private readonly IPrefixTreeClient _prefixTreeClient;
        private readonly IWordsDbService _wordsDbService;

        public WordIngestorService(IPrefixTreeClient prefixTreeClient, IWordsDbService wordsDbService)
        {
            _prefixTreeClient = prefixTreeClient ?? throw new ArgumentNullException(nameof(prefixTreeClient));
            _wordsDbService = wordsDbService ?? throw new ArgumentNullException(nameof(wordsDbService));
        }

        public async Task ProvisionDatabaseAsync()
        {
            await _wordsDbService.ProvisionDatabaseAsync();
        }

        public async Task InitializeAsync(IEnumerable<string>? words = null)
        {
            // If words are not provided, read from file
            if (words == null)
            {
                string filePath = Path.Combine("Data", "words.txt");

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File {filePath} not found in container.");

                words = await File.ReadAllLinesAsync(filePath);
            }

            await _wordsDbService.WipeDatabaseAsync();
            await BatchAddWordsAsync(words);
        }

        public async Task StartupAsync()
        {
            var words = await _wordsDbService.GetAllWordsAsync();
            Console.WriteLine($"Loading {words.Count()} words into PrefixTree...");
            await BatchAddWordsAsync(words);
        }

        public async Task BatchAddWordsAsync(IEnumerable<string> words)
        {
            var uniqueWords = words.Select(w => w.ToLower().Trim()).Where(w => !string.IsNullOrEmpty(w)).Distinct();
            Console.WriteLine($"Adding {uniqueWords.Count()} unique words...");

            await Parallel.ForEachAsync(uniqueWords, async (word, _) =>
            {
                try
                {
                    await _prefixTreeClient.InsertWordAsync(word);
                    await _wordsDbService.WriteWordAsync(word);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding word '{word}': {ex.Message}");
                }
            });
        }

        public async Task BatchDeleteWordsAsync(IEnumerable<string> words)
        {
            var uniqueWords = words.Select(w => w.ToLower().Trim()).Where(w => !string.IsNullOrEmpty(w)).Distinct();
            Console.WriteLine($"Deleting {uniqueWords.Count()} words...");

            await Parallel.ForEachAsync(uniqueWords, async (word, _) =>
            {
                try
                {
                    await _prefixTreeClient.RemoveWordAsync(word);
                    await _wordsDbService.DeleteWordAsync(word);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting word '{word}': {ex.Message}");
                }
            });
        }

        public void StartSynchronization()
        {
            _wordsDbService.OnWordAdded += async (sender, word) =>
            {
                Console.WriteLine($"Syncing new word '{word}'...");
                await _prefixTreeClient.InsertWordAsync(word);
            };

            _wordsDbService.OnWordDeleted += async (sender, word) =>
            {
                Console.WriteLine($"Syncing deleted word '{word}'...");
                await _prefixTreeClient.RemoveWordAsync(word);
            };
        }
    }
}
