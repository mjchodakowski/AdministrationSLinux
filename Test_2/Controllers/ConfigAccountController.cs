using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.FtpClient;
using System.Web;
using System.Web.Mvc;
using Test_2.Models;

namespace Test_2.Controllers
{
    public class ConfigAccountController : Controller
    {
        public List<ConfigUser_personal> ConfigUser_Personals_ = new List<ConfigUser_personal>();
        public ActionResult Index()
        {
            if (Session["UserID_change"] == null) { Session["UserID_change"] = ""; }
            List<String> USER = new List<String>();
            if (ViewBag.info == "") { ViewBag.info = "Wybierz"; }
            SshClient connect = null;
            try
            {
                connect = new SshClient((string)Session["ViewBag_ip"], Int32.Parse((string)Session["ViewBag_port"]), "root", (string)Session["ViewBag_rootKey"]);
                connect.Connect();
            }
            catch
            {
                Session["ViewBag_error"] = "Błąd połączenia z serwerem zdalnym!";
                Session["ViewBag_error_info"] = "Jeśli upewniłeś się że hasło SUPERUSER jest prawidłowe problem może powodować brak uprawnień SSH root " +
                    "Aby rozwiązać Problem przejrzyj Zakładke POMOC";

                return RedirectToAction("Index", "UserMainProfile");
            }
            SshCommand FullFile = connect.RunCommand("cat /etc/passwd");
            string Renew = FullFile.Result;

            string[] Loginy = Renew.Split(new Char[] { ' ', '\n', '\t' });

            for (int i = 0; i < Loginy.Length; i++)
            {
                string[] LoginyD = Loginy[i].Split(new Char[] { ':' });
                try
                {
                    ConfigUser_personal n = new ConfigUser_personal
                    {

                        Nazwa_użytkownika = LoginyD[0],
                        Haslo = LoginyD[1],
                        UID = LoginyD[2],
                        GID = LoginyD[3],
                        Komentarz = LoginyD[4],
                        Katalog_domowy = LoginyD[5],
                        Polecenie_logowania = LoginyD[6]

                    };
                    ConfigUser_Personals_.Add(n);
                }
                catch { }

            }
            ViewBag.Renew = ConfigUser_Personals_[0].Nazwa_użytkownika;
            ViewBag.Renew_1 = ConfigUser_Personals_[1].Nazwa_użytkownika;
            ViewBag.Renew_2 = ConfigUser_Personals_[2].Nazwa_użytkownika;
            return View(ConfigUser_Personals_);
        }

        [HttpPost]
        public ActionResult EditKey(ConfigUser_personal TempChange)
        {
            return View();
        }

        public SshClient Connecting()
        {
            SshClient connect = null;
            try
            {
                connect = new SshClient((string)Session["ViewBag_ip"], Int32.Parse((string)Session["ViewBag_port"]), "root", "test");
            }
            catch
            {
                RedirectToAction("Index", "UserMainProfile");
            }
            try { connect.Connect(); } catch {  RedirectToAction("Logon", "Home"); }
            return connect;
        }
        public SftpClient SftpClientConnect()
        {
            string login, password;
            SftpClient SftpClientFile = (SftpClient)Session["SftpClient"];

                    login = "root";
                    password = (string)Session["ViewBag_rootKey"];

                try
                {
                    SftpClientFile = new SftpClient((string)Session["ViewBag_ip"], Int32.Parse((string)Session["ViewBag_port"]), login, password);
                }
                catch
                {
                    Session["viewbag_error"] = "Aby utworzyć połączenie musisz wybrać profil dostępowy!";
                    Session["viewbag_error_pomoc"] = "Odtwórz menu a następnie 'Strona Główna Profilu'";
                return null;
                }

                return SftpClientFile;
          
        }

        [HttpPost]
        public ActionResult Uid(ConfigUser_personal TempChange)
        {
             var ls = Connecting();
             ls.RunCommand("usermod -u " + TempChange.UID + " " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Gid(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("usermod -g " + TempChange.GID + " " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Komentarz(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("usermod -c "+ "'"+TempChange.Komentarz+"'" + " " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Katalog_domowy(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("usermod -d " + TempChange.Katalog_domowy + " " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Polecenie_logowania(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("usermod -u " + TempChange.UID + " " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangeName(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("usermod -l " + TempChange.Polecenie_logowania + " " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Block(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            var azxc=ls.RunCommand("usermod -L " + TempChange.Nazwa_użytkownika);            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnBlock(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("usermod -U " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Edit(ConfigUser_personal TempChange)
        {
            Session["UserID_change"] = TempChange.Nazwa_użytkownika;
            return RedirectToAction("Index");
        }

 
       [HttpPost]
        public ActionResult Create(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            if (TempChange.Nazwa_użytkownika != null)
            {
                if (TempChange.Nazwa_użytkownika != "")
                {
                    ls.RunCommand("useradd -m " + TempChange.Nazwa_użytkownika);
                }
            }
            else
            {
                Session["ViewBag_error"] = "Proszę podać nazwę do tworzonego konta użytkownika";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(ConfigUser_personal TempChange)
        {
            var ls = Connecting();
            ls.RunCommand("userdel " + TempChange.Nazwa_użytkownika);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult ChangePassword(ConfigUser_personal TempChange)
        {

            if (TempChange.Haslo == TempChange.Polecenie_logowania)
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

                string nazwaDocelowa = Path.GetFileName("skrypt");
                string adressDocelowy = "/";

                using (FileStream fs = new FileStream(Server.MapPath("~/App_Data/File/skrypt"), FileMode.Open))
                {
                    SftpClientFile.ChangeDirectory(adressDocelowy);
                    SftpClientFile.BufferSize = 4 * 1024;
                    SftpClientFile.UploadFile(fs, nazwaDocelowa);


                }
                var ls = Connecting();
                var wynik = ls.RunCommand("cd / && chmod 755 skrypt && ./skrypt " + TempChange.Nazwa_użytkownika + " " + TempChange.Haslo).Execute();



                if (wynik.IndexOf("password updated successfully") == -1 && wynik.IndexOf("hasło zostało zmienione") == -1)
                {
                    Session["viewbag_error"] = "Wystąpił bląd przy zmianie hasła! Hasło nie zostało zmienione";
                    Session["viewbag_error_pomoc"] = "Aby rozwiązać problem sprawdz czy w systemie zainstalowany jest pakiet 'EXPECT' jesli nie zainstaluj go klikając w link w dolnej częsci strony!"
                        + "Wynik błędu dla administratora: " + wynik;
                }
                else
                {
                    Session["viewbag_success"] = "Hasło użytkownika " + TempChange.Nazwa_użytkownika + " zostało zmienione poprawnie ";
                }
            }
            else
            {
                Session["viewbag_info"] = "Hasła użytkownika " + TempChange.Nazwa_użytkownika + " muszą być identyczne.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult Expect()
        {
            string wynik= null;
            var ls = Connecting();
            try
            {
                wynik = ls.RunCommand("apt-get install expect").Execute();
                Session["viewbag_info"] = "Pakiet EXPECT - został zainstalowany";

            }
            catch { 
                Session["viewbag_error"] = "Wystąpił bląd przy instalowaniu pakietu skontaktuj się z administratorem";
                Session["viewbag_info_pomoc"] = "Wynik błędu dla administratora: " + wynik;
            }

            return RedirectToAction("Index");
        }
    }
}