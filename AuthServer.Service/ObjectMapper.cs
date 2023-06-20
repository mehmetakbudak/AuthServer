using AutoMapper;
using System;

namespace AuthServer.Service
{
    public static class ObjectMapper
    {
        // ilk başta değil de ihtiyaç olursa yüklenmesini istediğimiz zaman Lazy kullanabiliriz.
        private static readonly Lazy<IMapper> lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DtoMapper>();
            });

            return config.CreateMapper();
        });

        public static IMapper Mapper => lazy.Value;
    }
}
