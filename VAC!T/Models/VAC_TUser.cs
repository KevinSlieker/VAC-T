using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace VAC_T.Models;

// Add profile data for application users by adding properties to the VAC_TUser class
public class VAC_TUser : IdentityUser
{

    public string? Name { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    public string? Address { get; set; }

    [DataType(DataType.ImageUrl)]
    public string? ProfilePicture { get; set; }

    public string? Motivation { get; set; }

    [DataType(DataType.Url)]
    public string? CV { get; set; }

    public ICollection<Solicitation> Solicitations { get; set; }

}

