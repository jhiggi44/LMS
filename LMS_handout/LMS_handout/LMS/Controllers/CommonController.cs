﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
  public class CommonController : Controller
  {

        /*******Begin code to modify********/

        protected Team12LMSContext db;

        public CommonController() {
            db = new Team12LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        public void UseLMSContext(Team12LMSContext ctx) {
            db = ctx;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments() {
            var query = from d in db.Departments

            select new { name = d.Name, subject = d.Subject};
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog() { 
            List<object> catalog = new List<object>(); 

            var query =
                from dep in db.Departments
                select new { dname = dep.Name, subject = dep.Subject };

            foreach (var result in query) {
                var query2 =
                        from course in db.Courses
                        where course.Department == result.subject
                        select new { number = course.Number, cname = course.Name };

                catalog.Add(new {
                    result.subject,
                    result.dname,
                    courses = query2.ToArray()
                });

            }
            return Json(catalog.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number) {

            var query =
                from crs in db.Courses
                where crs.Department == subject
                && crs.Number == number
                join clss in db.Classes
                on crs.CatalogId equals clss.CatalogId into join1

                from j1 in join1
                join profs in db.Professors
                on j1.Professor equals profs.UId
                select new {
                    season = j1.Semester.Substring(0, j1.Semester.Length - 5),
                    year = j1.Semester.Substring(j1.Semester.Length - 4, 4),
                    location = j1.Location,
                    start = j1.StartTime,
                    end = j1.EndTime,
                    fname = profs.FirstName,
                    lname = profs.LastName
                };

            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname) {
            string semester = season + " " + year;

            var query =
                from courses in db.Courses
                where courses.Department == subject
                && courses.Number == num
                join clss in db.Classes
                on courses.CatalogId equals clss.CatalogId into offerings

                from offs in offerings
                where offs.Semester == semester
                join cat in db.AssignmentCategories
                on offs.ClassId equals cat.ClassId into clssCats

                from clscat in clssCats
                where clscat.Name == category
                join assigns in db.Assignments
                on clscat.CategoryId equals assigns.Category into all

                from a in all
                where a.Name == asgname
                select new { contents = a.Contents };

            if (query.Any()) {
                return Content(query.First().contents);
            }

            return Content("");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid) {
            string semester = season + " " + year;

            var query =
                from courses in db.Courses
                where courses.Department == subject
                && courses.Number == num
                join clss in db.Classes
                on courses.CatalogId equals clss.CatalogId into offerings

                from offs in offerings
                where offs.Semester == semester
                join cat in db.AssignmentCategories
                on offs.ClassId equals cat.ClassId into clssCats

                from clscat in clssCats
                where clscat.Name == category
                join assigns in db.Assignments
                on clscat.CategoryId equals assigns.Category into assignments

                from assigns2 in assignments
                where assigns2.Name == asgname
                join sub in db.Submission
                on assigns2.AId equals sub.AId into submits

                from sub2 in submits
                where sub2.UId == uid
                select new { contents = sub2.Contents };

            if (query.Any()) {
                return Content(query.First().contents);
            }

            return Content("");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid) {
            // u3804815
            var queryStudents =
                from stud in db.Students
                where stud.UId == uid
                select new { fname = stud.FirstName, lname = stud.LastName, uid = stud.UId, department = stud.Major };
            if (queryStudents.Any()) {

                return Json(queryStudents.First());
            }

            var queryProfs =
                from prof in db.Professors
                where prof.UId == uid
                select new { fname = prof.FirstName, lname = prof.LastName, uid = prof.UId, department = prof.Department };
            if (queryProfs.Any()) {
                return Json(queryProfs.First());
            }


            var queryAdmin =
                from admin in db.Administrators
                where admin.UId == uid
                select new { fname = admin.FirstName, lname = admin.LastName, uid = admin.UId };
            if (queryAdmin.Any()) {
                return Json(queryAdmin.First());
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}