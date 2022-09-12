namespace API.Helpers
{
    public class LikesParams: PaginationParams
    {
        public string Predicate { get; set; }
        public int UserId { get; set; }

    }
}
