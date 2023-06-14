using Study.Net.EFCoreEnvironment.DbContexts;
using Study.Net.IBaseRepository;
using Study.Net.Model;

namespace Study.Net.BaseRepository;

public class UserRepository:BaseRepository<User>,IUserRepository
{
    private readonly MySqlDbContext _dbContext;

    public UserRepository(MySqlDbContext dbContext)
    {
        base._dbContext = dbContext;
        _dbContext = dbContext;
    }
}
