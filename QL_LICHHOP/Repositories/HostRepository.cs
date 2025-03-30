using QL_LICHHOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Repositories
{
    public class HostRepository
    {
        private QL_LICHHOPDataContext _context;
        public HostRepository()
        {
            _context = new QL_LICHHOPDataContext();
        }
        public List<MeetingHost> GetMeetingHosts()
        {
            return _context.MeetingHosts.ToList();
        }
    }
}