using System.ComponentModel.DataAnnotations;

namespace toobeeh.Louvre.Server.Database.Model;

public class RenderEntity
{
    [Key]
    public string Id { get; set; }
    
    public bool Rendered { get; set; }
    public int RenderDuration { get; set; }
    public int RenderFps { get; set; }
    public int RenderOptimization { get; set; }
    
    public string Title { get; set; }
    public string Drawer { get; set; }
    public string Language { get; set; }
    
    public string? ApprovedDrawerLogin { get; set; }
    public string? ApprovedTitle { get; set; }
    
    public bool Approved { get; set; }
    
    public string CloudOwnerLogin { get; set; }
    public string OwnerCloudId { get; set; }
}