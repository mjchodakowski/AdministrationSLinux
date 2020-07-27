using Microsoft.AspNet.Identity;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Test_2.DAL;
using Test_2.Models;

namespace Test_2.Controllers
{
    public class ServerFilesController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();

        private serverFileBD db = new serverFileBD();


        public string SizeFilePText(double lpt)
        {
            string dopisek;
            if (lpt * 1.0E-12 > 1) { dopisek = (lpt / 1000000000000) + "TB"; }
            else
            {
                if (lpt * 1.0E-9 > 1) { dopisek = (lpt / 1000000000) + "GB"; }
                else
                {
                    if (lpt * 1.0E-6 > 1) { dopisek = (lpt / 1000000) + "MB"; }
                    else
                    {
                        if (lpt * 0.001 > 1) { dopisek = (lpt / 1000) + "KB"; }
                        else
                        {
                            dopisek = (lpt) + "B";
                        }
                    }
                }
            }
            return dopisek;

        }

        public SftpClient SftpClientConnect()
        {
            string login, password;
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];

            if (Session["WhatLogon"] == null)
            {
                Session["WhatLogon"] = 1;
            }

            if ((int)Session["WhatLogon"] != 0)
            {
                if ((int)Session["WhatLogon"] == 2)
                {
                    login = "root";
                    password = (string)Session["ViewBag_rootKey"];
                }
                else
                {
                    login = (string)Session["ViewBag_loginUser"];
                    password = (string)Session["ViewBag_loginKey"];
                }

                try
                {
                    SftpClientFile = new SftpClient((string)Session["ViewBag_ip"], Int32.Parse((string)Session["ViewBag_port"]), login, password);
                }
                catch
                {
                    Session["viewbag_error"] = "Aby utworzyć połączenie musi być wybrany profil!";
                    Session["viewbag_error_pomoc"] = "";
                }

                return SftpClientFile;
            }
            else
            {
                return null;
            }
        }

        public int LookingForNameServer()
        {
            string path_;
            int i;
            for (i = 0; i < 10000; i++)
            {
                int tl = (i.ToString()).Length;
                string nazwa = "";
                if (tl < 2)
                {
                    nazwa = i.ToString()[0].ToString();
                }
                else
                {
                    if (tl < 3)
                    {
                        nazwa = i.ToString()[0].ToString() + (i.ToString())[1].ToString();
                    }
                    else
                    {
                        if (tl < 4)
                        {
                            nazwa = i.ToString()[0].ToString() + (i.ToString())[1].ToString() + (i.ToString())[2].ToString();
                        }
                        else
                        {
                            if (tl < 5)
                            {
                                nazwa = i.ToString()[0].ToString() + (i.ToString())[1].ToString() + (i.ToString())[2].ToString() + (i.ToString())[3].ToString();
                            }
                        }
                    }
                }
                try
                {
                    path_ = Path.Combine(Server.MapPath("~/FileUsers"), nazwa);
                    var fs = System.IO.File.Open(path_, FileMode.Open);
                    fs.Close();
                }
                catch
                {
                    break;
                }
            }
            return i;
        }
        
        private void UstanowLimity()
        {
            if (User.IsInRole("user-full-10-50")) { Session["FullFile"] = 50; Session["FullSizeFile"] = "10485760"; }
            if (User.IsInRole("user-full-100-200")) { Session["FullFile"] = 200; Session["FullSizeFile"] = "104857600"; }
            if (User.IsInRole("root")) { Session["FullFile"] = 65535; Session["FullSizeFile"] = "107374182400"; }
            if (User.IsInRole("admin")) { Session["FullFile"] = 65535; Session["FullSizeFile"] = "107374182400"; }
            if (User.IsInRole("user-full")) { Session["FullFile"] = 65535; Session["FullSizeFile"] = "107374182400"; }
            if (User.IsInRole("user-full-10-50")) { Session["FullFile"] = 50; Session["FullSizeFile"] = "10485760"; }
            if (User.IsInRole("user-full-100-200")) { Session["FullFile"] = 200; Session["FullSizeFile"] = "104857600"; }
            if (User.IsInRole("user-part-server")) { Session["FullFile"] = 0; Session["FullSizeFile"] = "0"; }
            if (User.IsInRole("user-part-ftp-full")) { Session["FullFile"] = 65535; Session["FullSizeFile"] = "107374182400"; }//
            if (User.IsInRole("user-part-ftp-10-50")) { Session["FullFile"] = 50; Session["FullSizeFile"] = "10485760"; }//
            if (User.IsInRole("user-part-ftp-100-200")) { Session["FullFile"] = 200; Session["FullSizeFile"] = "104857600"; }//
            if (User.IsInRole("user-part-ftp-1000-500")) { Session["FullFile"] = 500; Session["FullSizeFile"] = "1073741824"; }//
            if (User.IsInRole("user-part-ftp-10000-1000")) { Session["FullFile"] = 1000; Session["FullSizeFile"] = "10737418240"; }//
        }

        [Authorize(Roles = "root,admin")]
        public ActionResult Index()
        {
            UstanowLimity();
            if (Session["FullFile"] == null)
            {
                Session["FullFile"] = 0;
            }
            if (Session["ViewBag_ip"] != null) { ViewBag.ip = (string)Session["ViewBag_ip"]; } else { ViewBag.ip = ""; }
            if (Session["ViewBag_port"] != null) { ViewBag.port = (string)Session["ViewBag_port"]; } else { ViewBag.port = ""; }
            if (Session["ViewBag_loginUser"] != null) { ViewBag.loginUser = (string)Session["ViewBag_loginUser"]; } else { ViewBag.loginUser = ""; }
            if (Session["ViewBag_loginKey"] != null) { ViewBag.loginKey = (string)Session["ViewBag_loginKey"]; } else { ViewBag.loginKey = ""; }
            if (Session["ViewBag_rootKey"] != null) { ViewBag.rootKey = (string)Session["ViewBag_rootKey"]; } else { ViewBag.rootKey = ""; }
            if (Session["send"] == null) { Session["send"] = false; }
            if ((bool)Session["send"] == true) { Session["send"] = false; }

            return View(db.Servers.ToList());
        }

        void ResetInformacji()
        {
            Session["viewbag_error"] = null;
            Session["viewbag_error_pomoc"] = null;
            Session["viewbag_info"] = null;
            Session["viewbag_info_pomoc"] = null;
            Session["viewbag_success"] = null;
            Session["viewbag_success_pomoc"] = null;
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        public ActionResult IndexAccount()
        {
            UstanowLimity();
            if (Session["FullFile"] == null)
            {
                Session["FullFile"] = 0;
            }
            List<ServerFile> NameFileAccount = new List<ServerFile>();
            List<ServerFile> TempProfile;
            ServerFile profileNameFile;
            string LoginNow;
            try
            {

                if (User.Identity.IsAuthenticated)
                {
                    LoginNow = User.Identity.GetUserName();

                    TempProfile = db.Servers.ToList();

                    if (LoginNow != null)
                    {
                        for (int i = 0; i < db.Servers.Count(); i++)
                        {
                            profileNameFile = TempProfile[i];
                            if (profileNameFile != null)
                            {
                                if (profileNameFile.NalezyDo == LoginNow)
                                {
                                    NameFileAccount.Add(profileNameFile);
                                }
                            }
                        }
                        if (TempProfile == null)
                        {
                            TempProfile = new List<ServerFile>();
                        }

                        Session["LoginSave"] = NameFileAccount;
                    }
                }
                Session["LoginNameFileSave"] = (List<ServerFile>)NameFileAccount;
            }
            catch { }

            if (db.Servers.Count() == 0)
            {
                ViewBag.error = "Brak Plików uzytkownika";
                ViewBag.error_info = "";
            }
            if (Session["ViewBag_ip"] != null) { ViewBag.ip = (string)Session["ViewBag_ip"]; } else { ViewBag.ip = ""; }
            if (Session["ViewBag_port"] != null) { ViewBag.port = (string)Session["ViewBag_port"]; } else { ViewBag.port = ""; }
            if (Session["ViewBag_loginUser"] != null) { ViewBag.loginUser = (string)Session["ViewBag_loginUser"]; } else { ViewBag.loginUser = ""; }
            if (Session["ViewBag_loginKey"] != null) { ViewBag.loginKey = (string)Session["ViewBag_loginKey"]; } else { ViewBag.loginKey = ""; }
            if (Session["ViewBag_rootKey"] != null) { ViewBag.rootKey = (string)Session["ViewBag_rootKey"]; } else { ViewBag.rootKey = ""; }
            if (Session["send"] == null) { Session["send"] = false; }
            if ((bool)Session["send"] == true) { Session["send"] = false; }

            return View(((List<ServerFile>)Session["LoginNameFileSave"]).ToList());
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        public ActionResult DownloandLengthBackup(FoldersChange Logs)
        {

            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
            Session["remoteDirectory_ChangeFIleController"] = Logs.Log;
            Session["noFolders_ChangeFIleController"] = true;

            try
            {
                SftpClientFile = SftpClientConnect();
                SftpClientFile.Connect();
            }
            catch
            {
                Session["viewbag_error"] = "Nawiazanie połączenia nie powiodło się!";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                    "czy host jest dostępny oraz czy dane logowania są poprawne.";
                return RedirectToAction("About");
            }

            string folderName = Server.MapPath("~/PlikiRar");
            string pathString = System.IO.Path.Combine(folderName, User.Identity.GetUserName());
            System.IO.Directory.CreateDirectory(pathString);

            string fileName = Path.GetFileName(Logs.Log);
            string path = Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName());
            string fileDirectory = "";
            int i;
            for (i = ((string)Session["remoteDirectory_ChangeFIleController"]).Length; i > 0; --i)
            {
                if (((string)Session["remoteDirectory_ChangeFIleController"])[i - 1] == '/')
                {
                    break;
                }
            }
            fileDirectory = ((string)Session["remoteDirectory_ChangeFIleController"]).Substring(0, i - 1);
            fileDirectory = fileDirectory + "/" + fileName;
            List<Renci.SshNet.Sftp.SftpFile> lista_ = SftpClientFile.ListDirectory(fileDirectory).ToList();

            for (int j = 1; j < lista_.Count(); j++)
            {
                if (lista_[j].Name[0] != '.')
                {
                    using (var file = System.IO.File.OpenWrite(path + "/" + lista_[j].Name))
                    {
                        SftpClientFile.ChangeDirectory(fileDirectory);
                        SftpClientFile.DownloadFile(lista_[j].Name, file);
                    }
                }
            }


            if (System.IO.File.Exists(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + fileName + ".zip")))
            {
                System.IO.File.Delete(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + fileName + ".zip"));
            }

            ZipArchive zip = ZipFile.Open(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + fileName + ".zip"), ZipArchiveMode.Create);

            for (int j = 1; j < lista_.Count(); j++)
            {
                if (lista_[j].Name[0] != '.')
                {
                    try
                    {
                        zip.CreateEntryFromFile(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + lista_[j].Name), lista_[j].Name);
                    }
                    catch
                    {
                          Session["viewbag_info"] = "W podanym folderze jest katalog. Katalog został ominięty.";
                    }
                }
            }
            zip.Dispose();

            string contentType = MimeMapping.GetMimeMapping(fileName);
            string fullPath = Path.Combine(path, fileName);


            ServerFile tempServerFile = new ServerFile();

            i = LookingForNameServer();
            string pathPoczatkowy = Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName());
            string backupPath = Server.MapPath("~/FileUsers");
            var fileName__ = Path.GetFileName(i.ToString());
            var fileName_ = Path.GetFileName(Logs.Log);
            path = Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + fileName_ + ".zip");
            System.IO.File.Copy(Path.Combine(pathPoczatkowy, fileName_ + ".zip"), Path.Combine(backupPath, fileName__), true);
            FileInfo F = new FileInfo(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + fileName_ + ".zip"));
            string dopisek= SizeFilePText(F.Length);

            try
            {
                tempServerFile.Komentarz = fileName__;
                tempServerFile.NalezyDo = User.Identity.GetUserName().ToString();
                tempServerFile.Nazwa = fileName_ + ".zip";
                tempServerFile.Rozmiar_File = F.Length.ToString();
                tempServerFile.Rozmiar = dopisek;
                tempServerFile.DataUtworzenia = System.IO.File.GetCreationTime(path);
                tempServerFile.Typ = Path.GetExtension(path);
                db.Servers.Add(tempServerFile);
                db.SaveChanges();
            }
            catch
            {
                Session["viewbag_error"] = "Wystąpił problem z dodanie pliku do bazy danych SQL";
                Session["viewbag_error_pomoc"] = "";
            }

            System.IO.File.Delete(Path.Combine(path, ""));
            return RedirectToAction("IndexAccount");
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        public ActionResult Downloands(int? id)
        {
            string fileName = Path.GetFileName(id.ToString());
            List<ServerFile> serw = new List<ServerFile>();
            string path = Server.MapPath("~/FileUsers");
            string contentType;
            string fullPath;
            string nazwa = "";
            try
            {
                serw = db.Servers.ToList();
                for (int i = 0; i < serw.Count(); i++)
                {
                    if (serw[i].Komentarz == id.ToString())
                    {
                        nazwa = serw[i].Nazwa;
                        break;
                    }

                }
                contentType = MimeMapping.GetMimeMapping(nazwa);
                fullPath = Path.Combine(path, fileName);
            }
            catch
            {
                Session["viewbag_error"] = "Pobieranie pliku o nazwie   " + fileName + "   nie powiodło się";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                    "czy plik na hoŚcie jest dalej dostępny oraz czy dany uzytkownik posiada uprawnienia do odczytu.";
                return RedirectToAction("About");
            }

            return File(fullPath, contentType, nazwa);
        }

        [Authorize(Roles = "root,admin")]
        [HttpPost]
        public ActionResult Create(HttpPostedFileBase uplfile)
        {
            if (SprawdzWielkoscPlikow(uplfile))
            {
                ServerFile tempServerFile = new ServerFile();
                if (Session["send"] == null) { Session["send"] = false; }
                if ((bool)Session["send"] == false) { Session["send"] = true; }
                int i = LookingForNameServer();

                try
                {
                    var fileName = Path.GetFileName(i.ToString());
                    var fileName_ = Path.GetFileName(uplfile.FileName);
                    string fileNameTemps = fileName;
                    string path = Path.Combine(Server.MapPath("~/FileUsers"), fileName);
                    uplfile.SaveAs(path);
                    FileInfo F = new FileInfo(Server.MapPath("~/FileUsers/" + fileName));

                    string dopisek = SizeFilePText(F.Length);

                    try
                    {
                        tempServerFile.Komentarz = fileName;
                        tempServerFile.NalezyDo = User.Identity.GetUserName().ToString();
                        tempServerFile.Nazwa = fileName_;
                        tempServerFile.Rozmiar = dopisek;
                        tempServerFile.DataUtworzenia = System.IO.File.GetCreationTime(path);
                        tempServerFile.Typ = Path.GetExtension(path);
                        tempServerFile.Rozmiar_File = F.Length.ToString();
                        db.Servers.Add(tempServerFile);
                        db.SaveChanges();
                    }
                    catch
                    {
                        Session["viewbag_error"] = "Problem w zapisze pliku na serwerze";
                        Session["viewbag_error_pomoc"] = "";
                    }



                }
                catch { }
                Session["viewbag_info"] = "Plik został wysłany!";
                Session["viewbag_info_pomoc"] = "";
                return RedirectToAction("Index");
            }
            else
            {
                Session["viewbag_error"] = "Nie można wysłać pliku poniewaz przekroczyło by to limit dostępnej pamieci!";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać ten problrm usuń niepotrzebne dane lub skontaktuj się z administratorem o zwiększenie limitu.";
                return RedirectToAction("IndexAccount");
            }
        }
        
        private bool SprawdzWielkoscPlikow(HttpPostedFileBase uplfile)
        {
            var rozmiar = 0;
            var lista = db.Servers.ToList();
            for (int i = 0; i < lista.Count(); i++)
            {
                if (lista[i].NalezyDo == User.Identity.Name)
                {
                    rozmiar = rozmiar + int.Parse(lista[i].Rozmiar_File);
                }
            }
            try
            {
                rozmiar = rozmiar + uplfile.ContentLength;
                if (rozmiar < long.Parse((string)Session["FullSizeFile"]))
                {
                    return true;
                }
                else { return false; }
            }
            catch
            {
                Session["viewbag_error"] = "Wystąpił problem z rozmiarem plików!";
                Session["viewbag_error"] = "W razie powtarzania się tego błędu skontaktuj sie z administratorem!";
                return false;
            }
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult CreateIndex(HttpPostedFileBase uplfile)
        {
            if (SprawdzWielkoscPlikow(uplfile))
            {
                ServerFile tempServerFile = new ServerFile();
                if (Session["send"] == null) { Session["send"] = false; }
                if ((bool)Session["send"] == false) { Session["send"] = true; }
                int i = LookingForNameServer();

                try
                {
                    var fileName = Path.GetFileName(i.ToString());
                    var fileName_ = Path.GetFileName(uplfile.FileName);
                    string fileNameTemps = fileName;
                    string path = Path.Combine(Server.MapPath("~/FileUsers"), fileName);
                    uplfile.SaveAs(path);
                    FileInfo F = new FileInfo(Server.MapPath("~/FileUsers/" + fileName));
                    string typ = "";
                    for (int jj = fileName_.Length - 1; jj > 0; jj--)
                    {
                        typ = fileName_[jj] + typ;
                        if (fileName_[jj] == '.') { break; }
                    }

                    string dopisek = SizeFilePText(F.Length);

                    try
                    {
                        tempServerFile.Komentarz = fileName;
                        tempServerFile.NalezyDo = User.Identity.GetUserName().ToString();
                        tempServerFile.Nazwa = fileName_;
                        tempServerFile.Rozmiar_File = F.Length.ToString();
                        tempServerFile.Rozmiar = dopisek;
                        tempServerFile.DataUtworzenia = System.IO.File.GetCreationTime(path);
                        tempServerFile.Typ = Path.GetExtension(path);
                        db.Servers.Add(tempServerFile);
                        db.SaveChanges();

                        if((string)Session["viewbag_error"]=="Nie można wysłać pliku poniewaz przekroczyło by to limit dostępnej pamieci!")
                        {
                            Session["viewbag_error"] = null;
                        }
                    }
                    catch
                    {
                        Session["viewbag_error"] = "Problem w zapisze pliku na serwerze";
                        Session["viewbag_error_pomoc"] = "";
                    }
                }
                catch { }
                Session["viewbag_success"] = "Plik został wysłany!";
                Session["viewbag_success_pomoc"] = "";
                return RedirectToAction("IndexAccount");
            }
            else
            {
                Session["viewbag_error"] = "Nie można wysłać pliku poniewaz przekroczyło by to limit dostępnej pamieci!";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać ten problrm usuń niepotrzebne dane lub skntaktuj się z administratorem o zwiększenie limitu.";
                return RedirectToAction("IndexAccount");
            }
        }

        [Authorize(Roles = "root,admin")]
        public ActionResult Delete(int? id)
        {
            List<ServerFile> NameFileAccount = new List<ServerFile>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int Id_ = db.Servers
                        .Where(x => x.Komentarz == id.ToString())
                        .Select(x => x.Id)
                        .FirstOrDefault();

            if (db.Servers.Find(Id_) == null)
            {
                return HttpNotFound();
            }

            return View(db.Servers.Find(Id_));
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            List<ServerFile> NameFileAccount = new List<ServerFile>();

            id = db.Servers
                        .Where(x => x.Komentarz == id.ToString())
                        .Select(x => x.Id)
                        .FirstOrDefault();

            string fileName = Path.GetFileName(db.Servers.Find(id).Komentarz);
            List<ServerFile> serw = new List<ServerFile>();
            string path = Server.MapPath("~/FileUsers");
            string pathBackup = Server.MapPath("~/FileUsersDelete");
            try
            {
                System.IO.File.Copy(Path.Combine(path, db.Servers.Find(id).Komentarz), Path.Combine(pathBackup, db.Servers.Find(id).Komentarz + " " + db.Servers.Find(id).NalezyDo + " " + db.Servers.Find(id).Nazwa), true);
                System.IO.File.Delete(Path.Combine(path, db.Servers.Find(id).Komentarz));

                try
                {
                    var a = db.Servers.Find(id);
                    db.Servers.Remove(a);
                    db.SaveChanges();
                    Session["viewbag_info"] = ("Plik o nazwie  " + a.Nazwa + "   został usuniety").ToString();
                }
                catch
                {
                    Session["viewbag_error"] = "Nie udało się usunąć pliku z bazy";
                    Session["viewbag_error_pomoc"] = "";
                }
            }
            catch
            {
                Session["viewbag_error"] = "Usunięcie pliku o nazwie " + db.Servers.Find(id).Nazwa + " nie powiodło się";
                Session["viewbag_error_pomoc"] = "";
                return RedirectToAction("Index");
            }
            Session["viewbag_success"] = "Plik został usunięty!";
            Session["viewbag_success_pomoc"] = "";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        public ActionResult DeleteIndex(int? id)
        {

            List<ServerFile> NameFileAccount = new List<ServerFile>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            id = db.Servers
                        .Where(x => x.Komentarz == id.ToString())
                        .Select(x => x.Id)
                        .FirstOrDefault();

            if (db.Servers.Find(id) == null)
            {
                return HttpNotFound();
            }

            return View(db.Servers.Find(id));
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        [HttpPost, ActionName("DeleteIndex")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedIndex(int id)
        {
            List<ServerFile> NameFileAccount = new List<ServerFile>();

            id = db.Servers
                        .Where(x => x.Komentarz == id.ToString())
                        .Select(x => x.Id)
                        .FirstOrDefault();
            try
            {
                string fileName = Path.GetFileName(db.Servers.Find(id).Komentarz);
            }
            catch { }
            List<ServerFile> serw = new List<ServerFile>();
            string path = Server.MapPath("~/FileUsers");
            string pathBackup = Server.MapPath("~/FileUsersDelete");
            try
            {
                System.IO.File.Copy(Path.Combine(path, db.Servers.Find(id).Komentarz), Path.Combine(pathBackup, db.Servers.Find(id).Komentarz + " " + db.Servers.Find(id).NalezyDo + " " + db.Servers.Find(id).Nazwa), true);
          
                try
                {
                    var a = db.Servers.Find(id);
                    db.Servers.Remove(a);
                    db.SaveChanges();
                    Session["viewbag_info"] = ("Plik o nazwie  " + a.Nazwa + "  został usuniety").ToString();
                }
                catch
                {
                    Session["viewbag_error"] = "Nie udało się usunąć pliku z bazy";
                    Session["viewbag_error_pomoc"] = "...";
                }
            }
            catch
            {
                try
                {
                    Session["viewbag_error"] = "Usunięcie pliku o nazwie  " + db.Servers.Find(id).Nazwa + "  nie powiodło się";
                    Session["viewbag_error_pomoc"] = "...";
                }
                catch { }
                return RedirectToAction("Index");
            }
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

        public string DelateAddresNameFile()
        {
            int i;
            if (((string)Session["remoteDirectory_ChangeFIleController"]).Length != 0)
            {
                for (i = ((string)Session["remoteDirectory_ChangeFIleController"]).Length; i > 0; --i)
                {
                    if (((string)Session["remoteDirectory_ChangeFIleController"])[i - 1] == '/')
                    {
                        break;
                    }
                }
                return ((string)Session["remoteDirectory_ChangeFIleController"]).Substring(0, i - 1);
            }
            return (string)Session["remoteDirectory_ChangeFIleController"];
        }

        [Authorize(Roles = "root,admin,user-full,user-part-ftp-full,user-part-ftp-10-50,user-part-ftp-100-200,user-part-ftp-1000-500,user-part-ftp-10000-1000,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult CreateIndexBackup(FoldersChange Logs)
        {

            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
            Session["remoteDirectory_ChangeFIleController"] = Logs.Log;
            Session["noFolders_ChangeFIleController"] = true;

            try
            {
                SftpClientFile = SftpClientConnect();
                SftpClientFile.Connect();
            }
            catch
            {
                Session["viewbag_error"] = "Nawiazanie połączenia nie powiodło się!";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                    "czy host jest dostępny oraz czy dane logowania są poprawne.";
                return RedirectToAction("About");
            }

            string fileName = Path.GetFileName((string)Session["remoteDirectory_ChangeFIleController"]);
            string path = Server.MapPath("~/App_Data/TempFile");
            try
            {
                using (var file = System.IO.File.OpenWrite(path + "/" + fileName))
                {
                    string fileDirectory = DelateAddresNameFile();
                    SftpClientFile.ChangeDirectory(fileDirectory);
                    SftpClientFile.DownloadFile(fileName, file);
                }
            }
            catch
            {
                Session["viewbag_error"] = "Pobieranie pliku o nazwie  " + fileName + "  nie powiodło się";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                    "czy plik na hoscie jest dalej dostępny oraz czy dany uzytkownik posiada uprawnienia do odczytu.";
                return RedirectToAction("About");
            }
            string contentType = MimeMapping.GetMimeMapping(fileName);
            string fullPath = Path.Combine(path, fileName);



            ServerFile tempServerFile = new ServerFile();

            int i = LookingForNameServer();
            string path___ = Server.MapPath("~/App_Data/TempFile");
            string backupPath = Server.MapPath("~/FileUsers");

            var fileName__ = Path.GetFileName(i.ToString());
            var fileName_ = Path.GetFileName(Logs.Log);

            System.IO.File.Copy(Path.Combine(path___, fileName_), Path.Combine(backupPath, fileName__), true);
            FileInfo F = new FileInfo(Server.MapPath("~/App_Data/TempFile/" + fileName_));
            string dopisek = SizeFilePText(F.Length);

            try
            {
                tempServerFile.Komentarz = fileName__;
                tempServerFile.NalezyDo = User.Identity.GetUserName().ToString();
                tempServerFile.Nazwa = fileName_ + ".zip";
                tempServerFile.Rozmiar_File = F.Length.ToString();
                tempServerFile.Rozmiar = dopisek;
                tempServerFile.DataUtworzenia = System.IO.File.GetCreationTime(path);
                tempServerFile.Typ = Path.GetExtension(path);
                db.Servers.Add(tempServerFile);
                db.SaveChanges();
            }
            catch
            {
                Session["viewbag_error"] = "Wystąpił problem z dodanie pliku do bazy danych SQL";
                Session["viewbag_error_pomoc"] = "";
            }
            System.IO.File.Delete(Path.Combine(path___, fileName_));
            System.IO.File.Delete(Server.MapPath("~/App_Data/TempFile/" + fileName_));
            return RedirectToAction("IndexAccount");
        }
    }
}
