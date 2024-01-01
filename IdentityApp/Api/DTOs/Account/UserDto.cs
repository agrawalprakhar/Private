using System.ComponentModel.DataAnnotations;
using System;

namespace Api.DTOs.Account
{
    public class UserDto
    {
 
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string JWT { get; set; } 
    }
}
