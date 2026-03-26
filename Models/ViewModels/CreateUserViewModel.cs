using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Gebruikersnaam is verplicht")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Gebruikersnaam moet tussen 3 of 50 karakters zijn.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol is verplicht")]
        public string Role { get; set; } = "Personeel";
    }
}
