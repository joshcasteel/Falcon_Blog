using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Falcon_Blog.Helpers;
using Falcon_Blog.Models;
using PagedList;
using PagedList.Mvc;


namespace Falcon_Blog.Controllers
{
    [RequireHttps]
    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private SearchHelper searchHelper = new SearchHelper();
        // GET: BlogPosts
        public ActionResult Index(int? page, string searchStr)
        {
            ViewBag.Search = searchStr;
            var blogList = searchHelper.IndexSearch(searchStr);
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            //var listPosts = db.BlogPosts.AsQueryable();
            //var blogPosts = db.BlogPosts.OrderByDescending(b => b.Created).ToList();
            
            return View(blogList.ToPagedList(pageNumber, pageSize));
        }

        
        // GET: BlogPosts/Details/5
        public ActionResult Details(string slug)
        {
            if(String.IsNullOrWhiteSpace(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.FirstOrDefault(b => b.Slug == slug);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            Random rand = new Random();
            int rands = rand.Next(1, 4);
            ViewBag.Rand = rands;
            
            return View(blogPost);
            
        }

        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,,Abstract,Body,MediaUrl,IsPublished")] BlogPost blogPost, HttpPostedFileBase image)
        {   
            if (ModelState.IsValid)
            {
                //convert title to URLFriendly string
                var slug = StringUtilities.URLFriendly(blogPost.Title);

                //check if slug has data
                if(String.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError("Title", "Please enter a title");
                    return View(blogPost);
                }
                //check for duplicates
                if (db.BlogPosts.Any(b => b.Slug == slug))
                {
                    ModelState.AddModelError("Title", $"The title: '{blogPost.Title}' has been used before");
                    return View(blogPost);
                }
                blogPost.Slug = slug;
                blogPost.Created = DateTime.Now;

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    //Isolate filename from extention
                    var imgFileName = Path.GetFileNameWithoutExtension(image.FileName);
                    //run filename through slugger to remove any spaces or unwanted characters
                    imgFileName = StringUtilities.URLFriendly(imgFileName);
                    //adding timecode to filename
                    imgFileName = $"{imgFileName}-{DateTime.Now.Ticks}";
                    //add back extention
                    imgFileName = $"{imgFileName}{Path.GetExtension(image.FileName)}";

                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), imgFileName));
                    blogPost.MediaUrl = "/Uploads/" + imgFileName;
                }

                db.BlogPosts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string slug)
        {
            
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.FirstOrDefault(b => b.Slug == slug);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);

            
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Slug,Abstract,Body,MediaUrl,IsPublished")] BlogPost blogPost, HttpPostedFileBase image)
        {
            
            

            if (ModelState.IsValid)
            {
                var slug = StringUtilities.URLFriendly(blogPost.Title);
                if (blogPost.Slug != slug)
                {
                    if (String.IsNullOrEmpty(slug))
                    {
                        ModelState.AddModelError("Title", "Please enter a title");
                        return View(blogPost);
                    }
                
                    if (String.IsNullOrEmpty(blogPost.Title))
                    {
                        ModelState.AddModelError("Title", "Please enter a title");
                        return View(blogPost);
                    }

                    if (db.BlogPosts.Any(b => b.Slug == slug))
                    {
                        ModelState.AddModelError("Title", "Duplicate Record Found");
                        return View(blogPost);
                    }
                    blogPost.Slug = slug;
                }
                
                blogPost.Updated = DateTime.Now;
                var bpId = blogPost.Id;
                var oldPost = db.BlogPosts.AsNoTracking().FirstOrDefault(b => b.Id == bpId);
                blogPost.Created = oldPost.Created;

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    //Isolate filename from extention
                    var imgFileName = Path.GetFileNameWithoutExtension(image.FileName);
                    //run filename through slugger to remove any spaces or unwanted characters
                    imgFileName = StringUtilities.URLFriendly(imgFileName);
                    //adding timecode to filename
                    imgFileName = $"{imgFileName}-{DateTime.Now.Ticks}";
                    //add back extention
                    imgFileName = $"{imgFileName}{Path.GetExtension(image.FileName)}";
                    
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), imgFileName));
                    blogPost.MediaUrl = "/Uploads/" + imgFileName;
                }

                db.Entry(blogPost).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");


            }
            

            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string slug)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.BlogPosts.FirstOrDefault(b => b.Slug == slug);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string slug)
        {
            BlogPost blogPost = db.BlogPosts.FirstOrDefault(b => b.Slug == slug);
            db.BlogPosts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
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
