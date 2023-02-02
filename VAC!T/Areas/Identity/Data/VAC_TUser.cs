using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace VAC_T.Areas.Identity.Data;

// Add profile data for application users by adding properties to the VAC_TUser class
public class VAC_TUser : IdentityUser
{
    [PersonalData]
    public string? Name { get; set; }

    [PersonalData]
    public string? Adress { get; set; }

    [PersonalData]
    public string? ProfilePicture { get; set; }

    [PersonalData]
    public string? Motivation { get; set; }

    [PersonalData]
    public string? CV { get; set; }

}

