using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;


namespace Tests.Infrastructure;

public static class MockHelpers
{
    // I mock the "UserManager" because i don't want to use a real database for testning.

    public static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>(); // mock IUserStore that UserManager uses to read/write users.

        return new Mock<UserManager<AppUser>>(
                store.Object,                                   // Store.Object are the mock for IUserStore.
                null, null, null, null, null, null, null, null  
            ); // The constructor for UserManager contains multipy dependencies but they should be null in the test because they wont be used.
    }
}
