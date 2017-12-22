using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.ModelConfiguration
{
    public class ModelMappings
    {
        public void MapModels()
        {
            AutoMapper.Mapper.Initialize(cfg =>
            {
                //map the get dto
                cfg.CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                src.DateOfBirth.GetCurrentAge()));

                cfg.CreateMap<Book, BookDto>();

                //map the input Dto
                cfg.CreateMap<AuthorInputDto, Author>();

                cfg.CreateMap<BookInputDto, Book>();

                cfg.CreateMap<BookUpdateDto, Book>();

                cfg.CreateMap<Book, BookUpdateDto>();
            });
        }
    }
}
