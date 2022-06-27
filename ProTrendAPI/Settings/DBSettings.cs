﻿namespace ProTrendAPI.Settings
{
    public class DBSettings
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UserCollection { get; set; } = null!;
        public string PostsCollection { get; set; } = null!;
        public string CommentsCollection { get; set; } = null!;
        public string LikesCollection { get; set; } = null!;
        public string PromotionsCollection { get; set; } = null!;
        public string FavoritesColection { get; set; } = null!;
        public string UsersProfileCollection { get; set; } = null!;
        public string ChatsCollection { get; set; } = null!;
        public string CategoriesCollection { get; set; } = null!;
        public string TagsCollection { get; set; } = null!;
    }
}
