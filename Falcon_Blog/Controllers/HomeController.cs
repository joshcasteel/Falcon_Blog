using Falcon_Blog.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using System.Web.UI.WebControls.Expressions;
using Falcon_Blog.Helpers;
using Microsoft.AspNet.Identity;

namespace Falcon_Blog.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SearchHelper searchHelper = new SearchHelper();
    
        public ActionResult Index(int? page, string searchStr)
        {
            ViewBag.Search = searchStr;
            var blogList = searchHelper.IndexSearch(searchStr);
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(blogList.Where(b => b.IsPublished).OrderByDescending(b => b.Created).ToPagedList(pageNumber, pageSize));
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            EmailModel model = new EmailModel();
             return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model, string Body)
        {
            ViewBag.Success = "<div class=\"alert alert-success\" role=\"alert\">Message sent! Thanks!</div>";
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
                    var from = "Josh's-Blog<example@email.com>";
                    model.Body = Body;

                    var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = "Portfolio Contact Email",
                        Body = string.Format(body, model.FromName, model.FromEmail, model.Body), IsBodyHtml = true 
                    };
                    
                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);

                    
                    return View(new EmailModel());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        await Task.FromResult(0);
                    }
            }
            
            return View(model);
        }

        public PartialViewResult _LoginPartial()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            
            return PartialView(user);
        }
    }
}