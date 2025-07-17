using System.ComponentModel.DataAnnotations;

namespace tobeh.Louvre.Server.Database.Model;

public class UserEntity
{
    [Key]
    public string Id { get; set; }
    
    public UserTypeEnum Type { get; set; }
    
    public string Name { get; set; }
}