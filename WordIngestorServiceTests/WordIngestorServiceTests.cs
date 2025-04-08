namespace WordIngestorServiceTests
{
    using Xunit;
    using Moq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IngestorService.Services;

    public class WordIngestorServiceTests
    {
        private readonly Mock<IPrefixTreeClient> _mockPrefixTreeClient;
        private readonly Mock<IWordsDbService> _mockWordsDbService;
        private readonly WordIngestorService _service;

        public WordIngestorServiceTests()
        {
            _mockPrefixTreeClient = new Mock<IPrefixTreeClient>();
            _mockWordsDbService = new Mock<IWordsDbService>();

            _service = new WordIngestorService(_mockPrefixTreeClient.Object, _mockWordsDbService.Object);
        }

        [Fact]
        public async Task BatchAddWordsAsync_Should_Add_Words_To_PrefixTree_And_WordsDB()
        {
            // Arrange
            var words = new List<string> { "apple", "banana", "cherry" };
            _mockPrefixTreeClient.Setup(client => client.InsertWordAsync(It.IsAny<string>()))
                                 .ReturnsAsync(true);
            _mockWordsDbService.Setup(db => db.WriteWordAsync(It.IsAny<string>()))
                               .Returns(Task.CompletedTask);

            // Act
            await _service.BatchAddWordsAsync(words);

            // Assert
            foreach (var word in words)
            {
                _mockPrefixTreeClient.Verify(client => client.InsertWordAsync(word), Times.Once);
                _mockWordsDbService.Verify(db => db.WriteWordAsync(word), Times.Once);
            }
        }

        [Fact]
        public async Task BatchDeleteWordsAsync_Should_Remove_Words_From_PrefixTree_And_WordsDB()
        {
            // Arrange
            var words = new List<string> { "apple", "banana", "cherry" };
            _mockPrefixTreeClient.Setup(client => client.RemoveWordAsync(It.IsAny<string>()))
                                 .ReturnsAsync(true);
            _mockWordsDbService.Setup(db => db.DeleteWordAsync(It.IsAny<string>()))
                               .Returns(Task.CompletedTask);

            // Act
            await _service.BatchDeleteWordsAsync(words);

            // Assert
            foreach (var word in words)
            {
                _mockPrefixTreeClient.Verify(client => client.RemoveWordAsync(word), Times.Once);
                _mockWordsDbService.Verify(db => db.DeleteWordAsync(word), Times.Once);
            }
        }

        [Fact]
        public async Task InitializeAsync_Should_Wipe_Db_And_Add_Words()
        {
            // Arrange
            var fileWords = new List<string> { "dog", "cat", "mouse" };
            _mockWordsDbService.Setup(db => db.WipeDatabaseAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.InitializeAsync(fileWords); // Pass mock words list

            // Assert
            _mockWordsDbService.Verify(db => db.WipeDatabaseAsync(), Times.Once);
            foreach (var word in fileWords)
            {
                _mockPrefixTreeClient.Verify(client => client.InsertWordAsync(word), Times.Once);
                _mockWordsDbService.Verify(db => db.WriteWordAsync(word), Times.Once);
            }
        }

        [Fact]
        public async Task StartupAsync_Should_Load_Words_From_DB_Into_PrefixTree()
        {
            // Arrange
            var dbWords = new List<string> { "elephant", "giraffe", "lion" };
            _mockWordsDbService.Setup(db => db.GetAllWordsAsync()).ReturnsAsync(dbWords);

            // Act
            await _service.StartupAsync();

            // Assert
            foreach (var word in dbWords)
            {
                _mockPrefixTreeClient.Verify(client => client.InsertWordAsync(word), Times.Once);
            }
        }
    }
}