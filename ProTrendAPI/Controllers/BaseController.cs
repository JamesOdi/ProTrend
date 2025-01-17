﻿using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services;

namespace ProTrendAPI.Controllers
{
    public class BaseController : Controller
    {
        public readonly NotificationService? _notificationService;
        public readonly CategoriesService? _categoriesService;
        public readonly RegistrationService? _regService;
        public readonly ProfileService? _profileService;
        public readonly SearchService? _searchService;
        public readonly PostsService? _postsService;
        public readonly IUserService? _userService;
        public readonly TagsService? _tagsService;
        public readonly Profile? _profile;
        public readonly PaymentService? _paymentService;

        public BaseController(IServiceProvider serviceProvider)
        {
            _regService = serviceProvider.GetService<RegistrationService>();
            _userService = serviceProvider.GetService<IUserService>();
            _categoriesService = serviceProvider.GetService<CategoriesService>();
            _notificationService = serviceProvider.GetService<NotificationService>();
            _postsService = serviceProvider.GetService<PostsService>();
            _profileService = serviceProvider.GetService<ProfileService>();
            _profile = _userService.GetProfile();
            _searchService = serviceProvider.GetService<SearchService>();
            _tagsService = serviceProvider.GetService<TagsService>();
            _paymentService = serviceProvider.GetService<PaymentService>();
        }
    }
}
