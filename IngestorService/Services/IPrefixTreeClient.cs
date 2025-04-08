namespace IngestorService.Services
{
    public interface IPrefixTreeClient
    {
        /// <summary>
        /// Inserts a word into the Prefix Tree.
        /// </summary>
        Task<bool> InsertWordAsync(string word);

        /// <summary>
        /// Removes a word from the Prefix Tree.
        /// </summary>
        Task<bool> RemoveWordAsync(string word);

        /// <summary>
        /// Searches for a word in the Prefix Tree.
        /// </summary>
        //Task<IEnumerable<string>> SearchWordsAsync(string word);

        ///// <summary>
        ///// Retrieves autocomplete suggestions based on a prefix.
        ///// </summary>
        //Task<IEnumerable<string>> GetWordsWithPrefixAsync(string prefix);
    }
}
