using System.ComponentModel.DataAnnotations;

namespace LearningCrud.Web.Models;

public sealed class LearningItemInputModel
{
    [Required(ErrorMessage = "Enter a title.")]
    [StringLength(120, ErrorMessage = "The title must be 120 characters or fewer.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2_000, ErrorMessage = "The description must be 2,000 characters or fewer.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}
