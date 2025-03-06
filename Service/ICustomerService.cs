using authProject.helper;
using authProject.Model;
using authProject.Repos.Models;

namespace authProject.Service
{
    public interface ICustomerService
    {
        Task<List<CustomerModel>> Getbycode();
        Task<CustomerModel> Getbycode(string code);
        Task<APIResponse> Remove(string code);
        Task<APIResponse> Create(CustomerModel data);
        Task<APIResponse> Update(CustomerModel data,string code);

    }
}
