﻿namespace ForumNet.Web.ViewModels.Posts
{
    using System.Collections.Generic;

    using Ganss.XSS;

    using Common;

    public class PostsDeleteDetailsViewModel
    {
        private readonly IHtmlSanitizer htmlSanitizer;

        public PostsDeleteDetailsViewModel()
        {
            this.htmlSanitizer = new HtmlSanitizer();
            this.htmlSanitizer.AllowedTags.Add(GlobalConstants.IFrameTag);
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string CreatedOn { get; set; }

        public string Description { get; set; }

        public string SanitizedDescription
            => this.htmlSanitizer.Sanitize(this.Description);

        public int RepliesCount { get; set; }

        public int Views { get; set; }

        public int Likes { get; set; }

        public int Loves { get; set; }

        public int HahaCount { get; set; }

        public int WowCount { get; set; }

        public int SadCount { get; set; }

        public int AngryCount { get; set; }

        public PostsAuthorDetailsViewModel Author { get; set; }

        public PostsCategoryDetailsViewModel Category { get; set; }

        public IEnumerable<PostsTagsDetailsViewModel> Tags { get; set; }
    }
}