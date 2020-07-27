using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Test_2.DAL;
using Test_2.Models;
using System.Security.Cryptography;

namespace Test_2.Controllers
{
    public class ProfileUsersController : Controller
    {
        private readonly ProfileUserConnectionLinux db = new ProfileUserConnectionLinux();

        void UpdateProfile()
        {

            ProfileUser profileUser = new ProfileUser();
            List<ProfileUser> TempProfile = new List<ProfileUser>();
            List<ProfileUser> LoginSave = new List<ProfileUser>();
            string LoginNow = "";
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (db != null)
                    {
                        LoginNow = User.Identity.GetUserName();

                        TempProfile = db.Profile.ToList();

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
                                ViewBag.info = "Brak profili uzytkownika " + LoginNow;
                                ViewBag.error_info = "Aby rospocząć proszę dodać profil";
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

                            if (db.Profile.Count() == 0)
                            {
                                ViewBag.error = "Brak profili uzytkownika";
                                ViewBag.error_info = "Nie można wczytać żadnych profili.";
                            }

                        }
                    }
                }
            }
            catch
            {
                ViewBag.error = "Błąd przetwarzania profili uzytkownika w  bazie SQL";
                ViewBag.error_info = "Blad 03:(00) SQL";
                if (LoginSave.Count == 0)
                {
                    ViewBag.info = "Brak profili uzytkownika " + LoginNow;
                    ViewBag.error_info = "Aby rospocząć proszę dodać profil";
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
            Session["LoginSave"] = LoginSave;

        }
 
        [Authorize(Roles = "root,admin")]
        public ActionResult Index()
        {
            return View(db.Profile.ToList());
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult IndexAccount()
        {
            UpdateProfile();
            List<ProfileUser> loginSave;
            if (Session["LoginSave"] != null)
            {
                loginSave = (List<ProfileUser>)Session["LoginSave"];
            }
            else
            {
                loginSave = new List<ProfileUser>();
            }

            return View(loginSave.ToList());
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult CreateIndex()
        {
            return View();
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateIndex([Bind(Include = "ProfileUserId,ProfilNalezyDo,ProfileName,ProfileIp,ProfilePort,ProfileNameUser,ProfileKeyUser,ProfileKeyRoot,ProfileKoment")] ProfileUser profileUser)
        {
            try
            {
                using (Rijndael myRijndael = Rijndael.Create())
                {
                    profileUser.ICryptoTransformKey = myRijndael.Key;
                    profileUser.ICryptoTransformVI = myRijndael.IV;

                    profileUser.ProfileKeyRootByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyRoot, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
                    profileUser.ProfileKeyUserByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyUser, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);

                }
            }
            catch (Exception e)
            {
                Session["viewbag_error"] = "Wystąpił błąd :" + e.Message;
            }

            profileUser.ProfileKeyUser = "key";
            profileUser.ProfileKeyRoot = "key";
            if (ModelState.IsValid)
            {
                profileUser.ProfilNalezyDo = User.Identity.GetUserName();
                db.Profile.Add(profileUser);
                db.SaveChanges();
                return RedirectToAction("IndexAccount");
            }

            return View(profileUser);
        }

        [Authorize(Roles = "root,admin")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "root,admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProfileUserId,ProfilNalezyDo,ProfileName,ProfileIp,ProfilePort,ProfileNameUser,ProfileKeyUser,ProfileKeyRoot,ProfileKoment")] ProfileUser profileUser)
        {
            try
            {
                using (Rijndael myRijndael = Rijndael.Create())
                {
                    profileUser.ICryptoTransformKey = myRijndael.Key;
                    profileUser.ICryptoTransformVI = myRijndael.IV;

                    profileUser.ProfileKeyRootByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyRoot, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
                    profileUser.ProfileKeyUserByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyUser, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);

                }
            }
            catch (Exception)
            {
                Session["viewbag_error"] = "Wystąpił błąd:";
            }

            profileUser.ProfileKeyUser = "key";
            profileUser.ProfileKeyRoot = "key";

            if (ModelState.IsValid)
            {
                db.Profile.Add(profileUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(profileUser);
        }
       
        [Authorize(Roles = "root,admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProfileUser profileUser = db.Profile.Find(id);
            if (profileUser == null)
            {
                return HttpNotFound();
            }
            return View(profileUser);
        }
        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult DetailsIndex(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProfileUser profileUser = db.Profile.Find(id);
            if (profileUser == null)
            {
                return HttpNotFound();
            }
            return View(profileUser);
        }

        [Authorize(Roles = "root,admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProfileUser profileUser = db.Profile.Find(id);

            if (profileUser == null)
            {
                return HttpNotFound();
            }

            profileUser.ProfileKeyRoot = RijndaelExample.DecryptStringFromBytes(profileUser.ProfileKeyRootByte, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
            profileUser.ProfileKeyUser = RijndaelExample.DecryptStringFromBytes(profileUser.ProfileKeyUserByte, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
            Session["tempKey"] = profileUser.ICryptoTransformKey;
            Session["tempVI"] = profileUser.ICryptoTransformVI;
            return View(profileUser);
        }

        [Authorize(Roles = "root,admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProfileUserId,ProfilNalezyDo,ProfileName,ProfileIp,ProfilePort,ProfileNameUser,ProfileKeyUser,ProfileKeyRoot,ProfileKoment")] ProfileUser profileUser)
        {

            try
            {
                profileUser.ICryptoTransformKey = (byte[])Session["tempKey"];
                profileUser.ICryptoTransformVI = (byte[])Session["tempVI"];
                profileUser.ProfileKeyRootByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyRoot, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
                profileUser.ProfileKeyUserByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyUser, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);

                profileUser.ProfileKeyUser = "key";
                profileUser.ProfileKeyRoot = "key";
            }
            catch (Exception e)
            {
                Session["viewbag_success"] = "Wystąpił błąd:" + e.Message;
            }

            if (ModelState.IsValid)
            {
                db.Entry(profileUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(profileUser);
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult EditIndex(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProfileUser profileUser = db.Profile.Find(id);
            if (profileUser == null)
            {
                return HttpNotFound();
            }

            profileUser.ProfileKeyRoot = RijndaelExample.DecryptStringFromBytes(profileUser.ProfileKeyRootByte, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
            profileUser.ProfileKeyUser = RijndaelExample.DecryptStringFromBytes(profileUser.ProfileKeyUserByte, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
            Session["tempKey"] = profileUser.ICryptoTransformKey;
            Session["tempVI"] = profileUser.ICryptoTransformVI;
            var aaaaaa = profileUser.ProfileKeyRoot;
            return View(profileUser);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult EditIndex([Bind(Include = "ProfileUserId,ProfilNalezyDo,ProfileName,ProfileIp,ProfilePort,ProfileNameUser,ProfileKeyUser,ProfileKeyRoot,ProfileKoment")] ProfileUser profileUser)
        {
            try
            {
                profileUser.ICryptoTransformKey = (byte[])Session["tempKey"];
                profileUser.ICryptoTransformVI = (byte[])Session["tempVI"];
                profileUser.ProfileKeyRootByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyRoot, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);
                profileUser.ProfileKeyUserByte = RijndaelExample.EncryptStringToBytes(profileUser.ProfileKeyUser, profileUser.ICryptoTransformKey, profileUser.ICryptoTransformVI);

                profileUser.ProfileKeyUser = "key";
                profileUser.ProfileKeyRoot = "key";
            }
            catch (Exception e)
            {
                Session["viewbag_error"] = "Wystąpił błąd:" + e.Message;
            }

            if (ModelState.IsValid)
            {
                profileUser.ProfilNalezyDo = User.Identity.GetUserName().ToString();
                db.Entry(profileUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexAccount");
            }
            return View(profileUser);
        }
        

        [Authorize(Roles = "root,admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProfileUser profileUser = db.Profile.Find(id);
            if (profileUser == null)
            {
                return HttpNotFound();
            }
            return View(profileUser);
        }

        [Authorize(Roles = "root,admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProfileUser profileUser = db.Profile.Find(id);
            db.Profile.Remove(profileUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult DeleteIndex(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProfileUser profileUser = db.Profile.Find(id);
            if (profileUser == null)
            {
                return HttpNotFound();
            }
            return View(profileUser);
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost, ActionName("DeleteIndex")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedIndex(int id)
        {
            ProfileUser profileUser = null;
            try
            {
                profileUser = db.Profile.Find(id);
                db.Profile.Remove(profileUser);
                db.SaveChanges();

            }
            catch
            {
                Session["viewbag_error"] = "Usuwanie profilu nie powiodło się";
                Session["viewbag_error_info"] = "Bląd krytyczny SQL!";
            }
            UpdateProfile();
            return RedirectToAction("IndexAccount");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }
}
