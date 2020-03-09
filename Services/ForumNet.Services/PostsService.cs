﻿namespace ForumNet.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;

    using Common.Extensions;
    using Contracts;
    using Data;
    using Data.Models;
    using Data.Models.Enums;

    public class PostsService : IPostsService
    {
        private readonly ForumDbContext db;
        private readonly IMapper mapper;
        private readonly IDateTimeProvider dateTimeProvider;

        public PostsService(ForumDbContext db, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            this.db = db;
            this.mapper = mapper;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<int> CreateAsync(
            string title,
            PostType type,
            string description,
            string authorId,
            int categoryId,
            string imageUrl = null,
            string videoUrl = null)
        {
            if (videoUrl != null && type == PostType.Video)
            {
                videoUrl = videoUrl.UrlToYouTubeEmbed();
            }

            var post = new Post
            {
                Title = title,
                Type = type,
                ImageUrl = imageUrl,
                VideoUrl = videoUrl,
                Description = description,
                CreatedOn = this.dateTimeProvider.Now(),
                AuthorId = authorId,
                CategoryId = categoryId
            };

            await this.db.Posts.AddAsync(post);
            await this.db.SaveChangesAsync();

            return post.Id;
        }

        public async Task EditAsync(
            int id,
            string title,
            string description,
            int categoryId,
            IEnumerable<int> tagIds,
            string imageUrl = null,
            string videoUrl = null)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            
            await this.RemoveTagsAsync(id, post);

            post.Title = title;
            post.Description = description;
            post.ImageUrl = imageUrl;
            post.VideoUrl = videoUrl;
            post.CategoryId = categoryId;
            post.ModifiedOn = this.dateTimeProvider.Now();

            await this.AddTagsAsync(id, tagIds);
            await this.db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.IsDeleted = true;
            post.DeletedOn = this.dateTimeProvider.Now();

            await this.db.SaveChangesAsync();
        }

        public async Task PinAsync(int id)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.IsPinned = true;
            post.ModifiedOn = this.dateTimeProvider.Now();

            await this.db.SaveChangesAsync();
        }

        public async Task UnpinAsync(int id)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.IsPinned = false;
            post.ModifiedOn = this.dateTimeProvider.Now();

            await this.db.SaveChangesAsync();
        }

        public async Task ViewAsync(int id)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.Views++;

            await this.db.SaveChangesAsync();
        }

        public async Task LikeAsync(int id)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.Likes++;

            await this.db.SaveChangesAsync();
        }

        public async Task DislikeAsync(int id)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.Likes--;

            await this.db.SaveChangesAsync();
        }

        public async Task AddTagsAsync(int id, IEnumerable<int> tagIds)
        {
            var post = await this.db.Posts.FirstOrDefaultAsync(p => p.Id == id);

            foreach (var tagId in tagIds)
            {
                post.Tags.Add(new PostTag
                {
                    PostId = id,
                    TagId = tagId
                });
            }

            await this.db.SaveChangesAsync();
        }

        public async Task<int> GetCountByUserId(string userId)
        {
            var count = await this.db.Posts
                .Where(p => p.AuthorId == userId)
                .CountAsync();

            return count;
        }

        public async Task<TModel> GetByIdAsync<TModel>(int id)
        {
            var post = await this.db.Posts
                .Where(p => p.Id == id && !p.IsDeleted)
                .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return post;
        }

        public async Task<IEnumerable<TModel>> GetAllAsync<TModel>()
        {
            var posts = await this.db.Posts
                .Where(p => !p.IsPinned && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<TModel>> GetAllPinnedAsync<TModel>()
        {
            var posts = await this.db.Posts
                .Where(p => p.IsPinned && !p.IsDeleted)
                .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<TModel>> GetAllByUserIdAsync<TModel>(string userId)
        {
            var posts = await this.db.Posts
                .Where(p => p.AuthorId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<TModel>> GetAllByCategoryIdAsync<TModel>(int categoryId)
        {
            var posts = await this.db.Posts
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<TModel>> GetAllWithDeletedAsync<TModel>()
        {
            var posts = await this.db.Posts
                .OrderByDescending(p => p.CreatedOn)
                .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();

            return posts;
        }

        private async Task RemoveTagsAsync(int id, Post post)
        {
            var postTags = await this.db.PostsTags.Where(pt => pt.PostId == id).ToListAsync();
            foreach (var tag in postTags)
            {
                post.Tags.Remove(tag);
            }

            await this.db.SaveChangesAsync();
        }
    }
}