namespace IngestorService.Services
{
    public interface IWordsDbService
    {
        Task WipeDatabaseAsync();
        Task<IEnumerable<string>> GetAllWordsAsync();
        Task WriteWordAsync(string word);
        Task DeleteWordAsync(string word);
        Task ProvisionDatabaseAsync(); 

        event EventHandler<string> OnWordAdded;
        event EventHandler<string> OnWordDeleted;
    }
}
