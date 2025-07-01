using System.ComponentModel.DataAnnotations;

namespace Eryth.Models.Enums
{
    /// <summary>
    /// Enumeration for sort order direction
    /// </summary>
    public enum SortOrder
    {
        [Display(Name = "Ascending")]
        Ascending = 0,
        [Display(Name = "Descending")]
        Descending = 1
    }
}
