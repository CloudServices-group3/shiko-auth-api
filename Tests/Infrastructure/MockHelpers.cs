using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;


namespace Tests.Infrastructure;

public static class MockHelpers
{
    public static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();

        return new Mock<UserManager<AppUser>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );
    }
}
