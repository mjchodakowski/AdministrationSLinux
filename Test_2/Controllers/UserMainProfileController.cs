using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Test_2.Models;
using Test_2.DAL;
using Microsoft.AspNet.Identity;

namespace Test_2.Controllers
{
    public class UserMainProfileController : Controller
    {
        
        private ProfileUserConnectionLinux context = new ProfileUserConnectionLinux();
        public ProfileUserConnectionLinux db = new ProfileUserConnectionLinux();
        private List<ProfileUser> LoginSave;
        private string LoginNow;
        
        [Authorize(Roles = "root,admin,user-full,user-part-server")]
        public ActionResult Index()
        {
            if (null == Session["TestConnect"])
            {
                Session["TestConnect"] = false;
            }
            ProfileUser profileUser = new ProfileUser();
            List<ProfileUser> TempProfile = new List<ProfileUser>();
            LoginSave = new List<ProfileUser>();

            if (Session["ViewBag_ip"] != null) { ViewBag.ip = (string)Session["ViewBag_ip"]; } else { ViewBag.ip = ""; }
            if (Session["ViewBag_port"] != null) { ViewBag.port = (string)Session["ViewBag_port"]; } else { ViewBag.port = ""; }
            if (Session["ViewBag_loginUser"] != null) { ViewBag.loginUser = (string)Session["ViewBag_loginUser"]; } else { ViewBag.loginUser = ""; }
            if (Session["ViewBag_loginKey"] != null) { ViewBag.loginKey = (string)Session["ViewBag_loginKey"]; } else { ViewBag.loginKey = ""; }
            if (Session["ViewBag_rootKey"] != null) { ViewBag.rootKey = (string)Session["ViewBag_rootKey"]; } else { ViewBag.rootKey = ""; }

            try
                {
                if (User.Identity.IsAuthenticated)
                {
                    if (db != null)
                    {
                        LoginNow = User.Identity.GetUserName();
                        
                        TempProfile = db.Profile.ToList();
                        int ii;
                        for (ii = 0; ii < TempProfile.Count(); ii++)
                        {
                            if (TempProfile[ii].ProfileName == profileUser.ProfileName)
                            {
                                break;
                            }
                        }
                        Session["NumberChangeProfile"] = ii;
                        
                        if (LoginNow != null)
                        {
                            for (int i = 0; i < db.Profile.Count(); i++)
                            {
                                profileUser = TempProfile[i];
                                if (profileUser != null)
                                {
                                    if (profileUser.ProfilNalezyDo == LoginNow)
                                    {
                                        LoginSave.Add(profileUser);
                                    }
                                }
                            }
                            if (LoginSave.Count == 0)
                            {
                                profileUser = new ProfileUser
                                {
                                    ProfileIp = "",
                                    ProfileKeyRoot = "",
                                    ProfileKeyUser = "",
                                    ProfileName = "-",
                                    ProfileNameUser = "-",
                                    ProfilePort = "-",
                                    ProfilNalezyDo = "-"
                                };
                                LoginSave.Add(profileUser);
                            }

                            if (db.Profile.Count()==0)
                            {
                                Session["viewbag_error"] = "Brak profili uzytkownika";
                                Session["viewbag_error_pomoc"] = "SQL";
                            }
                        }
                    }
                }
            }
            catch
            {
                Session["viewbag_error"] = "Błąd przetwarzania profili uzytkownika w  bazie SQL";
                Session["viewbag_error_pomoc"] = "Blad 03:(00) SQL";
                if (LoginSave.Count == 0)
                {
                    profileUser = new ProfileUser
                    {
                        ProfileIp = "",
                        ProfileKeyRoot = "",
                        ProfileKeyUser = "",
                        ProfileName = "-",
                        ProfileNameUser = "-",
                        ProfilePort = "-",
                        ProfilNalezyDo = "-"
                    };
                    LoginSave.Add(profileUser);
                }
            }
            ViewBag.ProfileName = new SelectList(LoginSave, "ProfileName", "ProfileName");
            Session["LoginSave"] = LoginSave;
            return View();
        }


        [Authorize(Roles = "root,admin,user-full,user-part-server")]
        [HttpPost]
        public ActionResult Connect()
        {
            
            Session["TestConnect"] = true;
            return RedirectToAction("Index", "UserMainProfile");
        }


        [Authorize(Roles = "root,admin,user-full,user-part-server")]
        [HttpPost]
        public ActionResult ProfileChange(ProfileUser wybranyProfil)
        {
            Session["WybranyProfil"] = 1;
            Session["loginUser"] = wybranyProfil.ProfileNameUser;
            Session["loginKey"] = wybranyProfil.ProfileKeyUser;
            Session["rootKey"] = wybranyProfil.ProfileKeyRoot;
            if (Session["LoginSave"]!=null)
            {
                LoginSave = (List<ProfileUser>)Session["LoginSave"];
            } else
            {
                Session["ViewBag_error"] = "Brak wybranego profilu";
                Session["ViewBag_error_info"]= "Proszę wybrać profil!";
                return RedirectToAction("Index");
            }

            int i;
            for(i=0;i< LoginSave.Count;i++)
            {
                if(LoginSave[i].ProfileName==wybranyProfil.ProfileName)
                {
                    wybranyProfil = LoginSave[i];
                }
            }


            if (wybranyProfil.ProfilePort!=null)
                {
                Session["ViewBag_ip"] = wybranyProfil.ProfileIp;
                Session["ViewBag_port"] = wybranyProfil.ProfilePort;
                Session["ViewBag_loginUser"] = wybranyProfil.ProfileNameUser;
                Session["ViewBag_loginKey"] = RijndaelExample.DecryptStringFromBytes(wybranyProfil.ProfileKeyUserByte, wybranyProfil.ICryptoTransformKey, wybranyProfil.ICryptoTransformVI);
                Session["ViewBag_rootKey"] = RijndaelExample.DecryptStringFromBytes(wybranyProfil.ProfileKeyRootByte, wybranyProfil.ICryptoTransformKey, wybranyProfil.ICryptoTransformVI);
            }
          
            return RedirectToAction("Index");
        }



    }
}