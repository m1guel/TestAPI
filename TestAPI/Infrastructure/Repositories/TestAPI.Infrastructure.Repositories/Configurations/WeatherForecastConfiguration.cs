using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using TestAPI.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TestAPI.Infrastructure.Repositories.SqlServer.Configurations
{
    public class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecast>
    {
        public void Configure(EntityTypeBuilder<WeatherForecast> entity)
        {
            entity.Property(e => e.EntityKey)
                    .UseHiLo();

            entity.Property(e => e.Summary).HasMaxLength(100);
        }
    }
}
