using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<User, UserReadDto>();
            CreateMap<FeatureGroup, FeatureGroupDto>();

            CreateMap<AttachedFile, AttachedFileUpsertDto>();

            CreateMap<Feature, FeatureDto>()
                .ForMember(dto => dto.PossibleValues, options => options
                .MapFrom(dao => ProductHelper.Convert<string>(dao.PossibleValues)));

            CreateMap<Product, ProductReadDto>()
                .ForMember(dto => dto.Features, options => options
                .MapFrom(dao => dao.FeaturesValues.Select(y => new FeatureValueReadDto()
                { Name = y.Feature.Name, ValueId = y.Id, Value = y.Value })));
                //.MapFrom(dao => dao.FeatureGroup.Features.Select(x => x.FeatureValues
                //.Select(y => new FeatureValueReadDto() { Name = x.Name, ValueId = y.Id, Value = y.Value }))));

            CreateMap<Category, CategoryDto>();
            CreateMap<Subcategory, SubcategoryDto>();
            CreateMap<FeatureValue, FeatureValueReadDto>()
                .ForMember(x => x.ValueId, options => options.MapFrom(dao => dao.Id))
                .ForMember(x => x.Name, options => options.MapFrom(dao => dao.Feature.Name));
        }
    }
}
