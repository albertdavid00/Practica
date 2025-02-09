﻿using Microsoft.AspNet.Identity;
using PracticaV1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using File = PracticaV1.Models.File;

namespace PracticaV1.Controllers
{
    public class FilesController : Controller
    {
        private ApplicationDbContext db = new PracticaV1.Models.ApplicationDbContext();
        // GET: Files

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Professor")]
        public ActionResult Upload(HttpPostedFileBase uploadedFile, File upFile)
        {
          
                string uploadedFileName = uploadedFile.FileName;
                string uploadedFileExtension = Path.GetExtension(uploadedFileName);

                if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpg" || uploadedFileExtension == ".pdf")
                {

                    string uploadFolderPath = Server.MapPath("~//UploadedFiles//");

                    uploadedFile.SaveAs(uploadFolderPath + uploadedFileName);
                    File file = new File();
                    file.Extension = uploadedFileExtension;
                    file.FileName = uploadedFileName;
                    file.FilePath = uploadFolderPath + uploadedFileName;
                    file.UploadTime = DateTime.Now;
                    file.SectionId = upFile.SectionId;
                    // 4. Se adauga modelul in baza de date
                    db.Files.Add(file);
                    db.SaveChanges();

                    var sectionId = file.SectionId;
                    var section = db.Sections.Find(sectionId);
                    var courseId = section.CourseId;

                    return Redirect("/Courses/Show/" + courseId.ToString());

                }
          

            // TODO: tratarea erorilor
            return View();
        }

        [Authorize(Roles = "Admin,Professor,Student")]
        public FileResult DownloadFile(int id)
        {
            string filePath = Server.MapPath("~//UploadedFiles//");
            File myFile = db.Files.Find(id);
            string fullPath = Path.Combine(filePath, myFile.FileName);
            string extension = myFile.Extension;
            extension = extension.Substring(1);
            string MIMEtype;
            if (extension == "pdf")
            {
                MIMEtype = "application/";
            }
            else
            {
                MIMEtype = "image";
            }
            return File(fullPath, MIMEtype + extension, myFile.FileName);
        }

        [Authorize(Roles ="Admin,Professor")]
        public ActionResult Delete(int id)
        {
            File file = db.Files.Find(id);
            var creatorId = file.Section.Course.Professor.UserId;
            var courseId = file.Section.CourseId;
            if (User.Identity.GetUserId() == creatorId|| User.IsInRole("Admin"))
            {
                db.Files.Remove(file);
                db.SaveChanges();
                return Redirect("/Courses/Show/" + courseId.ToString());
            }
            return Redirect("/Courses/Show/" + courseId.ToString());
        }
    }
}