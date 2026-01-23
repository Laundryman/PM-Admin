using AutoMapper;
using PMApplication.Dtos;
using PMApplication.Dtos.Categories;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Dtos.StandTypes;
using PMApplication.Entities;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.OrderAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
using PMApplication.Entities.ProductAggregate;
using PMApplication.Entities.StandAggregate;
using PMApplication.Specifications.Filters;

namespace PM_AdminApp.Server.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BrandFilterDto, BrandFilter>();
            CreateMap<Brand, BrandDto>();
            CreateMap<BrandDto, Brand>();

            CreateMap<CategoryFilterDto, CategoryFilter>();
            CreateMap<Category, CategoryDto>();

            CreateMap<CategoryFilterDto, CategoryFilter>();
            CreateMap<Category, ParentCategoryDto>();
            //CreateMap<Category, CategoryMenuDto>();
            CreateMap<Cluster, PlanmClusterDto>();
            //CreateMap<ClusterPart, PlanmPartInfo>();
            //CreateMap<ClusterShelf, PlanmPartInfo>();
            CreateMap<CountriesFilterDto, CountryFilter>();
            CreateMap<Country, CountryDto>();
            CreateMap<CountryDto, Country>();

            CreateMap<JobFolderDto, JobFolder>();

            CreateMap<Order, OrderDto>();

            CreateMap<PartFilterDto, PartFilter>();
            CreateMap<PartFilter, PartFilterDto>();
            CreateMap<Part, PartListDto>();
            CreateMap<Part, PartDto>();
            CreateMap<PartDto, Part>();

            CreateMap<Planogram, PlanmPlanogramDto>();
            CreateMap<PlanmPlanogramDto, Planogram>();
            CreateMap<PlanmPartFacing, PlanogramPartFacing>();

            CreateMap<PlanmMenuPart, CategoryMenuDto>();

            CreateMap<Product, PlanmProductDto>();
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<Product, ProductListDto>();
            CreateMap<Product, FullProductDto>();
            CreateMap<ProductFilterDto, ProductFilter>();

            CreateMap<RegionsFilterDto, RegionFilter>();
            CreateMap<Region, RegionDto>();

            CreateMap<ShadeFilterDto, ShadeFilter>();
            CreateMap<Shade, ShadeDto>();
            CreateMap<ShadeDto, Shade>();
            CreateMap<PlanmShadeDto, Shade>();
            CreateMap<Shade, PlanmShadeDto>();

            CreateMap<PlanmStandDto, Stand>();
            //CreateMap<Stand, PlanmStandDto>();
            CreateMap<PlanmStandColumnDto, StandColumn>();
            CreateMap<PlanmStandRowDto, StandRow>();
            //CreateMap<StandRow, PlanmStandRowDto>();
            CreateMap<StandColumn, PlanmStandColumnDto>();
            CreateMap<StandColumnUpright, PlanmStandColumnUprightDto>();
            CreateMap<StandRow, PlanmStandRowDto>();
            CreateMap<StandType, ParentStandTypeDto>();
            CreateMap<StandType, StandTypeDto>();
            CreateMap<StandTypeDto, StandType>();
            CreateMap<StandTypeFilterDto, StandTypeFilter>();
            CreateMap<Sku, ExportSkuDto>();


        }
    }
}
