using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetroGamingWorld.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Conținutul articolului este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Te rog adaugă un link către o imagine!")]
        public string? Image { get; set; }

        [NotMapped] // Îi spune bazei de date să ignore acest câmp
        [Display(Name = "Imagine Produs")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Column(TypeName = "decimal(6, 2)")]
        [Range(0.01, 9999.99, ErrorMessage = "Prețul trebuie să fie mai mare ca 0")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        [Range(0, 100000, ErrorMessage = "Stocul nu poate fi negativ")]
        public int? Stock { get; set; }

        [Range(1, 5, ErrorMessage = "Ratingul trebuie să fie între 1 și 5")]
        public float? Rating { get; set; }
        public bool IsApproved { get; set; } = false;
        public string? AdminFeedback { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        [InverseProperty("Articles")]
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<ArticleBookmark>? ArticleBookmarks { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}