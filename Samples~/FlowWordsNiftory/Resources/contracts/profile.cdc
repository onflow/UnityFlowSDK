access(all) contract Profile
{
    access(contract) var totalUsersCount: UInt64;

    access(all) var publicProfileStoragePath: PublicPath;
    access(all) var storageProfileStoragePath: StoragePath;

    access(all) resource interface IUserProfilePublic
    {
        access(all) let id: UInt64;
        access(all) let address: String;
        access(all) var name: String;

        access(all) fun getUserProfileInfo(): UserProfileInfo;
    }

    access(all) resource UserProfile : IUserProfilePublic
    {
        access(all) let id: UInt64;
        access(all) let address: String;
        access(all) var name: String;

        access(all) fun getUserProfileInfo(): UserProfileInfo
        {
            return UserProfileInfo(self.id, self.name, self.address);
        }

        access(all) fun updateUserName(_ name: String)
        {
            self.name = name;
        }

        init(_ id: UInt64, _ name: String, _ address: String)
        {
            self.id = id;
            self.name = name;
            self.address = address;
        }
    }

    access(all) fun createUserProfile(_ name: String, _ address: String): @UserProfile
    {
        let newUserProfile <- create UserProfile(self.totalUsersCount, name, address);
        self.totalUsersCount = self.totalUsersCount + 1;
        return <- newUserProfile;
    }

    access(all) struct UserProfileInfo
    {
        access(all) let id: UInt64;
        access(all) let name: String;
        access(all) let address: String;

        init(_ id: UInt64, _ name: String, _ address: String)
        {
            self.id = id;
            self.name = name;
            self.address = address;
        }
    }

    init()
    {
        self.totalUsersCount = 0;

        self.publicProfileStoragePath = /public/userProfile;
        self.storageProfileStoragePath = /storage/userProfile;
    }
}