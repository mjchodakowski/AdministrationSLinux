using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Test_2.Models;
using System.IO;
using Renci.SshNet.Common;
using Microsoft.AspNet.Identity;
using System.IO.Compression;
using Test_2.DAL;

namespace Test_2.Controllers
{
    public class ChangeFileController : Controller
    {

        private serverFileBD db = new serverFileBD();
        public SftpClient SftpClientConnect()
        {
            string login, password;
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
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
                    Session["viewbag_error"] = "Aby utworzyć połączenie musisz wybrać profil dostępowy!";
                    Session["viewbag_error_pomoc"] = "Przejdz do menu a następnie 'Strona Główna Profilu'";
                }

                return SftpClientFile;
            }
            else
            {
                return null;
            }
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

        public ActionResult Downloands(FoldersChange Logs)
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
                Session["viewbag_error"] = "Pobieranie pliku o nazwie " + fileName + " nie powiodło się";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                    "czy plik na hoście jest dalej dostępny oraz czy dany uzytkownik posiada uprawnienia do odczytu.";
                return RedirectToAction("About");
            }
            string contentType = MimeMapping.GetMimeMapping(fileName);
            string fullPath = Path.Combine(path, fileName);
            return File(fullPath, contentType, fileName);
        }
        
        public ActionResult DownloandLength(FoldersChange Logs)
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
            List<Renci.SshNet.Sftp.SftpFile> lista = ((List<Renci.SshNet.Sftp.SftpFile>)Session["filelist_ChangeFIleController"]);
            List<Renci.SshNet.Sftp.SftpFile> lista_ = SftpClientFile.ListDirectory(fileDirectory).ToList();

            for (int j = 1; j < lista_.Count(); j++)
            {
                if (lista_[j].Name[0] != '.')
                {
                    using (var file = System.IO.File.OpenWrite(path + "/" + lista_[j].Name))
                    {
                        SftpClientFile.ChangeDirectory(fileDirectory);
                        try
                        {
                            SftpClientFile.DownloadFile(lista_[j].Name, file);
                        }
                        catch
                        {
                            return RedirectToRoute("Index", "Home");
                        }
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
                try
                {
                    if (lista_[j].Name[0] != '.')
                    {

                        zip.CreateEntryFromFile(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + lista_[j].Name), lista_[j].Name);

                    }
                } catch
                {
                    Session["viewBag_info"] = "W podanym folderze jest katalog. ";
                    Session["viewBag_info_pomoc"] = "Katalog został ominięty.";
                }
            }
            zip.Dispose();
            
            return File(Server.MapPath("~/PlikiRar/" + User.Identity.GetUserName() + "/" + fileName + ".zip"), "application/zip", fileName+".zip");
            
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult About()
        {
            if (Session["resetInfoBar"] == null) { Session["resetInfoBar"] = false; }
            if (Session["WhatLogon"] == null) { Session["WhatLogon"] = 0; }
            if (Session["WybranyProfil"] == null)
            {
                return RedirectToAction("Index", "UserMainProfile");
            }

            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];

            if ((int)(Session["WhatLogon"]) != 0)
            {

                if ((string)Session["remoteDirectory_ChangeFIleController"] == null)
                {
                    Session["remoteDirectory_ChangeFIleController"] = "";
                }

                SftpClientFile = SftpClientConnect();
                try { SftpClientFile.Connect(); }
                catch
                {
                    Session["viewbag_error"] = "Nawiazanie połączenia nie powiodło się!";
                    Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                        "czy host jest dostępny oraz czy dane logowania są poprawne.";
                    Session["errorLogon"] = true;
                    return RedirectToAction("Index", "UserMainProfile");
                }

                Session["WhatOperation"] = WhatOperation_((string)Session["remoteDirectory_ChangeFIleController"]);

                if ((bool)Session["WhatOperation"])
                {
                    int t = 0;
                    int i;
                    for (i = (((string)Session["remoteDirectory_ChangeFIleController"]).Length) - 1; i > 0; i--)
                    {
                        if ('/' != ((string)Session["remoteDirectory_ChangeFIleController"])[i]) { t++; } else { i = -1; }
                    }

                    for (int j = 0; j < (((string)Session["remoteDirectory_ChangeFIleController"]).Length) - t; j++)
                    {
                        Session["temp_ChangeFileController"] = (string)Session["temp_ChangeFileController"] + ((string)Session["remoteDirectory_ChangeFIleController"])[j];
                    }
                    if (Session["temp_ChangeFileController"] == null) { Session["temp_ChangeFileController"] = ""; }
                    var ala = (string)Session["temp_ChangeFileController"];
                    try
                    {
                        Session["filelist_ChangeFIleController"] = SftpClientFile.ListDirectory((string)Session["temp_ChangeFileController"]).ToList();
                    }
                    catch
                    {
                        Session["temp_ChangeFileController"] = "";
                        Session["filelist_ChangeFIleController"] = SftpClientFile.ListDirectory((string)Session["temp_ChangeFileController"]).ToList();

                    }
                }
                else
                {
                    try
                    {
                        Session["filelist_ChangeFIleController"] = SftpClientFile.ListDirectory((string)Session["remoteDirectory_ChangeFIleController"]).ToList();
                    }
                    catch
                    {
                        Session["viewbag_error"] = "Wystąpił błąd tworzenia listy plików w ekploratorze plików!";
                        Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                            "czy uprawnienia odczytu  plików i folderów są dostępne!";
                        return RedirectToAction("About");
                    }
                }
            }

            if (Session["viewbag_error"] != null) { ViewBag.error = Session["viewbag_error"]; ViewBag.error_pomoc = Session["viewbag_error_pomoc"]; }
            if (Session["viewbag_success"] != null) { ViewBag.success = Session["viewbag_success"]; ViewBag.success_pomoc = Session["viewbag_success_pomoc"]; }
            if (Session["viewbag_info"] != null) { ViewBag.info = Session["viewbag_info"]; ViewBag.info_pomoc = Session["viewbag_info_pomoc"]; }
            if (Session["ViewBag_ip"] != null) { ViewBag.ip = (string)Session["ViewBag_ip"]; } else { ViewBag.ip = ""; }
            if (Session["ViewBag_port"] != null) { ViewBag.port = (string)Session["ViewBag_port"]; } else { ViewBag.port = ""; }
            if (Session["ViewBag_loginUser"] != null) { ViewBag.loginUser = (string)Session["ViewBag_loginUser"]; } else { ViewBag.loginUser = ""; }
            if (Session["ViewBag_loginKey"] != null) { ViewBag.loginKey = (string)Session["ViewBag_loginKey"]; } else { ViewBag.loginKey = ""; }
            if (Session["ViewBag_rootKey"] != null) { ViewBag.rootKey = (string)Session["ViewBag_rootKey"]; } else { ViewBag.rootKey = ""; }

            return View((List<Renci.SshNet.Sftp.SftpFile>)Session["filelist_ChangeFIleController"]);
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult About(FoldersChange Logs)
        {
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
            Session["remoteDirectory_ChangeFIleController"] = Logs.Log;
            Session["noFolders_ChangeFIleController"] = true;
            return RedirectToAction("About");
        }
        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]

        [HttpPost]
        public ActionResult Undo(FoldersChange Logs)
        {
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
            char ifs;
            bool temp_2 = true;
            int dlugoscW = ((string)Session["remoteDirectory_ChangeFIleController"]).Length;
            try
            {
                var a = Session["noFolders_ChangeFIleController"];
                if (!(bool)Session["noFolders_ChangeFIleController"])
                {
                    Session["viewbag_info"] = "Nie można cofnąc - jesteś w katalogu głównym";
                    Session["viewbag_info_pomoc"] = "";
                    return RedirectToAction("About");
                }
                else
                {
                    for (int i = 0; i < dlugoscW && temp_2; i++)
                    {
                        ifs = ((string)Session["remoteDirectory_ChangeFIleController"])[((string)Session["remoteDirectory_ChangeFIleController"]).Length - 1];
                        if (ifs.ToString() != "/")
                        {
                            Session["remoteDirectory_ChangeFIleController"] = ((string)Session["remoteDirectory_ChangeFIleController"]).Remove(((string)Session["remoteDirectory_ChangeFIleController"]).Length - 1);
                        }
                        else
                        {
                            Session["remoteDirectory_ChangeFIleController"] = ((string)Session["remoteDirectory_ChangeFIleController"]).Remove(((string)Session["remoteDirectory_ChangeFIleController"]).Length - 1);
                            temp_2 = false;
                        }

                    }

                }

            }
            catch
            {
                Session["viewbag_info"] = "Nie można cofnąc - jesteś w katalogu głównym";
                Session["viewbag_info_pomoc"] = "";
                return RedirectToAction("About");
            }
            return RedirectToAction("About");
        }

        public bool WhatOperation_(string WhatFiles)
        {
            if (WhatFiles != "")
            {
                for (int i = 1; i < 10; i++)
                {
                    try
                    {
                        if (WhatFiles[(WhatFiles.Length) - i].ToString() == ".")
                        {
                            return true;
                        }
                    }
                    catch { }
                }
            }
            return false;
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult Sending(HttpPostedFileBase uplfile)
        {

            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
            try
            {
                SftpClientFile = SftpClientConnect();
                SftpClientFile.Connect();
            }
            catch
            {
                Session["viewbag_error"] = "Nawiązanie połączenia nie powiodło się!";
                Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                    "czy host jest dostępny oraz czy dane logowania są poprawne.";
                return RedirectToAction("About");
            }

            try
            {
                var fileName = Path.GetFileName(uplfile.FileName);

                string path = Path.Combine(Server.MapPath("~/file_"), fileName);
                uplfile.SaveAs(path);
                ViewBag.FileName = uplfile.FileName;
                ViewBag.path = path;
                string nazwaDocelowa = Path.GetFileName(uplfile.FileName);
                string adressDocelowy = (string)Session["remoteDirectory_ChangeFIleController"];

                try
                {
                    using (FileStream fs = new FileStream(Server.MapPath("~/file_/" + fileName), FileMode.Open))
                    {
                        SftpClientFile.ChangeDirectory(adressDocelowy);
                        SftpClientFile.BufferSize = 4 * 1024;
                        SftpClientFile.UploadFile(fs, nazwaDocelowa);

                        Session["viewbag_info"] = "Plik o nazwie " + nazwaDocelowa + " został wysłany!";
                    }
                }
                catch
                {
                    Session["viewbag_error"] = "Wysłanie pliku nie powiodło się!";
                    Session["viewbag_error_pomoc"] = "Aby rozwiązać problem proszę sprawdzić  " +
                        "czy host ma uprawnienia dostepu do wysyłania plików w aktualnej lokalizacji.";
                }
            }
            catch
            {
                Session["viewbag_info"] = "Plik do wysłania nie został wyznaczony";
                Session["viewbag_info_pomoc"] = "Aby wysłać plik należy wybrać go z menu kontekstowego";
            }
            return RedirectToAction("About");
        }


        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult ZmienNazwe(FoldersChange foldersChange_)
        {
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
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

            // string adressDocelowy = DelateAddresNameFile();
            string adressDocelowy = (string)Session["remoteDirectory_ChangeFIleController"];
            
            try
            {
                SftpClientFile.ChangeDirectory(adressDocelowy);
                SftpClientFile.RenameFile(foldersChange_.Del, foldersChange_.Del_new);
                Session["viewbag_success"] = "Nazwa pliku została zmieniona" + foldersChange_.Del;
                Session["viewbag_success_pomoc"] = "";
            }
            catch
            {
                Session["viewbag_error"] = "Blad zmiany nazwy- to nie jest katalog lub katalog nie jest pusty";
                Session["viewbag_error_pomoc"] = "";
            }
            return RedirectToAction("About");
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult CreateFolder(FoldersChange foldersChange_)
        {
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
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
            string adressDocelowy = (string)Session["remoteDirectory_ChangeFIleController"];
            SftpClientFile.ChangeDirectory(adressDocelowy);
            try
            {
                SftpClientFile.CreateDirectory(foldersChange_.nameNewFolder);
                Session["viewbag_success"] = "Folder o nazwie " + foldersChange_.nameNewFolder + " zostal utworzony.";
                Session["viewbag_success_pomoc"] = "";
            }
            catch
            {
                Session["viewbag_error"] = "Błąd tworzenia folderu  o nazwie" + foldersChange_.nameNewFolder;
                Session["viewbag_error_pomoc"] = "";
                return RedirectToAction("About");
            }
            Session["viewbag_success"] = "Folder o nazwie " + foldersChange_.nameNewFolder + " zostal utworzony.";
            return RedirectToAction("About");
        }


        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult Del(FoldersChange foldersChange_)
        {
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];
            string adressDocelowy;
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

            if (WhatOperation_(foldersChange_.Del))
            {
                int i;
                for (i = ((string)Session["remoteDirectory_ChangeFIleController"]).Length; i > 0; --i)
                {
                    if (((string)Session["remoteDirectory_ChangeFIleController"])[i - 1] == '/')
                    {
                        break;
                    }
                }
                string docelowykatalog = ((string)Session["remoteDirectory_ChangeFIleController"]).Substring(0, i);
                SftpClientFile.ChangeDirectory(docelowykatalog);
                try
                {
                    SftpClientFile.DeleteFile(foldersChange_.Del);
                }
                catch
                {
                    Session["viewbag_error"] = "Blad usuwania - to nie jest katalog lub katalog nie jest pusty";
                    return RedirectToAction("About");
                }
                Session["viewbag_success"] = "Plik o nazwie " + foldersChange_.Del + " zostal usuniety.";
            }
            else
            {
                adressDocelowy = (string)Session["remoteDirectory_ChangeFIleController"];
                SftpClientFile.ChangeDirectory(adressDocelowy);
                try
                {
                    SftpClientFile.DeleteDirectory(foldersChange_.Del);
                }
                catch
                {
                    Session["viewbag_error"] = "Blad usuwania - plik chroniony ";
                    return RedirectToAction("About");
                }
                Session["viewbag_success"] = "Folder o nazwie " + foldersChange_.Del + " zostal usuniety.";
            }

            return RedirectToAction("About");
        }


        [HttpGet]
        public ActionResult RootConnect()
        {
            Session["WhatLogon"] = 2;
            return RedirectToAction("About");
        }

        [HttpGet]
        public ActionResult UserConnect()
        {
            Session["WhatLogon"] = 1;
            return RedirectToAction("About");
        }

        public ActionResult Disconnect()
        {
            Session["WhatLogon"] = 0;
            return RedirectToAction("About");
        }

    }
}