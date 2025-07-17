using Microsoft.EntityFrameworkCore;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Dto;

namespace tobeh.Louvre.Server.Service;

public class UsersService(ILogger<UsersService> logger, AppDatabaseContext db)
{
    public async Task<AuthorizedUserDto?> GetUserByLogin(string login)
    {
        logger.LogTrace("GetUserByLogin({Login})", login);
        
        var user = await db.Users.FindAsync(login);
        if (user == null)
        {
            logger.LogWarning("User not found: {Login}", login);
            return null;
        }

        return new AuthorizedUserDto(user.Id, user.Type, user.Name);
    }
    
    public async Task DeleteUserByLogin(string login)
    {
        logger.LogTrace("DeleteUserByLogin({Login})", login);
        
        var user = await db.Users.FindAsync(login);
        if (user == null)
        {
            logger.LogWarning("User not found for deletion: {Login}", login);
            return;
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        
        logger.LogInformation("User deleted: {Login}", login);
    }
    
    public async Task<AuthorizedUserDto> AddUser(AuthorizedUserDto userDto)
    {
        logger.LogTrace("AddUser({UserDto})", userDto);
        
        var existingUser = await db.Users.FindAsync(userDto.Login);
        if (existingUser != null)
        {
            logger.LogWarning("User already exists: {Login}", userDto.Login);
            return new AuthorizedUserDto(existingUser.Id, existingUser.Type, existingUser.Name);
        }

        var newUser = new Database.Model.UserEntity
        {
            Id = userDto.Login,
            Type = userDto.UserType,
            Name = userDto.Name
        };
        
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        
        logger.LogInformation("New user added: {Login}", userDto.Login);

        return userDto;
    }
    
    public async Task<IEnumerable<AuthorizedUserDto>> GetAllUsers()
    {
        logger.LogTrace("GetAllUsers()");
        
        return await db.Users
            .Select(u => new AuthorizedUserDto(u.Id, u.Type, u.Name))
            .ToListAsync();
    }
}