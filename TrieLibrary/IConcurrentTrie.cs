using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrieLibrary
{
    using System.Collections.Generic;

    public interface IConcurrentTrie
    {
        /// <summary>
        /// Inserts a word into the trie.
        /// Returns true if the word was newly added, false if it already existed.
        /// </summary>
        bool Insert(string word);

        /// <summary>
        /// Removes a word from the trie.
        /// Returns true if the word was removed, false if it was not found.
        /// </summary>
        bool Remove(string word);

        /// <summary>
        /// Checks if a word exists in the trie.
        /// </summary>
        bool Search(string word);

        /// <summary>
        /// Retrieves words that start with a given prefix.
        /// </summary>
        List<string> GetWordsWithPrefix(string prefix);
    }
}
