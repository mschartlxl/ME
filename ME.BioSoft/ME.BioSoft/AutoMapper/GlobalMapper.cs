using AutoMapper;
using ME.BaseCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BioSoft.AutoMapper
{
    public class GlobalMapper
    {
        private static GlobalMapper _instance;
        public static GlobalMapper Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new GlobalMapper();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 对象映射
        /// </summary>
        public IMapper mapper;
        private GlobalMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrginalProfile>();
            });
            mapper = config.CreateMapper();
        }
    }
}
