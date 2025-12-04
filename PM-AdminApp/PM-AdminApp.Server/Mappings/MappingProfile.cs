using AutoMapper;
using PMApplication.Dtos;
using PMApplication.Dtos.Filters;
using PMApplication.Dtos.PlanModels;
using PMApplication.Entities;
using PMApplication.Entities.ClusterAggregate;
using PMApplication.Entities.CountriesAggregate;
using PMApplication.Entities.JobsAggregate;
using PMApplication.Entities.OrderAggregate;
using PMApplication.Entities.PartAggregate;
using PMApplication.Entities.PlanogramAggregate;
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

            CreateMap<CategoryFilterDto, CategoryFilter>();
            CreateMap<Category, CategoryDto>();

            CreateMap<CategoryFilterDto, CategoryFilter>();
            CreateMap<Category, CategoryDto>();
            CreateMap<Category, CategoryMenuDto>();
            CreateMap<Cluster, PlanmClusterDto>();
            CreateMap<CountriesFilterDto, CountryFilter>();
            CreateMap<Country, CountryDto>();

            CreateMap<JobFolderDto, JobFolder>();

            CreateMap<Order, OrderDto>();

            CreateMap<PartFilterDto, PartFilter>();
            CreateMap<PartFilter, PartFilterDto>();
            CreateMap<Part, PartListDto>();
            CreateMap<Part, PartDto>();

            CreateMap<Planogram, PlanmPlanogramDto>();
            CreateMap<PlanmPlanogramDto, Planogram>();
            CreateMap<PlanmPartFacing, PlanogramPartFacing>();


            CreateMap<Product, PlanmProductDto>();
            CreateMap<Product, ProductDto>();
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
            CreateMap<Stand, PlanmStandDto>();
            CreateMap<PlanmStandColumnDto, StandColumn>();
            CreateMap<PlanmStandRowDto, StandRow>();
            CreateMap<StandRow, PlanmStandRowDto>();
            CreateMap<StandColumn, PlanmStandColumnDto>();
            CreateMap<StandRow, PlanmStandRowDto>();
            CreateMap<Sku, ExportSkuDto>();


        }
    }
}
