using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
  [Authorize(Roles = "Student")]
  public class StudentController : CommonController
  {

    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Catalog()
    {
      return View();
    }

    public IActionResult Class(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }


    public IActionResult ClassListings(string subject, string num)
    {
      System.Diagnostics.Debug.WriteLine(subject + num);
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid) {
            var query =
                from s in db.Students
                where s.UId == uid
                join eg in db.EnrollmentGrade
                on s.UId equals eg.UId into enrolled

                from ec in enrolled
                join c in db.Classes
                on ec.CId equals c.ClassId into sClss

                from sc in sClss
                join course in db.Courses
                on sc.CatalogId equals course.CatalogId
                select new {
                    subject = course.Department,
                    number = course.Number,
                    name = course.Name,
                    season = sc.Semester.Substring(0, sc.Semester.Length - 5),
                    year = sc.Semester.Substring(sc.Semester.Length - 4, 4),
                    grade = ec.Grade
                };


            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid) {
            string semester = season + " " + year;

            var classQ =
                from students in db.Students
                where students.UId == uid
                join enrolled in db.EnrollmentGrade
                on students.UId equals enrolled.UId into sEnrolls

                from se in sEnrolls
                join clss in db.Classes
                on se.CId equals clss.ClassId into sClasses

                from sclss in sClasses
                where sclss.Semester == semester
                join courses in db.Courses
                on sclss.CatalogId equals courses.CatalogId into sCourses

                from scourses in sCourses
                where scourses.Department == subject
                && scourses.Number == num
                select new { cId = sclss.ClassId };

            if (classQ.Any()) {
                uint classID = classQ.First().cId;

                var assignmentQ =
                    from clss in db.Classes
                    where clss.ClassId == classID
                    join cats in db.AssignmentCategories
                    on clss.ClassId equals cats.ClassId into clssCats

                    from clat in clssCats
                    join assigns in db.Assignments
                    on clat.CategoryId equals assigns.Category

                    //from clsign in clssAssigns
                    //join submits in db.Submission.DefaultIfEmpty()
                    //on clsign.AId equals submits.AId
                    select new {
                        aname = assigns.Name,
                        cname = clat.Name,
                        due = assigns.DueDate,
                        score = 0
                    };

                if (assignmentQ.Any()) {
                    return Json(assignmentQ.ToArray());
                }

                
            }

            return Json(null);
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
      string category, string asgname, string uid, string contents) {

            string semester = season + " " + year;

            var queryAId =
                from courses in db.Courses
                where courses.Department == subject
                && courses.Number == num
                join clss in db.Classes
                on courses.CatalogId equals clss.CatalogId into offerings

                from offs in offerings
                where offs.Semester == semester
                join cats in db.AssignmentCategories
                on offs.ClassId equals cats.ClassId into classCategories

                from clat in classCategories
                where clat.Name == category
                join assigns in db.Assignments
                on clat.CategoryId equals assigns.Category into categoryAssignments

                from caas in categoryAssignments
                where caas.Name == asgname
                select new { aId = caas.AId };

            if (queryAId.Any()) {
                ulong aid = queryAId.First().aId;
                var hasSubbedQ =
                    from assigns in db.Assignments
                    where assigns.AId == aid
                    join subs in db.Submission
                    on assigns.AId equals subs.AId into allSubmits

                    from a in allSubmits
                    where a.UId == uid
                    select a;

                if (hasSubbedQ.Any()) {
                    foreach (var result in hasSubbedQ) {
                        result.Contents = contents;
                        result.Time = DateTime.Now;
                    }
                    try {
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                } else {
                    Submission sub = new Submission {
                        AId = aid,
                        UId = uid,
                        Time = DateTime.Now,
                        Contents = contents,
                        Score = 0
                    };
                    db.Submission.Add(sub);

                    try {
                        db.SaveChanges();
                        return Json(new { success = true });
                    } catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid) {

            string semester = season + " " + year;

            var query =
                from course in db.Courses
                where course.Department == subject
                && course.Number == num
                join cls in db.Classes
                on course.CatalogId equals cls.CatalogId into classes

                from cls2 in classes
                where cls2.Semester == semester
                join enroll in db.EnrollmentGrade
                on cls2.ClassId equals enroll.CId into enrollments

                from enroll2 in enrollments
                where enroll2.UId == uid
                select new { uID = enroll2.UId };

            if (query.Any()) {
                return Json(new { success = false });
            }
            else {
                var query2 =
                     from course in db.Courses
                     where course.Department == subject
                     && course.Number == num
                     join cls in db.Classes
                     on course.CatalogId equals cls.CatalogId into classes

                     from cls2 in classes
                     where cls2.Semester == semester
                     select new { cID = cls2.ClassId };

                if (query2.Any()) {
                    EnrollmentGrade enroll = new EnrollmentGrade {
                        UId = uid,
                        CId = query2.First().cID,
                        Grade = "--"
                    };

                    db.EnrollmentGrade.Add(enroll);

                    try {
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            return Json(new { success = false });
        }


        Dictionary<string, double> getGradeMap() {
           return new Dictionary<string, double>() {
                { "A", 4.0 }, { "A-", 3.7 },
                { "B+", 3.3 }, { "B", 3.0 }, { "B-", 2.7 },
                { "C+", 2.3 }, { "C", 2.0 }, { "C-", 1.7 },
                { "D+", 1.3 }, { "D", 1.0 }, { "D-", 0.7 }
            };
        }


        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid) {
            var gradeDict = getGradeMap();
            var gradeQ =
                from grades in db.EnrollmentGrade
                where grades.UId == uid
                select grades;

            if (gradeQ.Any()) {
                double credits = gradeQ.Count() * 4;
                double sum = 0;
                foreach (var row in gradeQ) {
                    if (row.Grade != "--") {
                        sum += gradeDict[row.Grade];
                    }
                }
                return Json(new { gpa = (sum * credits) / credits });
            }



            return Json(new { gpa = 0.0 });
        }

        /*******End code to modify********/

    }
}