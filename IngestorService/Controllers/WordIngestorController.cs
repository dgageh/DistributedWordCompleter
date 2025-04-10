namespace IngestorService.Controllers
{
    using IngestorService.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/words-ingestor")]
    public class WordIngestorController : Controller
    {
        private readonly WordIngestorService _wordIngestorService;

        public WordIngestorController(WordIngestorService wordIngestorService)
        {
            _wordIngestorService = wordIngestorService;
        }

        [HttpPost("provision")]
        public async Task<IActionResult> ProvisionDatabase()
        {
            await _wordIngestorService.ProvisionDatabaseAsync();
            return Ok("Database and container provisioned successfully.");
        }

        /// <summary>
        /// Initializes the WordsDB by wiping it and repopulating from words.txt
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            await _wordIngestorService.InitializeAsync();
            return Ok("WordsDB has been wiped and repopulated.");
        }

        /// <summary>
        /// Loads all words into the PrefixTree
        /// </summary>
        [HttpPost("startup")]
        public async Task<IActionResult> Startup()
        {
            await _wordIngestorService.StartupAsync();
            return Ok("All words have been loaded into the PrefixTree.");
        }

        /// <summary>
        /// Adds a batch of words to WordsDB and PrefixTree
        /// </summary>
        [HttpPost("batch-add")]
        public async Task<IActionResult> BatchAddWords([FromBody] List<string> words)
        {
            if (words == null || words.Count == 0)
                return BadRequest("Word list cannot be empty.");

            await _wordIngestorService.BatchAddWordsAsync(words);
            return Ok($"Added {words.Count} words.");
        }

        /// <summary>
        /// Removes a batch of words from WordsDB and PrefixTree
        /// </summary>
        [HttpDelete("batch-delete")]
        public async Task<IActionResult> BatchDeleteWords([FromBody] List<string> words)
        {
            if (words == null || words.Count == 0)
                return BadRequest("Word list cannot be empty.");

            await _wordIngestorService.BatchDeleteWordsAsync(words);
            return Ok($"Deleted {words.Count} words.");
        }

        /// <summary>
        /// Starts synchronization to listen for changes in WordsDB
        /// </summary>
        [HttpPost("sync")]
        public IActionResult StartSync()
        {
            _wordIngestorService.StartSynchronization();
            return Ok("Synchronization started for database changes.");
        }
    }
}
