using Microsoft.AspNetCore.Mvc;
using TrieLibrary;

namespace PrefixTreeServiceB.Controllers
{
    [ApiController]
    [Route("api/words")]
    public class PrefixTreeController : Controller
    {
        private readonly IConcurrentTrie _trie;

        public PrefixTreeController(IConcurrentTrie trie)
        {
            _trie = trie;
        }

        [HttpPost("insert")]
        public IActionResult Insert([FromQuery] string word)
        {
            if (string.IsNullOrEmpty(word)) return BadRequest("Word cannot be empty.");
            return _trie.Insert(word) ? Ok("Word added.") : Conflict("Word already exists.");
        }

        [HttpDelete("remove")]
        public IActionResult Remove([FromQuery] string word)
        {
            if (string.IsNullOrEmpty(word)) return BadRequest("Word cannot be empty.");
            return _trie.Remove(word) ? Ok("Word removed.") : NotFound("Word not found.");
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string word)
        {
            if (string.IsNullOrEmpty(word)) return BadRequest("Word cannot be empty.");
            return Ok(_trie.Search(word));
        }

        [HttpGet("autocomplete")]
        public IActionResult GetWordsWithPrefix([FromQuery] string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return BadRequest("Prefix cannot be empty.");
            return Ok(_trie.GetWordsWithPrefix(prefix));
        }
    }
}
