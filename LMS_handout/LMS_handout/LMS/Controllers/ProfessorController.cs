﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers {
    [Authorize(Roles = "Professor")]
    public class ProfessorController : CommonController {
        public IActionResult Index() {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid) {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year) {

            //may need to check for case sensativity in season variable 
            string semester = season + ' ' + year;
      

            //query from join of Courses for department and number
            var query =
                from c in db.Courses
                where c.Department == subject &&
                c.Number == num
                join clss in db.Classes on
                c.CatalogId equals clss.CatalogId
                into combined

                from cmb in combined
                where cmb.Semester == semester
                join e in db.EnrollmentGrade
                on cmb.ClassId equals e.CId into gradeTable

                from g in gradeTable
                join s in db.Students
                on g.UId equals s.UId

                select new { fname = s.FirstName, lname = s.LastName, uid = s.UId, dob = s.Dob, grade = g.Grade };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category) {

            List<Object> assignnmentsList = new List<Object>();
            string semster = season + ' ' + year;
            if (category != null) {
                var query =
                    from assigns in db.Assignments
                    join cats in db.AssignmentCategories
                    on assigns.Category equals cats.CategoryId into assignmentsInCategory

                    from acats in assignmentsInCategory
                    where acats.Name == category
                    join clss in db.Classes
                    on acats.ClassId equals clss.ClassId into assignmentsInClass

                    from aclss in assignmentsInClass
                    where aclss.Semester == semster
                    join cr in db.Courses
                    on aclss.CatalogId equals cr.CatalogId into assignmentsInSemester

                    from asem in assignmentsInSemester
                    where asem.Number == num &&
                    asem.Department == subject

                    select new { aname = assigns.Name, cname = acats.Name, due = assigns.DueDate, aID = assigns.AId};

                foreach (var t in query) {
                    var submissionQuery =
                        from s in db.Submission
                        where s.AId == t.aID
                        select s;

                    int submissions = 0;

                    if (submissionQuery.Any()) {
                        submissions = submissionQuery.Count();
                    }

                    assignnmentsList.Add(new { t.aname, t.cname, t.due, submissions });
                }

                return Json(assignnmentsList.ToArray());
                   
            }
            else {

                var query =
                    from assigns in db.Assignments
                    join cats in db.AssignmentCategories
                    on assigns.Category equals cats.CategoryId into assignmentsInCategory

                    from acats in assignmentsInCategory
                    join clss in db.Classes
                    on acats.ClassId equals clss.ClassId into assignmentsInClass

                    from aclss in assignmentsInClass
                    where aclss.Semester == semster
                    join cr in db.Courses
                    on aclss.CatalogId equals cr.CatalogId into assignmentsInSemester

                    from asem in assignmentsInSemester
                    where asem.Number == num &&
                    asem.Department == subject

                    select new { aname = assigns.Name, cname = acats.Name, due = assigns.DueDate, aID = assigns.AId };

                foreach (var t in query) {
                    var submissionQuery =
                        from s in db.Submission
                        where s.AId == t.aID
                        select s;

                    int submissions = 0;

                    if (submissionQuery.Any()) {
                        submissions = submissionQuery.Count();
                    }

                    assignnmentsList.Add(new { t.aname, t.cname, t.due, submissions });
                }
                return Json(assignnmentsList.ToArray());

            }


        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year) {

            string semester = season + ' ' + year;

            var query =
                from c in db.Courses
                where c.Department == subject &&
                c.Number == num
                join cls in db.Classes
                on c.CatalogId equals cls.CatalogId into dataTable

                from data in dataTable
                where data.Semester == semester
                join a in db.AssignmentCategories
                on data.ClassId equals a.ClassId

                select new { name = a.Name, weight = a.Weight };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight) {

            string semester = season + ' ' + year;
            var query =
                from crs in db.Courses
                where crs.Department == subject &&
                crs.Number == num
                join cls in db.Classes
                on crs.CatalogId equals cls.CatalogId

                select new { classID = cls.ClassId };

            if (query.Any()) {

                uint clsID = query.First().classID;

                var catagoryQuery =
                    from cl in db.Classes
                    where cl.ClassId == clsID
                    join asmts in db.AssignmentCategories
                    on cl.ClassId equals asmts.ClassId into joined

                    from jnd in joined
                    where jnd.Name == category
                    select jnd;

                if (!catagoryQuery.Any()) {
                    AssignmentCategories assignmentCatgories = new AssignmentCategories {
                        Name = category,
                        Weight = (byte)catweight,
                        ClassId = clsID
                    };
                    db.AssignmentCategories.Add(assignmentCatgories);

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
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents) {

            string semester = season + ' ' + year;
            var query =
                from c in db.Courses
                where c.Department == subject &&
                c.Number == num
                join cls in db.Classes
                on c.CatalogId equals cls.CatalogId

                select new { classID = cls.ClassId };

            if (query.Any()) {
                uint clsID = query.First().classID;

                var assignmentQuery =
                    from c in db.Classes
                    where c.ClassId == clsID
                    join asg in db.AssignmentCategories
                    on c.ClassId equals asg.ClassId into joined

                    from jnd in joined
                    where jnd.Name == category
                    select jnd;
                if (assignmentQuery.Any()) {
                    var categoryId = assignmentQuery.First().CategoryId;
                    Assignments assignment = new Assignments {
                        Category = categoryId,
                        Name = asgname,
                        Points = (uint) asgpoints,
                        DueDate = asgdue,
                        Contents = asgcontents
                    };

                    db.Assignments.Add(assignment);

                    try {
                        //need the updated code to finish this portion 
                        db.SaveChanges();

                        var enrollmentQuery =
                            from e in db.EnrollmentGrade
                            where e.CId == clsID
                            select e;

                        if (enrollmentQuery.Any()) {

                            foreach (var enrollment in enrollmentQuery) {
                                enrollment.Grade = getGradeForClass(enrollment.UId, clsID);
                            }

                            db.SaveChanges();
                        }

                        return Json(new { success = true });
                    } catch (Exception e){
                        Console.WriteLine(e.Message);
                    }
                }
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname) {

            string semester = season + " " + year;

            var query =
                from crs in db.Courses
                where crs.Number == num
                && crs.Department == subject
                join clss in db.Classes
                on crs.CatalogId equals clss.CatalogId into crsOfferings

                from offs in crsOfferings
                where offs.Semester == semester
                join cats in db.AssignmentCategories
                on offs.ClassId equals cats.ClassId into clssCategories

                from clscat in clssCategories
                where clscat.Name == category
                join assigns in db.Assignments
                on clscat.CategoryId equals assigns.Category into catAssignments

                from catsign in catAssignments
                where catsign.Name == asgname
                join subs in db.Submission
                on catsign.AId equals subs.AId into allSubmissions

                from alsubs in allSubmissions
                join studs in db.Students
                on alsubs.UId equals studs.UId
                select new { fname = studs.FirstName, lname = studs.LastName, uid = studs.UId, time = alsubs.Time, score = alsubs.Score };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score) {

            string semester = season + " " + year;

            var query =
              from crs in db.Courses
              where crs.Number == num
              && crs.Department == subject
              join clss in db.Classes
              on crs.CatalogId equals clss.CatalogId into crsOfferings

              from offs in crsOfferings
              where offs.Semester == semester
              join cats in db.AssignmentCategories
              on offs.ClassId equals cats.ClassId into clssCategories

              from clscat in clssCategories
              where clscat.Name == category
              join assigns in db.Assignments
              on clscat.CategoryId equals assigns.Category into catAssignments

              from catsign in catAssignments
              where catsign.Name == asgname
              join subs in db.Submission
              on catsign.AId equals subs.AId into allSubmissions

              from alsubs in allSubmissions
              where alsubs.UId == uid
              select alsubs;

            if (query.Any()) {
                var result = query.First();
                result.Score = (uint) score;

                try {
                    db.SaveChanges();

                    var classIdQuery =
                        from crs in db.Courses
                        where crs.Department == subject
                        && crs.Number == num
                        join clss in db.Classes
                        on crs.CatalogId equals clss.CatalogId into offerings

                        from offs in offerings
                        where offs.Semester == semester
                        select new { cid = offs.ClassId };

                    uint classId = classIdQuery.First().cid;

                    var enrollmentQuery =
                        from e in db.EnrollmentGrade
                        where e.CId == classId
                        && e.UId == uid
                        select e;

                    var curGrade = enrollmentQuery.First();
                    curGrade.Grade = getGradeForClass(uid, classId);

                    db.SaveChanges();

                    return Json(new { success = true });
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid) {

            var query =
                from p in db.Professors
                where p.UId == uid
                join c in db.Classes
                on p.UId equals c.Professor into joined

                from jnd in joined
                join crs in db.Courses
                on jnd.CatalogId equals crs.CatalogId

                select new {
                    subject = crs.Department,
                    number = crs.Number,
                    name = crs.Name,
                    season = jnd.Semester.Substring(0, jnd.Semester.Length - 5),
                    year = jnd.Semester.Substring(jnd.Semester.Length - 4)
                };
            return Json(query.ToArray());
        }



        // AUTO GRADE FUNCTION

        string getGradeForClass(string uid, uint classID) {
            var query =
                //from crs in db.Courses
                //where crs.Department == subject
                //&& crs.Number == num
                //join clss in db.Classes
                //on crs.CatalogId equals clss.CatalogId into offerings

                //from offs in offerings
                //where offs.Semester == semester
                from clss in db.Classes
                where clss.ClassId == classID
                join cats in db.AssignmentCategories
                on clss.ClassId equals cats.ClassId into ClassCategories

                from clsscats in ClassCategories
                join assigns in db.Assignments
                on clsscats.CategoryId equals assigns.Category into ClassAssignments

                from clssassigns in ClassAssignments
                join subs in db.Submission
                on clssassigns.AId equals subs.AId into AssignmentSubmissions

                from asubs in AssignmentSubmissions
                where asubs.UId == uid
                select new { weight = clsscats.Weight, category = clsscats.Name, score = asubs.Score, points = clssassigns.Points };

            if (query.Any()) {
                List<CategoryDetails> categories = new List<CategoryDetails>();
                foreach (var result in query) {
                    bool isNotInList = true;
                    foreach (CategoryDetails cd in categories) {
                        if (cd.name == result.category) {
                            cd.addSubmission(result.score, result.points);
                            isNotInList = false;
                        }
                    }
                    if (isNotInList) {
                        categories.Add(new CategoryDetails(result.category, result.weight));
                        categories.Last().addSubmission(result.score, result.points);
                    }
                }

                double weightedTotal = 0.0;
                foreach (CategoryDetails cd in categories) {
                    weightedTotal += cd.getWeightedGrade();
                }

                int classPercentage = (int) weightedTotal;

                if (classPercentage > 93) {
                    return "A";
                }
                if(classPercentage > 90) {
                    return "A-";
                }
                if(classPercentage > 87) {
                    return "B+";
                }
                if(classPercentage > 83) {
                    return "B";
                }
                if (classPercentage > 80) {
                    return "B-";
                }
                if (classPercentage > 77) {
                    return "C+";
                }
                if (classPercentage > 73) {
                    return "C";
                }
                if (classPercentage > 70) {
                    return "C-";
                }
                if (classPercentage > 67) {
                    return "D+";
                }
                if (classPercentage > 63) {
                    return "D";
                }

                return "D-";
            }

            return "--";
        }



        /*******End code to modify********/

    }
    public class CategoryDetails : ProfessorController {
        public string name;
        public double totalPoints, totalScore;
        public double weightInDecimal;
        public CategoryDetails(string categoryName, int weight) {
            name = categoryName;
            totalPoints = 0;
            totalScore = 0;
            weightInDecimal = weight / 100.0;
        }
        public void addSubmission(uint score, uint points) {
            totalScore += score;
            totalPoints += points;
        }

        public double getWeightedGrade() {
            return ((totalScore / totalPoints) * 100.0) * weightInDecimal;
        }
    }
}