using QL_LICHHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Repositories
{
    public class ScheduleTypeRepository
    {
        private QL_LICHHOPDataContext _context;
        public ScheduleTypeRepository()
        {
            _context = new QL_LICHHOPDataContext();
        }
        public List<ScheduleType> GetScheduleTypes()
        {
            return _context.ScheduleTypes.ToList();
        }
    }
}