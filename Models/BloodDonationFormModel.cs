using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace WebApplication1.Models
{
    public class BloodDonationFormModel : PageModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Blood Type is required")]
        public string? BloodType { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastDonationDate { get; set; }

        public bool HasChronicIllness { get; set; }
        public bool HasRecentSurgery { get; set; }
        public bool HasTraveledRecently { get; set; }

        public string? AdditionalNotes { get; set; }

        [Required(ErrorMessage = "Consent is required")]
        public bool ConsentToDonate { get; set; }
    }
}