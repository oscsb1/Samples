using InCorpApp.Data;


namespace InCorpApp.Services
{
    public class BasicDBService
    {
        private readonly ApplicationDbContext _context;
        public BasicDBService(ApplicationDbContext context)
        {
            _context = context;
        }
        public void ClearTracking()
        {
            _context.ChangeTracker.Clear();
        }
    }
}
