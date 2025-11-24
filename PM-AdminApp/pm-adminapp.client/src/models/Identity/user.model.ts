export default class User {
  constructor(private user: User) {
    // Initialization inside the constructor
    this.user = user
  }
  Password: String = ''
  Id: String = ''
  DiamCountryId: Number = 0
  DiamUserId: Number = 0
  Brands: String = ''
  Roles: String = ''
  GivenName: String = ''
  Surname: String = ''
  Email: String = ''
  UserName: String = ''
  DisplayName: String = ''
  MailNickName: String = ''
}
