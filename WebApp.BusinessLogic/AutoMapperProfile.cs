using AutoMapper;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models.PostReplyModels;
using WebApp.BusinessLogic.Models.ThreadModels;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.DataAccess.Entities;

namespace WebApp.BusinessLogic;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        _ = this.CreateMap<Post, PostModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Thread, opt => opt.Ignore());

        _ = this.CreateMap<Post, PostCreateModel>();

        _ = this.CreateMap<PostCreateModel, Post>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        _ = this.CreateMap<PostReply, PostReplyModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Post, opt => opt.Ignore());

        _ = this.CreateMap<PostReply, PostReplyCreateModel>();

        _ = this.CreateMap<PostReplyCreateModel, PostReply>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        _ = this.CreateMap<ForumThread, ForumThreadModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Posts, opt => opt.Ignore());

        _ = this.CreateMap<ForumThread, ForumThreadCreateModel>().ReverseMap();

        _ = this.CreateMap<ApplicationUser, LoginModel>().ReverseMap();

        _ = this.CreateMap<ApplicationUser, RegistrationModel>().ReverseMap();

        _ = this.CreateMap<ApplicationUser, UserModel>().ReverseMap();
    }
}
