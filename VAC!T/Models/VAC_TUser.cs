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
    [Display(Name = "Naam")]
    public string? Name { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Geboortedatum")]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [DataType(DataType.PostalCode)]
    public string? Postcode { get; set; }

    [Display(Name = "Plaats")]
    public string? Residence { get; set; }

    [DataType(DataType.ImageUrl)]
    public string? ProfilePicture { get; set; }

    [Display(Name = "Motivatie")]
    public string? Motivation { get; set; }

    [DataType(DataType.Url)]
    public string? CV { get; set; }

    public Company? Company { get; set; }

    public ICollection<Solicitation>? Solicitations { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
    public ICollection<Appointment>? AppointmentsCandidate { get; set; }

}

