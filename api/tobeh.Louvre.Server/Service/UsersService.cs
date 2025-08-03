using Microsoft.EntityFrameworkCore;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Service;

public class UsersService(ILogger<UsersService> logger, AppDatabaseContext db)
{
    public async Task<UserDto?> GetUserByTypoId(int typoId)
    {
        logger.LogTrace("GetUserByTypoId({Login})", typoId);
        
        var user = await db.Users.FindAsync(typoId);
        if (user == null)
        {
            logger.LogWarning("User not found: {Login}", typoId);
            return null;
        }

        return new UserDto(user.Id, user.Type, user.Name);
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
    
    public async Task<UserDto> AddUser(UserDto userDto)
    {
        logger.LogTrace("AddUser({UserDto})", userDto);
        
        var existingUser = await db.Users.FindAsync(userDto.TypoId);
        if (existingUser != null)
        {
            logger.LogWarning("User already exists: {Login}", userDto.TypoId);
            return new UserDto(existingUser.Id, existingUser.Type, existingUser.Name);
        }

        var newUser = new Database.Model.UserEntity
        {
            Id = userDto.TypoId,
            Type = userDto.UserType,
            Name = userDto.Name
        };
        
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        
        logger.LogInformation("New user added: {Login}", userDto.TypoId);

        return userDto;
    }
    
    public async Task<IEnumerable<UserDto>> GetAllUsers()
    {
        logger.LogTrace("GetAllUsers()");
        
        return await db.Users
            .Select(u => new UserDto(u.Id, u.Type, u.Name))
            .ToListAsync();
    }
    
    public async Task<UserDto> RenameUser(int typoId, string newName)
    {
        logger.LogTrace("RenameUser({typoId}, {NewName})", typoId, newName);
        
        var user = await db.Users.FindAsync(typoId);
        if (user == null)
        {
            logger.LogWarning("User not found for renaming: {Login}", typoId);
            throw new KeyNotFoundException($"User with typoId {typoId} not found.");
        }

        user.Name = newName;
        db.Users.Update(user);
        await db.SaveChangesAsync();
        
        logger.LogInformation("User renamed: {typoId} to {NewName}", typoId, newName);

        return new UserDto(user.Id, user.Type, user.Name);
    }
    
    public async Task<UserDto> ChangeUserType(int typoId, UserTypeEnum newType)
    {
        logger.LogTrace("ChangeUserType({typoId}, {newType})", typoId, newType);
        
        var user = await db.Users.FindAsync(typoId);
        if (user == null)
        {
            logger.LogWarning("User not found for promoting: {typoId}", typoId);
            throw new KeyNotFoundException($"User with typoId {typoId} not found.");
        }

        user.Type = newType;
        db.Users.Update(user);
        await db.SaveChangesAsync();
        
        logger.LogInformation("User type changed: {typoId} to {newType}", typoId, newType);

        return new UserDto(user.Id, user.Type, user.Name);
    }
}