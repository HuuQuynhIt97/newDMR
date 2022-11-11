using DMR_API.DTO;
using DMR_API.Models;
using AutoMapper;
using System;
using System.Linq;
using dmr_api.Models;

namespace DMR_API.Helpers.AutoMapper
{
    public class DtoToEfMappingProfile : Profile
    {
        public DtoToEfMappingProfile()
        {
            var ct = DateTime.Now;


            CreateMap<DispatchTodolistDto, Dispatch>();
            CreateMap<UserForDetailDto, User>();
            CreateMap<GlueDto, Glue>();
            CreateMap<GlueCreateDto, Glue>();
            CreateMap<GlueCreateDto1, Glue>();
            CreateMap<IngredientDto, Ingredient>()
            .ForMember(d => d.VOC, o => o.MapFrom(x => x.VOC.ToDouble().ToSafetyString()))
            .ForMember(d => d.Unit, o => o.MapFrom(x => x.Unit.ToDouble().ToSafetyString()))
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.GlueTypeID, o => o.MapFrom(x => x.GlueTypeID == 0 ? null : x.GlueTypeID))
            .ForMember(d => d.GlueType, o => o.Ignore());
            CreateMap<IngredientForImportExcelDto, Ingredient>();
            CreateMap<IngredientDto1, Ingredient>()
            .ForMember(d => d.VOC, o => o.MapFrom(x => x.VOC.ToDouble().ToSafetyString()))
            .ForMember(d => d.GlueType, o => o.Ignore())
            .ForMember(d => d.GlueTypeID, o => o.MapFrom(x => x.GlueTypeID == 0 ? null : x.GlueTypeID))
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.Unit, o => o.MapFrom(x => x.Unit.ToDouble().ToSafetyString()));

            CreateMap<LineDto, Line>();

            CreateMap<LunchTimeDto, LunchTime>(); 
            CreateMap<GlueIngredientForMapDto, GlueIngredient>();
            CreateMap<ModelNameDto, ModelName>();
            CreateMap<PlanDto, Plan>();
            CreateMap<StationDto, Station>().ForMember(d => d.CreateTime, o => o.MapFrom(x => DateTime.Now.ToLocalTime()));

            CreateMap<ModelNoDto, ModelNo>();
            CreateMap<UserDetail, UserDetailDto>();
            CreateMap<SuppilerDto, Supplier>();
            CreateMap<ArticleNoDto, ArticleNo>();
            CreateMap<BuildingDto, Building>()
            .ForMember(d => d.Kind, o => o.Ignore())
            .ForMember(d => d.BuildingType, o => o.Ignore())
            .ForMember(d => d.LunchTime, o => o.Ignore())
            .ForMember(d => d.PeriodMixingList, o => o.Ignore())
            .ForMember(d => d.LunchTimeID, o => o.MapFrom(x => x.LunchTimeID == 0 || x.LunchTimeID == null ? null : x.LunchTimeID))
            .ForMember(d => d.KindID, o => o.MapFrom(x => x.KindID == 0 || x.KindID == null ? null : x.KindID))
            .ForMember(d => d.BuildingTypeID, o => o.MapFrom(x => x.BuildingTypeID == 0 || x.BuildingTypeID == null ? null : x.BuildingTypeID))
            .ForMember(d => d.ParentID, o => o.MapFrom(x => x.ParentID == 0 || x.ParentID == null ? null : x.ParentID));

            CreateMap<SubpackageDto, Subpackage>();
            CreateMap<BuildingUserDto, BuildingUser>();
            CreateMap<CommentDto, Comment>();
            CreateMap<BPFCEstablishDto, BPFCEstablish>();
            CreateMap<ArtProcessDto, ArtProcess>();
            CreateMap<ProcessDto, Process>();
            CreateMap<KindDto, Kind>()
                .ForMember(d => d.KindType, o => o.Ignore());
            CreateMap<PartDto, Part>();
            CreateMap<RoleDto, Role>();
            CreateMap<MaterialDto, Material>();
            CreateMap<ToDoListDto, ToDoList>();
            CreateMap<DispatchListDto, DispatchList>();
            CreateMap<MixingInfoDto, MixingInfo>();
            CreateMap<MixingInfo, MixingInfoForCreateDto>();
            CreateMap<IngredientInfo, IngredientInfoDto>();
            CreateMap<IngredientInfoReport, IngredientInfoReportDto>();
            CreateMap<SettingDTO, Setting>();
            CreateMap<MixingInfoDetail, MixingInfoDetailForAddDto>();
            CreateMap<StirDTO, Stir>();
            CreateMap<Plan, PlanForCloneDto>();
            CreateMap<ScaleMachine, ScaleMachineDto>();

            CreateMap<MailingDto, Mailing>();


            //CreateMap<AuditTypeDto, MES_Audit_Type_M>();
        }
    }
}