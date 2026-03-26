using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Huidig wachtwoord is verplicht")]
        [DataType(DataType.Password)]
        [Display(Name = "Huidig wachtwoord")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nieuw wachtwoord is verplicht")]
        [StringLength(100, ErrorMessage = "Het nieuwe wachtwoord moet minimaal {2} en maximaal {1} karakters lang zijn.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nieuw wachtwoord")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Bevestig nieuw wachtwoord")]
        [Compare("NewPassword", ErrorMessage = "Het nieuwe wachtwoord en de bevestiging komen niet overeen.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
