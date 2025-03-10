using authProject.helper;
using authProject.Model;
using clearAccess.Repos;
using clearAccess.Repos.Models;
using authProject.Service;
using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace authProject.Container
{
    public class CustomerService : ICustomerService
    {
        private readonly LearndataContext context;
         
        private readonly IMapper mapper;    //inject the mapper

        private readonly ILogger<CustomerService> logger;  //inject the logger

        public CustomerService(LearndataContext context, IMapper mapper, ILogger<CustomerService>logger)  //pass mapper parameter,pass logger parameter
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<APIResponse> Create(CustomerModel data)
        {
            APIResponse response = new APIResponse();
            try
            {
                this.logger.LogInformation("Create Begins");      //log msg
                Customer _customer = this.mapper.Map<CustomerModel,Customer>(data);
                await this.context.Customers.AddAsync(_customer);
                await this.context.SaveChangesAsync();
                response.ResponseCode = 201;
                response.Result = data.Code;

            }
            catch (Exception ex) 
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
                this.logger.LogError(ex.Message,ex);  //log msg

            }
            return response;
        }

        public async Task<List<CustomerModel>> Getbycode()
        {
            List<CustomerModel> _response = new List<CustomerModel>();     // Initialize an empty list of CustomerModel
            var _data = await this.context.Customers.ToListAsync();                       // Retrieve all customer records from the database.

            if (_data != null) 
            {
                _response = this.mapper.Map<List<Customer>, List<CustomerModel>>(_data);  // Map List<Customer> to List<CustomerModel>.
            }
            return _response;                                                        // Return the mapped list.
        }

        public async  Task<CustomerModel> Getbycode(string code)
        {
            CustomerModel _response = new CustomerModel();     
            var _data = await this.context.Customers.FindAsync(code);                     

            if (_data != null)
            {
                _response = this.mapper.Map<Customer,CustomerModel>(_data);  
            }
            return _response;
        }

        public async Task<APIResponse> Remove(string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this.context.Customers.FindAsync(code);
                if(_customer != null) 
                {
                    this.context.Customers.Remove(_customer);
                    await this.context.SaveChangesAsync();
                    response.ResponseCode = 201;
                    response.Result = code;
                }
                else 
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Data not Found";
                }
               

            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;

            }
            return response;
        }

        public async Task<APIResponse> Update(CustomerModel data, string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this.context.Customers.FindAsync(code);
                if (_customer != null)
                {
                    _customer.Name = data.Name;
                    _customer.Email = data.Email;
                    _customer.Phone = data.Phone;
                    _customer.Taxcode = data.Taxcode;
                    _customer.IsActive = data.IsActive;
                    _customer.Creditlimit = data.Creditlimit;

                    await this.context.SaveChangesAsync();
                    response.ResponseCode = 201;
                    response.Result = code;
                }
                else
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Data not Found";
                }


            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;

            }
            return response;
        }
    }
}
