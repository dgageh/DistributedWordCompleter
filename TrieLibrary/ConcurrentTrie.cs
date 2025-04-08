namespace TrieLibrary
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class TrieNode
    {
        public ConcurrentDictionary<char, TrieNode> Children { get; } = new();
        public bool IsWordEnd { get; set; }
    }

    public class ConcurrentTrie : IConcurrentTrie
    {
        private readonly TrieNode _root = new();

        // Insert a word into the trie
        public bool Insert(string word)
        {
            var currentNode = _root;
            bool isNewWord = false;

            foreach (var ch in word)
            {
                currentNode = currentNode.Children.GetOrAdd(ch, _ =>
                {
                    isNewWord = true; // A new node was created, meaning it's a new word insertion
                    return new TrieNode();
                });
            }

            if (currentNode.IsWordEnd)
            {
                return false; // Word already exists
            }

            currentNode.IsWordEnd = true;
            return true; // Successfully added a new word
        }

        // Check if a word exists in the trie
        public bool Search(string word)
        {
            var currentNode = _root;
            foreach (var ch in word)
            {
                if (!currentNode.Children.TryGetValue(ch, out currentNode))
                    return false;
            }
            return currentNode.IsWordEnd;
        }

        // Retrieve words that start with a given prefix
        public List<string> GetWordsWithPrefix(string prefix)
        {
            var currentNode = _root;
            foreach (var ch in prefix)
            {
                if (!currentNode.Children.TryGetValue(ch, out currentNode))
                    return new List<string>(); // No words with this prefix
            }

            var results = new List<string>();
            CollectWords(currentNode, prefix, results);
            return results;
        }

        private void CollectWords(TrieNode node, string prefix, List<string> results)
        {
            if (node.IsWordEnd)
                results.Add(prefix);

            foreach (var kvp in node.Children)
            {
                CollectWords(kvp.Value, prefix + kvp.Key, results);
            }
        }

        public bool Remove(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;

            var currentNode = _root;
            var stack = new Stack<(TrieNode node, char letter)>(); // Track nodes for cleanup

            foreach (char ch in word)
            {
                if (!currentNode.Children.TryGetValue(ch, out var nextNode))
                {
                    return false; // Word doesn't exist
                }

                stack.Push((currentNode, ch));
                currentNode = nextNode;
            }

            if (!currentNode.IsWordEnd)
            {
                return false; // Word doesn't exist
            }

            currentNode.IsWordEnd = false; // Mark word as deleted

            // Cleanup orphaned nodes
            while (stack.Count > 0)
            {
                var (parentNode, letter) = stack.Pop();

                if (currentNode.Children.Count == 0 && !currentNode.IsWordEnd)
                {
                    parentNode.Children.TryRemove(letter, out _);
                }

                currentNode = parentNode;
            }

            return true; // Successfully removed the word
        }
    }
}
