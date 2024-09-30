using System.ComponentModel.DataAnnotations;

namespace Elevator.Data.Entities
{
    public class dbElevator
    {
        [Key]
        public int? CarId { get; set; }
        public string CarName { get; set; }
        public int? CurrentFloor { get; set; }
        public int? ManageThreadId { get; set; }
    }
}