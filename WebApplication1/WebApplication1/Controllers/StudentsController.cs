using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using tut5.DAL;
using tut5.Models;

namespace tut5.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            List<Student> students = new List<Student>();
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18376;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "Select FirstName, LastName, BirthDate, Name, Semester From Enrollment, Student, Studies Where Enrollment.IdStudy = Studies.IdStudy AND Enrollment.IdEnrollment = Student.IdEnrollment; ";

                    connection.Open();
                    var dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Student student = new Student
                        {
                            FirstName = dataReader["FirstName"].ToString(),
                            LastName = dataReader["LastName"].ToString(),
                            BirthDate = dataReader["BirthDate"].ToString(),
                            enrollment = new Enrollment
                            {
                                IdSemester = (int)dataReader["Semester"],
                                study = new Studies { Name = dataReader["Name"].ToString() }
                            }
                        };
                        students.Add(student);
                    }
                }
            }
            return Ok(students);
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {
            Enrollment enroll = new Enrollment();
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18376;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "select Semester from Student, Enrollment, Studies Where Student.IndexNumber=@id AND Enrollment.IdStudy = Studies.IdStudy AND Enrollment.IdEnrollment = Student.IdEnrollment";
                    command.Parameters.AddWithValue("id", id);
                    connection.Open();
                    var dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        enroll.IdSemester = (int)dataReader["Semester"];
                    }
                }
            }
            return Ok(enroll);
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 5000)}";
            return Ok(student);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Deleted Completed");
        }

        [HttpPut("{id}")]
        public IActionResult PutStudent(int id)
        {
            return Ok("Update completed");
        }
    }
}