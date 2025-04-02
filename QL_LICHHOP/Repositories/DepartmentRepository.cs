using QL_LICHHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Repositories
{
    public class DepartmentRepository
    {
        private QL_LICHHOPDataContext _context;
        public DepartmentRepository()
        {
            _context = new QL_LICHHOPDataContext();
        }
        public List<Department> GetDepartments()
        {
            return _context.Departments.ToList();
        }
        public Department GetDepartmentById(int id)
        {
            return _context.Departments.FirstOrDefault(d => d.DepartmentID == id);
        }
    }
}