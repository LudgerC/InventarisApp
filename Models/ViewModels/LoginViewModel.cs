using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Gebruikersnaam is verplicht")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
