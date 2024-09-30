using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elevator.Data.Entities
{
    public class dbElevatorRequest
    {
        [Key]
        public int? RequestId { get; set; }

        [ForeignKey("db_Elevator")]
        public int CarId { get; set; }
        public int RequestedDirection { get; set; }
        public int RequestedFromFloor { get; set; }
        public string RequestedFloors { get; set; }
    }
}