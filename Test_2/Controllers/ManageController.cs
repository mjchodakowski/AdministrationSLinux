using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Test_2.Models;

namespace Test_2.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        { 
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Zmieniono hasło."
                : message == ManageMessageId.SetPasswordSuccess ? "Ustawiono hasło."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }
        
        public ActionResult AddPhoneNumber()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Twój kod zabezpieczający: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        public ActionResult SetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }
            return View(model);
        }
        
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Usunięto logowanie zewnętrzne."
                : message == ManageMessageId.Error ? "Wystąpił błąd."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }
        
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        public ActionResult ChangeNameUsers()
        {
            return View();
        }

        private ApplicationDbContext Namecontex=new ApplicationDbContext();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeNameUsers(ChangeNameUserViewModel model)
        {
            var che = Namecontex.Users.Find(User.Identity.GetUserId());
            var check = UserManager.PasswordHasher.VerifyHashedPassword(che.PasswordHash, model.OldPassword);

            switch (check.ToString())
            {
                case "Success":

                    string oldNameUser = User.Identity.GetUserName();
                    List<ApplicationUser> lista = Namecontex.Users.ToList();
                    if (model.NameUser != model.CNameUser)
                    {
                        ViewBag.error = "Nazwa uzytkownika są różne! ";
                        return View();
                    }
                    bool znalezionoMalpe = false;
                    bool znalezionoKropke = false;

                    for (int i = 0; i < lista.Count(); i++)
                    {
                        if (lista[i].UserName == model.NameUser)
                        {
                            ViewBag.error = "Wybrana e-mail jest już zajęty.";
                            ViewBag.error_pomoc = "Aby rozwiązać ten problem proszę wybrać inną nazwę e-mail!";
                            return View();
                        }
                    }


                    if (model.NameUser == model.CNameUser)
                    {
                        for (int i = 0; i < model.NameUser.Length; i++)
                        {

                            if (model.NameUser[i] == '@')
                            {
                                if (i + 1 < model.NameUser.Length)
                                {
                                    if (model.NameUser[i + 1] != '.' && model.NameUser[i + 1] != '@')
                                    {
                                        for (; i < model.NameUser.Length; i++)
                                        {
                                            if (model.NameUser[i] == '.')
                                            {
                                                znalezionoKropke = true;
                                                if (i + 1 < model.NameUser.Length)
                                                {
                                                    if (model.NameUser[i + 1] != '.' && model.NameUser[i + 1] != '@')
                                                    {
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        ViewBag.error = "Nieprawidłowa struktura e-mail";
                                                        return View();
                                                    }
                                                }
                                                else
                                                {
                                                    ViewBag.error = "Nieprawidłowa struktura e-mail";
                                                    return View();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.error = "Nieprawidłowa struktura e-mail";
                                        return View();
                                    }
                                }
                                else
                                {
                                    ViewBag.error = "Nieprawidłowa struktura e-mail";
                                    return View();
                                }
                            }
                        }

                        if (!znalezionoKropke && !znalezionoMalpe)
                        {
                            ViewBag.error = "Nieprawidłowa struktura e-mail";
                            return View();
                        }

                    }


                    if (ModelState.IsValid)
                    {
                        ApplicationUser te = Namecontex.Users.Find(User.Identity.GetUserId());
                        te.UserName = model.NameUser;
                        te.Email = model.NameUser;


                        Namecontex.Entry(te).State = EntityState.Modified;
                        Namecontex.SaveChanges();

                        Session["viewbag_success"] = "Adres e-mail został zmieniony poprawnie z " + oldNameUser + " na " + model.NameUser + " ";
                        Session["viewbag_success_pomoc"] = "Dane zostaną wprowadzone dopiero po ponowny logowaniu";
                        return RedirectToAction("Index", "Manage");
                    }
                    else
                    {
                        ViewBag.error = "Nieprawidłowa zgodność e-mail.";
                        return View();
                    }
                    
                case "Failed":
                    Session["viewbag_error"] = "Wprowadzono błędne aktualne hasło";
                    Session["viewbag_error_pomoc"] = "Aby zmienić dane logowania hasło musi być wprowadzone poprawnie";
                    return RedirectToAction("Index", "Manage");
                    
                default:
                    Session["viewbag_success"] = "Ok, ale powinien zaktualizować i ponownie wprowadzić hasło";
                    return RedirectToAction("Index", "Manage");
            }
        }


        #region Pomocnicy
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}