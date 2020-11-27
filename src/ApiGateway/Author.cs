namespace ApiGateway
{
    public class Author
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Book
    {
        public string Title { get; set; } = string.Empty;
        public Author Author { get; set; } = new Author();
    }

    public class Query
    {
        public Book GetBook()
        {
            return new Book
            {
                Author =
                {
                    Name = "Jon Skeet"
                },
                Title = "C# in depth"
            };
        }
    }
}