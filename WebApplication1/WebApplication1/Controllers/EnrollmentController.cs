using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using tut5.DAL;
using tut5.DTOs.Requests;
using tut5.DTOs.Responces;

namespace tut5.Controllers
{
    [Route("api/enrollment")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IDbService _dbService;

        public EnrollmentController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost(Name = "EnrollStudent")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponce response = new EnrollStudentResponce();
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18376;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "Select * From Studies Where Name = @Name";
                    command.Parameters.AddWithValue("Name", request.Studies);
                    connection.Open();

                    var transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    var dataReader = command.ExecuteReader();

                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        transaction.Rollback();
                        return BadRequest("Specified studies does not exist");
                    }

                    int idStudy = (int)dataReader["IdStudy"];

                    dataReader.Close();

                    command.CommandText = "Select * From Enrollment Where Semester = 1 And IdStudy = @idStudy";
                    int IdEnrollment = (int)dataReader["IdEnrollemnt"] + 1;
                    command.Parameters.AddWithValue("IdStudy", idStudy);
                    dataReader = command.ExecuteReader();

                    if (dataReader.Read())
                    {
                        dataReader.Close();
                        command.CommandText = "Select MAX(idEnrollment) as 'idEnrollment' From Enrollment";
                        dataReader = command.ExecuteReader();
                        dataReader.Close();
                        DateTime StartDate = DateTime.Now;
                        command.CommandText = "Insert Into Enrollment(IdEnrollment, Semester, IdStudy, StartDate) Values (@IdEnrollemnt, 1, @IdStudy, @StartDate)";
                        command.Parameters.AddWithValue("IdEnrollemnt", IdEnrollment);
                        command.Parameters.AddWithValue("IdStudy", idStudy);
                        command.Parameters.AddWithValue("StartDate", StartDate);
                        command.ExecuteNonQuery();
                    }
                    dataReader.Close();
                    command.CommandText = "Select * From Student Where IndexNumber=@IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        command.CommandText = "Insert Into Student(IndexNumber, FirstName, LastName, Birthdate, IdEnrollment) Value (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                        command.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                        command.Parameters.AddWithValue("FirstName", request.FirstName);
                        command.Parameters.AddWithValue("LastName", request.LastName);
                        command.Parameters.AddWithValue("BirthDate", request.BirthDate);
                        command.Parameters.AddWithValue("IdEnrollment", IdEnrollment);
                        command.ExecuteNonQuery();
                        dataReader.Close();
                        response.Semester = 1;
                    }
                    else
                    {
                        dataReader.Close();
                        transaction.Rollback();
                        return BadRequest("You can't add student with the same index number");
                    }
                    transaction.Commit();
                }
            }
            return Created("EnrollStudent", response);
        }
    }
}