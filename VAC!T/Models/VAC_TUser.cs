using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace VAC_T.Models;

// Add profile data for application users by adding properties to the VAC_TUser class
public class VAC_TUser : IdentityUser
{

    public string? Name { get; set; }


    public string? Adress { get; set; }


    public string? ProfilePicture { get; set; }

    public string? Motivation { get; set; }

    public string? CV { get; set; }

    public ICollection<Solicitation> Solicitations { get; set; }

}

