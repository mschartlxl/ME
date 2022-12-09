using AutoMapper;
using ME.BioSoft.Model;
using ME.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BioSoft.AutoMapper
{
    public class OrginalProfile : Profile
    {
        public OrginalProfile()
        {
            CreateMap<PlatformActionDTO, PlatformAction>().ReverseMap();
            CreateMap<SystemSetDTO, SystemSet>().ReverseMap();

        }
    }
}
