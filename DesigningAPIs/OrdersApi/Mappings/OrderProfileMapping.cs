﻿using AutoMapper;
using OrdersApi.Data.Domain;
using OrdersApi.Models;

namespace OrdersApi.Mappings
{
    public class OrderProfileMapping : Profile
    {
        public OrderProfileMapping()
        {
            CreateMap<OrderItemModel, OrderItem>();
            CreateMap<CustomerModel, Customer>();
            CreateMap<OrderModel, Order>().
                ForMember(dest => dest.Customer, opt => opt.MapFrom(src => new Customer()
                {
                    Email = src.Email,
                    Phone = src.Phone,
                    Name = src.Name
                }))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }

}
