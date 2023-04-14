namespace CourseLibrary.API.Models
{
    public class AuthorDto
    {
        /// <summary>
        /// the outer facing model represents what's sent over the wire 
        /// </summary>
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string MainCategory { get; set; }
    }
}
