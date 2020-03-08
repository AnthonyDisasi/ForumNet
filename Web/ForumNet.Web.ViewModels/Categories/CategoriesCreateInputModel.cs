﻿namespace ForumNet.Web.ViewModels.Categories
{
    using System.ComponentModel.DataAnnotations;

    using Services.Common.Attributes;

    public class CategoriesCreateInputModel
    {
        [Required]
        [MaxLength(50)]
        [ValidateCategoryName]
        public string Name { get; set; }
    }
}