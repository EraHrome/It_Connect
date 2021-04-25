using ItConnectionBotApiCore.Enums;

namespace ItConnectionBotApiCore.Models.DbModels
{
    public class User
    {

        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public RoleEnum RoleEnum { get; set; }
        public bool WaitingForTitleCreatingOffer{ get; set; }

    }
}
