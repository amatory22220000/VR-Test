namespace MeowStudio.DataSave
{
    public interface IDataHandler<T>
    {
        void Initialize(string path, string fileName, bool useEncryption);
        T Load();
        void Save(T gameData);
    }
}