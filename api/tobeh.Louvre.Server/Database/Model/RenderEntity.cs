using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace tobeh.Louvre.Server.Database.Model;

[Index(nameof(CloudOwnerTypoId), nameof(OwnerCloudId), IsUnique = true)]
public class RenderEntity
{
    [Key]
    public string Id { get; set; }
    
    public bool Rendered { get; set; }
    public int? RenderDuration { get; set; }
    public int? RenderFps { get; set; }
    public int? RenderOptimization { get; set; }
    
    public string Title { get; set; }
    public string Drawer { get; set; }
    public string Language { get; set; }
    
    public int? ApprovedDrawerTypoId { get; set; }
    public string? ApprovedTitle { get; set; }
    
    public bool Approved { get; set; }
    public int? CloudOwnerTypoId { get; set; }
    public string OwnerCloudId { get; set; }
}