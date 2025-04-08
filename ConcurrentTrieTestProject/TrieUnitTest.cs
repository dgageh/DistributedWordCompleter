using TrieLibrary;
using Xunit;

namespace ConcurrentTrieTestProject
{
    public class ConcurrentTrieTests
    {
        [Fact]
        public void Insert_NewWord_ReturnsTrue()
        {
            var trie = new ConcurrentTrie();
            bool result = trie.Insert("hello");

            Assert.True(result); // Should return true because "hello" was newly added
        }

        [Fact]
        public void Insert_ExistingWord_ReturnsFalse()
        {
            var trie = new ConcurrentTrie();
            trie.Insert("world"); // Insert once

            bool result = trie.Insert("world"); // Insert again

            Assert.False(result); // Should return false because "world" already exists
        }

        [Fact]
        public void Search_ExistingWord_ReturnsTrue()
        {
            var trie = new ConcurrentTrie();
            trie.Insert("test");

            Assert.True(trie.Search("test")); // Should return true
        }

        [Fact]
        public void Search_NonExistingWord_ReturnsFalse()
        {
            var trie = new ConcurrentTrie();

            Assert.False(trie.Search("missing")); // Should return false
        }

        [Theory]
        [InlineData("app")]
        [InlineData("apple")]
        [InlineData("application")]
        public void Insert_MultipleWords_AllExist(string word)
        {
            var trie = new ConcurrentTrie();
            trie.Insert(word);

            Assert.True(trie.Search(word)); // Should return true for each inserted word
        }

        [Fact]
        public void GetWordsWithPrefix_ReturnsCorrectWords()
        {
            var trie = new ConcurrentTrie();
            trie.Insert("cat");
            trie.Insert("car");
            trie.Insert("cart");
            trie.Insert("dog");

            var words = trie.GetWordsWithPrefix("ca");

            Assert.Contains("cat", words);
            Assert.Contains("car", words);
            Assert.Contains("cart", words);
            Assert.DoesNotContain("dog", words); // "dog" should not be included
        }


        [Fact]
        public void Remove_ExistingWord_ReturnsTrue()
        {
            var trie = new ConcurrentTrie();
            trie.Insert("hello");

            bool result = trie.Remove("hello");

            Assert.True(result); // Should return true because "hello" was removed
            Assert.False(trie.Search("hello")); // "hello" should no longer exist
        }

        [Fact]
        public void Remove_NonExistingWord_ReturnsFalse()
        {
            var trie = new ConcurrentTrie();

            bool result = trie.Remove("world"); // Trying to remove a word that was never added

            Assert.False(result); // Should return false because "world" does not exist
        }

        [Fact]
        public void Remove_WordThatSharesPrefix_DoesNotAffectOtherWords()
        {
            var trie = new ConcurrentTrie();
            trie.Insert("car");
            trie.Insert("cart");

            bool result = trie.Remove("car");

            Assert.True(result); // "car" should be removed
            Assert.True(trie.Search("cart")); // "cart" should still exist
        }

        [Fact]
        public void Remove_LastWordInTrie_CleansUpStructure()
        {
            var trie = new ConcurrentTrie();
            trie.Insert("solo");

            bool result = trie.Remove("solo");

            Assert.True(result); // Word should be removed
            Assert.False(trie.Search("solo")); // Trie should no longer contain "solo"
        }
    }
}
