using authProject.Model;
using authProject.Repos.Models;
using AutoMapper;

namespace authProject.helper
{
    public class AutoMapperHandler: Profile
    {
        public AutoMapperHandler()
        {
            CreateMap<Customer, CustomerModel>().ForMember(item => item.StatusName, opt => opt.MapFrom(
                item => (item.IsActive.HasValue&&item.IsActive.Value)? "Active" : "In active")).ReverseMap();

        }
    }
}
