namespace CoffeeHouseAPI.DTOs
{
    public class StringListSingleton
    {
        private static readonly Lazy<StringListSingleton> _instance =
            new Lazy<StringListSingleton>(() => new StringListSingleton());

        private StringListSingleton()
        {
            Strings = new List<string>(); // Khởi tạo danh sách
        }

        public static StringListSingleton Instance => _instance.Value;

        public List<string> Strings { get; } // Danh sách chỉ có thể thêm/xóa từ bên ngoài

        public void AddString(string item)
        {
            Strings.Add(item);
        }

        public void RemoveString(string item)
        {
            Strings.Remove(item);
        }

        public void RemoveAll()
        {
            Strings.Clear();
        }

        public void PrintAll()
        {
            Console.WriteLine(string.Join(", ", Strings));
        }
    }
}
