﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;
using Terminal.Core.Objects;
using Terminal.Core.ExtensionMethods;
using Terminal.Core.Settings;

namespace Terminal.Core.Data.Repositories
{
    /// <summary>
    /// Repository for persisting data to the Entity Framework data context.
    /// </summary>
    public class ReplyRepository : IReplyRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public ReplyRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        /// <summary>
        /// Adds a reply to the data context.
        /// </summary>
        /// <param name="reply">The reply to be added.</param>
        public void AddReply(Reply reply)
        {
            _entityContainer.Replies.Add(reply);
        }

        /// <summary>
        /// Updates an existing reply in the data context.
        /// </summary>
        /// <param name="reply">The reply to be updated.</param>
        public void UpdateReply(Reply reply)
        {
            
        }

        /// <summary>
        /// Deletes a reply from the data context.
        /// </summary>
        /// <param name="reply">The reply to be deleted.</param>
        public void DeleteReply(Reply reply)
        {
            _entityContainer.Replies.Remove(reply);
        }

        /// <summary>
        /// Gets a reply from the data context by its unique ID.
        /// </summary>
        /// <param name="replyID">The unique ID of the reply.</param>
        /// <returns>A reply entity.</returns>
        public Reply GetReply(long replyID)
        {
            var query = _entityContainer.Replies.Where(x => x.ReplyID == replyID);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Gets all replies from the data context by the unique topic ID.
        /// </summary>
        /// <param name="topicID">The unique topic ID.</param>
        /// <param name="isModerator">True if moderator-only replies should be included.</param>
        /// <returns>An enumerable list of replies.</returns>
        public CollectionPage<Reply> GetReplies(long topicID, int page, int itemsPerPage, bool isModerator)
        {
            var replies = _entityContainer.Replies.AsQueryable();
            var totalReplies = 0;
            if (isModerator)
                replies = replies
                    .Where(x => x.TopicID == topicID)
                    .OrderBy(x => x.PostedDate);
            else
                replies = replies
                    .Where(x => x.TopicID == topicID)
                    .Where(x => !x.ModsOnly)
                    .OrderBy(x => x.PostedDate);

            totalReplies = replies.Count();


            int totalPages = totalReplies.NumberOfPages(itemsPerPage);
            if (totalPages <= 0)
                totalPages = 1;

            if (page > totalPages)
                return GetReplies(topicID, totalPages, itemsPerPage, isModerator);
            else if (page < 1)
                return GetReplies(topicID, 1, itemsPerPage, isModerator);
            else
            {
                if (totalReplies > itemsPerPage)
                    replies = replies
                        .Skip((page - 1) * itemsPerPage)
                        .Take(itemsPerPage)
                        .OrderBy(x => x.PostedDate);

                return new CollectionPage<Reply>
                {
                    Items = replies.ToList(),
                    TotalItems = totalReplies,
                    TotalPages = totalPages
                };
            }
        }
    }

    /// <summary>
    /// Repository for storing topic replies.
    /// </summary>
    public interface IReplyRepository
    {
        /// <summary>
        /// Adds a reply to the repository.
        /// </summary>
        /// <param name="reply">The reply to be added.</param>
        void AddReply(Reply reply);

        /// <summary>
        /// Updates an existing reply in the repository.
        /// </summary>
        /// <param name="reply">The reply to be added.</param>
        void UpdateReply(Reply reply);

        /// <summary>
        /// Deletes a reply from the repository.
        /// </summary>
        /// <param name="reply">The reply to be deleted.</param>
        void DeleteReply(Reply reply);

        /// <summary>
        /// Get a reply by its unique ID.
        /// </summary>
        /// <param name="replyID">The unique ID of the reply.</param>
        /// <returns>A reply entity.</returns>
        Reply GetReply(long replyID);

        /// <summary>
        /// Get all replies for a single topic.
        /// </summary>
        /// <param name="topicID">The unique ID of the topic.</param>
        /// <param name="isModerator">True if moderator-only replies should be returned.</param>
        /// <returns>An enumerable list of replies.</returns>
        CollectionPage<Reply> GetReplies(long topicID, int page, int itemsPerPage, bool isModerator);
    }
}