using ItConnectionBotApiCore.Enums;
using System;

namespace ItConnectionBotApiCore.Models.DbModels
{
    public class Offer
    {

        public int Id { get; set; }
        public string CreatedUserId { get; set; }
        public string TaskTitle { get; set; }
        public ItTaskStatusEnum ItTaskStatus { get; set; }
        public DateTime DateOfCreating { get; set; }

    }
}
