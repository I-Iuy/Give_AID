using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        public class DonationDetail
        {
            public string Purpose { get; set; }
            public string Program { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; } 
        }


        public class User
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public decimal TotalDonated { get; set; }
            public bool IsInterested { get; set; }
            public List<DonationDetail> Donations { get; set; }
        }

        private static readonly List<User> Users = new()
{
    new User
    {
        Id = 1,
        UserName = "nguyenvana",
        FullName = "Nguyen Van A",
        Email = "nva@example.com",
        PhoneNumber = "0123456789",
        Address = "123 Nguyen Trai, Hanoi",
        TotalDonated = 200,
        IsInterested = true,
        Donations = new List<DonationDetail>
        {
            new DonationDetail
            {
                Purpose = "Children",
                Program = null,
                Amount = 100,
                Date = new DateTime(2024, 5, 1),
                Status = "Success"
            },
            new DonationDetail
            {
                Purpose = "Children",
                Program = "Scholarship Encouragement",
                Amount = 100,
                Date = new DateTime(2024, 6, 15),
                Status = "Pending"
            }
        }
    },
    new User
    {
        Id = 2,
        UserName = "tranthib",
        FullName = "Tran Thi B",
        Email = "ttb@example.com",
        PhoneNumber = "0987654321",
        Address = "456 Le Loi, Hue",
        TotalDonated = 300,
        IsInterested = false,
        Donations = new List<DonationDetail>
        {
            new DonationDetail
            {
                Purpose = "Environment",
                Program = null,
                Amount = 300,
                Date = new DateTime(2024, 7, 20),
                Status = "Failed"
            }
        }
    }
};


        public IActionResult List()
        {
            return View(Users);
        }

        public IActionResult Details(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
