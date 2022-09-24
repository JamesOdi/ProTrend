namespace ProTrendAPI.Services.UserSevice
{
    public interface IUserService
    {
        Profile? GetProfile();
        Profile? GetMobileProfile(string token);
    }
}
