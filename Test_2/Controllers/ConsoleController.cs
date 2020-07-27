using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Test_2.Models;

namespace Test_2.Controllers
{
    public class ConsoleController : Controller
    {
        SshClient nowy;


        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        public ActionResult Index()
        {
            if (Session["ViewBag_ip"] == null)
            {
                Session["viewbag_error"] = "Aby utworzyć połączenie z hostem musi byc wybrany profil połączenia";
                Session["viewbag_error_pomoc"] = "Aby rozwiazać ten problem proszę wybrać profil polączenia!";
            }

            if (Session["isConnect"]==null)
            {
                Session["isConnect"] = false;
            }
            return View();
        }

        [Authorize(Roles = "root,admin,user-full,user-part-server,user-full-10-50,user-full-100-200")]
        [HttpPost]
        public ActionResult Index(Consoles o)
        {
            if (Session["ViewBag_ip"] == null)
            {
                Session["viewbag_error"] = "Aby utworzyć połączenie z hostem musi byc wybrany profil połączenia";
                Session["viewbag_error_pomoc"] = "Aby rozwiazać ten problem proszę wybrać profil polączenia!";
            }
            else
            {
                try
                {

                    if (nowy == null)
                    {
                        nowy = new SshClient((string)Session["ViewBag_ip"], (string)Session["ViewBag_loginUser"], (string)Session["ViewBag_loginKey"]);  
                        if (!nowy.IsConnected)
                        {
                            nowy.Connect();
                            if (!(bool)Session["isConnect"])
                            {
                                Session["viewbag_success"] = "Ustanowiono połączenie z hostem!";
                                Session["isConnect"] = true;
                            }
                        }
                    }

                    if (nowy.IsConnected)
                    {
                        List<string> komendy;
                                if (Session["tablica_komend"] == null)
                                {
                                   komendy = new List<string>();
                                } else
                                {
                                    komendy = (List<string>)Session["tablica_komend"];
                                }
                            try
                            {
                            if (Session["temps"] == null) { Session["temps"] = ""; }
                                Session["temps"] = nowy.RunCommand(o.In_).Execute();
                            
                            if ((string)Session["temps"].ToString() != "" && (string)Session["temps"].ToString()!=null) { 
                                komendy.Add((string)Session["temps"].ToString());
                                Session["tablica_komend"] = komendy;
                            } else {
                                    Session["viewbag_error"] = "Polecenie nie może być puste";
                            }

                            }
                            catch
                            {
                            Session["viewbag_error"] = "Błąd przetwarzania danych!";
                            }
                    }
                }
                catch
                {
                    Session["viewbag_error"] = "Nie można ustanowić połączenia! Sprawdz czy dane logowania są poprawne";
                }
            }
            return View();
        }
    }
}
