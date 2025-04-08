﻿namespace IngestorService.Services
{
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Container = Microsoft.Azure.Cosmos.Container;

    public class WordsDbService : IWordsDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _wordsContainer;
        private const string DatabaseId = "WordsDB";
        private const string ContainerId = "WordsContainer";

        public event EventHandler<string>? OnWordAdded;
        public event EventHandler<string>? OnWordDeleted;

        public WordsDbService(string endpoint, string key)
        {
            _cosmosClient = new CosmosClient(endpoint, key);
            var database = _cosmosClient.GetDatabase(DatabaseId);
            _wordsContainer = database.GetContainer(ContainerId);
        }

        /// <summary>
        /// Wipes the WordsDB and recreates the container.
        /// </summary>
        public async Task WipeDatabaseAsync()
        {
            Console.WriteLine("Wiping WordsDB...");
            var database = _cosmosClient.GetDatabase(DatabaseId);
            var container = database.GetContainer(ContainerId);
            await container.DeleteContainerAsync();
            await database.CreateContainerIfNotExistsAsync(ContainerId, "/id");
        }

        /// <summary>
        /// Retrieves all words from the database.
        /// </summary>
        public async Task<IEnumerable<string>> GetAllWordsAsync()
        {
            var query = new QueryDefinition("SELECT c.id FROM c");
            var iterator = _wordsContainer.GetItemQueryIterator<WordDocument>(query);

            var words = new List<string>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                words.AddRange(response.Select(doc => doc.Id));
            }

            return words;
        }

        /// <summary>
        /// Writes a word to the database.
        /// </summary>
        public async Task WriteWordAsync(string word)
        {
            var wordDoc = new WordDocument { Id = word };
            await _wordsContainer.UpsertItemAsync(wordDoc);
            OnWordAdded?.Invoke(this, word);
        }

        /// <summary>
        /// Deletes a word from the database.
        /// </summary>
        public async Task DeleteWordAsync(string word)
        {
            await _wordsContainer.DeleteItemAsync<WordDocument>(word, new PartitionKey(word));
            OnWordDeleted?.Invoke(this, word);
        }
    }

    public class WordDocument
    {
        public string Id { get; set; } = default!;
    }
}
