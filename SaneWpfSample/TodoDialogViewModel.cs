using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using SaneWpf.Framework;

namespace SaneWpfSample
{
    class TodoDialogViewModel : ViewModelBase
    {
        public TodoDialogViewModel()
        {
            InitializeCommand = new AsyncCommand(async _ =>
            {
                using (var httpClient = new HttpClient())
                {
                    var todoString = await httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/todos/" + TodoId);
                    var todo = JsonSerializer.Deserialize<Todo>(todoString);
                    UserId = todo.UserId;
                    Id = todo.Id;
                    Title = todo.Title;
                    Completed = todo.Completed;
                }
            });
        }

        private class Todo
        {
            [JsonPropertyName("userId")]
            public int UserId { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("completed")]
            public bool Completed { get; set; }
        }

        public ICommand InitializeCommand { get; }
        private string _title;

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private int _userId;

        public int UserId
        {
            get => _userId;
            set => Set(ref this._userId, value);
        }

        private int _id;

        public int Id
        {
            get => this._id;
            set => Set(ref this._id, value);
        }

        private bool _completed;

        public bool Completed
        {
            get => _completed;
            set => Set(ref this._completed, value);
        }

        public int TodoId { get; set; }
    }
}
