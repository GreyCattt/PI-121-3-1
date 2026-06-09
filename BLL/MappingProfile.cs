using AutoMapper;
using DAL.Entities;
using BLL.DTOs;
using System.Linq;

namespace BLL
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Lot, LotDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SellerUsername, opt => opt.MapFrom(src => src.Seller.Username))
                .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src =>
                    src.Bids.Any() ? src.Bids.Max(b => b.Amount) : src.StartingPrice));

            CreateMap<LotCreateDto, Lot>();

            CreateMap<Bid, BidDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));
        }
    }
}