﻿using TemplateMVC.Core.Models;

namespace TemplateMVC.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Price { get; set; }
    }
}
