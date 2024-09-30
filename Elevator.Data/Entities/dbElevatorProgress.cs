using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elevator.Data.Entities
{
    public class dbElevatorProgress
    {
        [Key]
        public int? ProgressId { get; set; }

        [ForeignKey("db_Elevator")]
        public int CarId { get; set; }
        public int CurrentStatus { get; set; }
        public int CurrentDirection { get; set; }
        public string CurrentFloorsQueued { get; set; }
    }
}