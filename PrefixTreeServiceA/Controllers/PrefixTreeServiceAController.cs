using Microsoft.AspNetCore.Mvc;
using TrieLibrary;

namespace PrefixTreeServiceA.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing words in the Prefix Tree.
    /// </summary>
    [ApiController]
    [Route("api/words")]
    public class PrefixTreeController : ControllerBase
    {
        private readonly IConcurrentTrie _trie;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixTreeController"/> class.
        /// </summary>
        /// <param name="trie">The concurrent trie used for word operations.</param>
        public PrefixTreeController(IConcurrentTrie trie)
        {
            _trie = trie;
        }

        /// <summary>
        /// Inserts a word into the Prefix Tree.
        /// </summary>
        /// <param name="word">The word to insert.</param>
        /// <returns>
        /// A response indicating whether the word was successfully added or if it already exists.
        /// </returns>
        [HttpPost("insert")]
        public IActionResult Insert([FromQuery] string word)
        {
            if (string.IsNullOrEmpty(word)) return BadRequest("Word cannot be empty.");
            return _trie.Insert(word) ? Ok("Word added.") : Conflict("Word already exists.");
        }

        /// <summary>
        /// Removes a word from the Prefix Tree.
        /// </summary>
        /// <param name="word">The word to remove.</param>
        /// <returns>
        /// A response indicating whether the word was successfully removed or if it was not found.
        /// </returns>
        [HttpDelete("remove")]
        public IActionResult Remove([FromQuery] string word)
        {
            if (string.IsNullOrEmpty(word)) return BadRequest("Word cannot be empty.");
            return _trie.Remove(word) ? Ok("Word removed.") : NotFound("Word not found.");
        }

        /// <summary>
        /// Searches for a word in the Prefix Tree.
        /// </summary>
        /// <param name="word">The word to search for.</param>
        /// <returns>
        /// A response indicating whether the word exists in the Prefix Tree.
        /// </returns>
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string word)
        {
            if (string.IsNullOrEmpty(word)) return BadRequest("Word cannot be empty.");
            return Ok(_trie.Search(word));
        }

        /// <summary>
        /// Retrieves all words in the Prefix Tree that start with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to search for.</param>
        /// <returns>
        /// A list of words that start with the specified prefix.
        /// </returns>
        [HttpGet("autocomplete")]
        public IActionResult GetWordsWithPrefix([FromQuery] string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return BadRequest("Prefix cannot be empty.");
            return Ok(_trie.GetWordsWithPrefix(prefix));
        }
    }
}
