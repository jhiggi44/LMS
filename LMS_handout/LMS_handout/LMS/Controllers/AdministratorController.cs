using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers {
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : CommonController {
        public IActionResult Index() {
            return View();
        }

        public IActionResult Department(string subject) {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject) {

            var query =
                from c in db.Courses
                where subject == c.Department

                select new { number = c.Number, name = c.Name };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject) {
            var query =
                from d in db.Departments
                where d.Subject == subject
                join p in db.Professors
                on d.Subject equals p.Department
                select new { lname = p.LastName, fname = p.FirstName, uid = p.UId };

            return Json(query.ToArray());
        
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name) {

            var query = from c in db.Courses
                        where c.Department == subject && c.Number == number
                        select c;

            if (!query.Any()) {
                Courses course = new Courses { Name = name, Number = (ushort)number, Department = subject };

                db.Courses.Add(course);

                try {
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return Json(new { success = false });
                }
            }

            return Json(new { success = false });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor) {

            var courseQuery =
                from c in db.Courses
                where c.Department == subject &&
                c.Number == number
                select c;

            string subjecAbrevv = "";
            string catalogID = "";
            foreach (var t in courseQuery) {
                catalogID = t.CatalogId;
                subjecAbrevv = t.Department;
            }

            if (courseQuery.Any()) {
                //query 
                var classesQuery =
                    from c in db.Classes
                    where c.Location == location &&
                    c.StartTime.ToString() == start.ToString() && c.EndTime.ToString() == end.ToString()
                    && c.Semester == season
                    select c;

                var coursesQuery =
                    from crs in db.Classes
                    where catalogID == crs.CatalogId &&
                    crs.Semester == season
                    select crs;

                if (!classesQuery.Any() || !courseQuery.Any()) {
                    //create new Class
                    Classes cl = new Classes { Semester = season + " " + year, CatalogId = catalogID, StartTime = start, EndTime = end, Location = location, Professor = instructor };

                    db.Classes.Add(cl);

                    try {
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                        return Json(new { success = false });
                    }

                }

            }
            return Json(new { success = false });
        }
        /*******End code to modify********/

    }
}

    
   
