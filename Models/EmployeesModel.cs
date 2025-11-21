using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    public class EmployeesModel
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string PhoneNumbers { get; set; }
        public string ShiftType { get; set; }
        public int TotalShiftsToday { get; set; }
        public string Status { get; set; }
    }
}
