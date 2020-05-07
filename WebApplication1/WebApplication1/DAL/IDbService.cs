using System.Collections.Generic;
using tut5.Models;

namespace tut5.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}
