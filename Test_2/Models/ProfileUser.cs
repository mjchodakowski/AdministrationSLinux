using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Test_2.Models
{
    public class ProfileUser
    {

        public int ProfileUserId { get; set; }

        [Display(Name = "Do profilu posiada dostęp użytkownik: ")]
        [Required(ErrorMessage = "Nazwa przynaleznego profilu nie może być pusta! ")]
        public string ProfilNalezyDo { get; set; }

        [Display(Name = "Nazwa profilu: ")]
        [Required(ErrorMessage = "Nazwa profilu nie może być pusta!")]
        [StringLength(15, ErrorMessage = "Nazwa profilu nie może mieć mniej znaków niż 1!; x>1")]
        public string ProfileName { get; set; }

        [Display(Name = "Adress IP: ")]
        [Required(ErrorMessage = "Pole adresu nie może być puste!")]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "Nieprawidłowy format adresu IP (XXX.XXX.XXX.XXX)")]
        [StringLength(15, ErrorMessage = "Nieprawidłowa długość adresu IP. ( Adres musi składać sie z 4 par trzy cyfrowych cyfr.)")]

        public string ProfileIp { get; set; }

        [Display(Name = "Port ")]
        [RegularExpression(@"^\d*", ErrorMessage = "Port musi być numerem")]
        [Required(ErrorMessage = "Pole PORT nie może być puste!")]
        public string ProfilePort { get; set; }

        [Display(Name = "Nazwa użytkownika ")]
        public string ProfileNameUser { get; set; }

        [Display(Name = "Hasło użytkownika ")]
        [DataType(DataType.Password)]
        [Required]
        public string ProfileKeyUser { get; set; }
        public byte[] ProfileKeyUserByte { get; set; }

        [Display(Name = "Hasło Użytkownika root ")]
        [DataType(DataType.Password)]
        [Required]
        public string ProfileKeyRoot { get; set; }
        public byte[] ProfileKeyRootByte { get; set; }
        
        public byte[] ICryptoTransformKey { get; set; }
        public byte[] ICryptoTransformVI { get; set; }

    }
}